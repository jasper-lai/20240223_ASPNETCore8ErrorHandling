using ASPNETCore8ErrorHandling.Filters;
using ASPNETCore8ErrorHandling.Middlewares;
using MyBakeryMvcWeb.Services;
using System.Text.Encodings.Web;
using System.Text.Unicode;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
    {
        // 註冊全域的 Filter
        // 如果套用到全域, 可以不用管 DI 的問題, logger 會自動注入
        // 如果套用到 Controller, 則需要利用 ServiceFilterAttribute
        // options.Filters.Add<CalcActionExecTimeAttribute>();
        options.Filters.Add(new ValidateModelAttribute());
        ////以下這個寫法, 會出 CS7036 的錯誤
        ////未提供任何可對應到 '計算Action執行時間Attribute.計算Action執行時間Attribute(ILogger<計算Action執行時間Attribute>)' 之必要參數 'logger' 的引數
        //options.Filters.Add(new 計算Action執行時間Attribute());
    })
    .AddJsonOptions(jsonOptions =>
    {
        // PropertyNamingPolicy = JsonNamingPolicy.CamelCase,   
        // null: 維持 C# 字首大寫的 json 欄位格式.
        jsonOptions.JsonSerializerOptions.PropertyNamingPolicy = null;
        //允許基本拉丁英文及中日韓文字維持原字元
        jsonOptions.JsonSerializerOptions.Encoder =
            JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.CjkUnifiedIdeographs);
    });

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<CalcActionExecTimeAttribute>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
