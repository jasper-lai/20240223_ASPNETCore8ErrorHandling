using ASPNETCore8ErrorHandling.Middlewares;
using MyBakeryMvcWeb.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

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
