using ASPNETCore8ErrorHandling.Filters;
using ASPNETCore8ErrorHandling.Middlewares;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using System.Text.Unicode;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {

        // 預設第一字母轉小寫 (camel), 若要改成 C# 的字首大寫格式 (Pascal), 要設為 null 
        // 重要: 若設成字首小寫, 則自行在 ProblemDetails 擴增的欄位 (TraceId, ControllerName...), 並不會轉小寫!!!
        //
        // PropertyNamingPolicy = JsonNamingPolicy.CamelCase,   
        options.JsonSerializerOptions.PropertyNamingPolicy = null; 
        //允許基本拉丁英文及中日韓文字維持原字元
        options.JsonSerializerOptions.Encoder =
            JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.CjkUnifiedIdeographs);
    }); 

//#region 註冊 Exception Filter 至 DI
//// {jasper} 註冊全域的 Exception Filter
//builder.Services.AddScoped<MyExceptionFilter>();
//builder.Services.AddControllersWithViews(options =>
//{
//    options.Filters.AddService<MyExceptionFilter>();
//});
//#endregion

#region 註冊 GlobalExceptionHandler 及 ProblemDeatils 至 DI
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

#region 使用 TraceIdMiddleware
// {jasper} 若要停用自定義 ttpContext.TraceIdentifier 的話, 只要把這段作 Remark 即可
// 註冊自定義產出 TraceId 的 Middleware
app.UseMiddleware<TraceIdMiddleware>();
#endregion

#region 使用 ExceptionHandlingMiddleware
// {jasper} 註冊例外攔截的 Middleware
// 注意: 這個必須排在預設內建的例外處理機制之後, 在發生例外時, 才能由自定義的 Middleware 作處理
app.UseMiddleware<ExceptionHandlingMiddleware>();
#endregion

//#region 使用內建的 ExceptionHandler
//app.UseExceptionHandler();
//#endregion

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
