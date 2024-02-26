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
        options.Filters.Add(new ValidateModelAttribute());
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
