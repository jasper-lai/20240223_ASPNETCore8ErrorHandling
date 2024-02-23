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

        // �w�]�Ĥ@�r����p�g (camel), �Y�n�令 C# ���r���j�g�榡 (Pascal), �n�]�� null 
        // ���n: �Y�]���r���p�g, �h�ۦ�b ProblemDetails �X�W����� (TraceId, ControllerName...), �ä��|��p�g!!!
        //
        // PropertyNamingPolicy = JsonNamingPolicy.CamelCase,   
        options.JsonSerializerOptions.PropertyNamingPolicy = null; 
        //���\�򥻩ԤB�^��Τ�������r������r��
        options.JsonSerializerOptions.Encoder =
            JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.CjkUnifiedIdeographs);
    }); 

//#region ���U Exception Filter �� DI
//// {jasper} ���U���쪺 Exception Filter
//builder.Services.AddScoped<MyExceptionFilter>();
//builder.Services.AddControllersWithViews(options =>
//{
//    options.Filters.AddService<MyExceptionFilter>();
//});
//#endregion

#region ���U GlobalExceptionHandler �� ProblemDeatils �� DI
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

//#region �ϥΤ��ت� ExceptionHandler
//app.UseExceptionHandler();
//#endregion

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
