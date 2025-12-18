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

    public string GenerateToken(UserEmployee userAfterVerifyPass)
    {
        var key = Encoding.ASCII.GetBytes(_key);
        var claims = new List<Claim>
        {
            new Claim("UserId", userAfterVerifyPass.Id.ToString()),          // 🔥 Thêm UserId
            new Claim("UserName", userAfterVerifyPass.Username),              // Claim mặc định cho username
            new Claim(JwtRegisteredClaimNames.Sub, userAfterVerifyPass.Username),   // Subject của token
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique ID của token
            new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()) // Thời gian tạo token
        };

        var lstRole = _context.RoleUsers.Include(n => n.Role).Where(item => item.UserId == userAfterVerifyPass.Id);
        if (lstRole.Count() > 0)
        {
            foreach (RoleUser item in lstRole)
            {
                claims.Add(new Claim(ClaimTypes.Role, item.Role.RoleName.ToString()));
            }
        }
  
        // Tạo khóa bí mật để ký token
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256Signature
        );

        // Thiết lập thông tin cho token
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(1), // Token hết hạn sau 1 giờ
            SigningCredentials = credentials,
            Issuer = _issuer,                 // Thêm Issuer vào token
            Audience = _audience,              // Thêm Audience vào token
        };


        // Tạo token bằng JwtSecurityTokenHandler
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

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
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var usernameClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "UserName"); // Common in some identity providers
            if (usernameClaim == null)
            {
                throw new InvalidOperationException("Username not found in payload");
            }
            return usernameClaim.Value;
        }
        catch (Exception ex)
        {
            // Xử lý lỗi (có thể log lỗi ở đây)
            throw new InvalidOperationException($"Lỗi khi decode token: {ex.Message}", ex);
        }
    }


}

