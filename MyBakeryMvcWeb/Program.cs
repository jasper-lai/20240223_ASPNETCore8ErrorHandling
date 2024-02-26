using ASPNETCore8ErrorHandling.Filters;
using ASPNETCore8ErrorHandling.Middlewares;
using MyBakeryMvcWeb.Services;
using System.Text.Encodings.Web;
using System.Text.Unicode;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
    {
        // ���U���쪺 Filter
        options.Filters.Add(new ValidateModelAttribute());
    })
    .AddJsonOptions(jsonOptions =>
    {
        // PropertyNamingPolicy = JsonNamingPolicy.CamelCase,   
        // null: ���� C# �r���j�g�� json ���榡.
        jsonOptions.JsonSerializerOptions.PropertyNamingPolicy = null;
        //���\�򥻩ԤB�^��Τ�������r������r��
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

#region �ϥ� TraceIdMiddleware
// {jasper} �Y�n���Φ۩w�q ttpContext.TraceIdentifier ����, �u�n��o�q�@ Remark �Y�i
// ���U�۩w�q���X TraceId �� Middleware
app.UseMiddleware<TraceIdMiddleware>();
#endregion

#region �ϥ� ExceptionHandlingMiddleware
// {jasper} ���U�ҥ~�d�I�� Middleware
// �`�N: �o�ӥ����Ʀb�w�]���ت��ҥ~�B�z�����, �b�o�ͨҥ~��, �~��Ѧ۩w�q�� Middleware �@�B�z
app.UseMiddleware<ExceptionHandlingMiddleware>();
#endregion

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
