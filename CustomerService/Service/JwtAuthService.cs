using dbMovies.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SqlServer.Server;
using System;
using System.Data;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;


public class JwtAuthService
{
    private readonly string? _key;
    private readonly string? _issuer;
    private readonly string? _audience;
    private readonly dbMoviesContext _context;
    public JwtAuthService(IConfiguration Configuration, dbMoviesContext context)
    {
        _key = Configuration["jwt:Secret-Key"];
        _issuer = Configuration["jwt:Issuer"];
        _audience = Configuration["jwt:Audience"];
        _context = context;

    }

    public string GenerateToken(UserCustomer userAfterVerifyPass)
    {

        //Hàm GenerateToken(User userSauKhiVerifyPass) sinh ra một chuỗi JWT token(JSON Web Token) để client dùng khi gọi API.
        //Token sẽ chứa:
        //Thông tin người dùng(username, email, role, …).
        //Thời gian sống(expiration).
        //Chữ ký số để chống giả mạo.

        // Khóa bí mật để ký token
        var key = Encoding.ASCII.GetBytes(_key);
        // _key lấy từ file appsettings.json("jwt:Secret-Key").
        //Chuyển thành mảng byte để sau này ký token bằng thuật toán HMAC-SHA256.
        // Ví dụ: nếu Secret-Key = "abc123", thì sau bước này key = [97, 98, 99, 49, 50, 51].

        // Tạo danh sách các claims cho token
        var claims = new List<Claim>
        {
            new Claim("UserId", userAfterVerifyPass.Id.ToString()),          // 🔥 Thêm UserId
            new Claim("UserName", userAfterVerifyPass.Username),              // Claim mặc định cho username
            //new Claim("Email", userAfterVerifyPass.Email),               // Claim mặc định cho username
            // new Claim(ClaimTypes.Role, userLogin.Role),                   // Claim mặc định cho Role
            new Claim(JwtRegisteredClaimNames.Sub, userAfterVerifyPass.Username),   // Subject của token
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique ID của token
            new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()) // Thời gian tạo token
        };

        // Tạo khóa bí mật để ký token
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256Signature
        );
        //Dùng key bí mật +thuật toán HMAC-SHA256 để ký token.
        //Đây là phần Signature của JWT → giúp server verify token có hợp lệ không.

        // Thiết lập thông tin cho token
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(1), // Token hết hạn sau 1 giờ
            SigningCredentials = credentials,
            Issuer = _issuer,                 // Thêm Issuer vào token
            Audience = _audience,              // Thêm Audience vào token
        };
        //ClaimsIdentity(claims) → gắn tất cả claims vào payload.
        //Expires → thời gian hết hạn(ở đây là 1 ngày kể từ lúc tạo).
        //Issuer → ai phát hành token(ví dụ: "ebay-api").
        //Audience → đối tượng được phép dùng token(ví dụ: "ebay-client").


        // Tạo token bằng JwtSecurityTokenHandler
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        //CreateToken(tokenDescriptor) → tạo ra object JWT.
        //WriteToken(token) → chuyển object thành chuỗi dạng:

        // Trả về chuỗi token đã mã hóa
        //GenerateToken lấy thông tin user + role từ DB → nhúng vào claims → tạo JWT token.
        //Token này dùng để xác thực(Authentication) và phân quyền(Authorization) trong API.
        return tokenHandler.WriteToken(token);
    }

    public string DecodePayloadToken(string token)
    {
        try
        {
            // Kiểm tra token có null hoặc rỗng không
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("Token cannot be empty", nameof(token));
            }
            //Nếu token bị null hoặc rỗng → ném ra lỗi ArgumentException.
            //Giúp tránh trường hợp gọi hàm mà không truyền token.

            // Tạo handler và đọc token
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            //JwtSecurityTokenHandler: class có sẵn trong System.IdentityModel.Tokens.Jwt, dùng để parse token.
            //ReadJwtToken(token) → parse chuỗi JWT thành object JwtSecurityToken.
            //Lúc này ta có thể truy cập:
            //jwtToken.Header → phần Header (alg, typ, …).
            //jwtToken.Payload → phần Payload(chứa claims).
            //jwtToken.Claims → danh sách các claim.


            // Lấy username từ claims (thường nằm trong claim "sub" hoặc "name")
            var usernameClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "UserName"); // Common in some identity providers
            //Tìm claim có Type = "UserName".
            //Đây chính là claim đã được thêm trong hàm GenerateToken.

            if (usernameClaim == null)
            {
                //Nếu không có claim "UserName" → ném lỗi InvalidOperationException.
                //Điều này giúp tránh việc token không đúng format mong đợi.
                throw new InvalidOperationException("Username not found in payload");
            }

            //Lấy ra giá trị thực sự trong claim "UserName".
            //Ví dụ: "nguyenvanA".

            return usernameClaim.Value;
        }
        catch (Exception ex)
        {
            // Xử lý lỗi (có thể log lỗi ở đây)
            throw new InvalidOperationException($"Lỗi khi decode token: {ex.Message}", ex);
        }
    }


}
