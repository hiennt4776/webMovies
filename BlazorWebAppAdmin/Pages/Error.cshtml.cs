using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;
using System.Security.Claims;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

//System.Diagnostics: dùng để lấy Activity.Current?.Id (theo dõi hoạt động hiện tại trong hệ thống tracing).
//Microsoft.AspNetCore.Mvc: cung cấp các attribute như [ResponseCache], [IgnoreAntiforgeryToken].
//Microsoft.AspNetCore.Mvc.RazorPages: chứa PageModel, base class cho các Razor Pages model.

namespace BlazorWebApp.Pages;
//Đặt class này trong namespace BlazorWebApp.Pages (trùng với thư mục Pages).

//[ResponseCache(...)]
//Duration = 0: không cho phép cache.
//Location = ResponseCacheLocation.None: không cache ở client hay proxy.
//NoStore = true: không lưu trữ response.
//👉 Ý nghĩa: Trang lỗi luôn luôn được load mới, không bị cache, để hiển thị đúng thông tin lỗi mới nhất.
//[IgnoreAntiforgeryToken]
//Bỏ qua kiểm tra CSRF token (anti-forgery).
//Vì trang lỗi chỉ hiển thị thông tin, không xử lý form POST → không cần CSRF protection.

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]

public class ErrorModel : PageModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    //    RequestId: lưu mã định danh cho request hiện tại.
    //ShowRequestId: property chỉ đọc, trả về true nếu RequestId khác null/empty.
    //👉 Dùng để quyết định trong View (Error.cshtml) có hiển thị RequestId hay không.


    private readonly ILogger<ErrorModel> _logger;

    //    ILogger<ErrorModel>: công cụ logging tích hợp ASP.NET Core.

    //Giúp ghi log khi có lỗi, để sau này kiểm tra log trên server.

    public ErrorModel(ILogger<ErrorModel> logger)
    {
        _logger = logger;
    }
    //    ASP.NET Core sẽ Dependency Injection một ILogger<ErrorModel> khi tạo ErrorModel.
    //Sau đó gán vào _logger để có thể dùng log trong class (ví dụ _logger.LogError(...)).


    public void OnGet()
    {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
    }


    //    Đây là Page Handler cho HTTP GET(khi user vào /Error).
    //Activity.Current?.Id: lấy ID từ System.Diagnostics.Activity
    //(dùng trong distributed tracing, OpenTelemetry, Application Insights...).
    //HttpContext.TraceIdentifier: ID của request hiện tại(ASP.NET Core luôn tạo).
    //Dùng toán tử ?? → nếu không có Activity.Id thì lấy TraceIdentifier.
    //👉 Nhờ đó RequestId luôn có giá trị duy nhất cho mỗi request,
    //giúp developer lần ra log chính xác khi lỗi xảy ra.
}

//Class ErrorModel là code-behind cho trang Error.cshtml.
//Nó:
//Không cho cache response.
//Bỏ qua CSRF check.
//Sinh ra RequestId duy nhất cho mỗi request.
//Cho phép view kiểm tra ShowRequestId để hiển thị hoặc ẩn.
//Sẵn sàng dùng logger để ghi log lỗi.
//👉 Bạn có muốn mình minh họa thêm bằng ví dụ log: khi exception xảy ra trong app,
//log server và trang lỗi (RequestId) sẽ khớp nhau như thế nào không?
