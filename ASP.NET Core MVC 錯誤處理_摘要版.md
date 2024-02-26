
# ASP.NET Core MVC Error Handling 摘要版

這篇係針對前一篇文章, 摘錄筆者認為在 ASP.NET Core 8 MVC 框架下的適宜作法.  
這篇不作太多的文字說明, 主要是描述實作程序, 及相關程式碼.  

## 章節內容
* 壹. [相關資源](#section1)  
* 貳. [文章摘要 (建議的方案)](#section2)  
  * 一. [ASP.NET Core MVC 訊息流程](#section2_1)  
  * 二. [實作 基礎 MVC 專案](#section2_2)  
    (一) [定義例外類別](#section2_2_1)  
    (二) [撰寫產生 HTTP RQ/RP 的唯一識別碼 的 Middleware](#section2_2_2)  
    (三) [撰寫例外攔截的 Middleware (採用 ProblemDetails 類別作為統一的回傳格式](#section2_2_3)  
    (四) [撰寫未通過 Model Validation Attribute 的 Action Filter](#section2_2_4)  
  * 三. [實作 範例 MVC 專案](#section2_3)  
    (一) [Program.cs](#section2_3_1)  
    (二) [Model Validation Attribute 的處理](#section2_3_2)  
    (三) [自行在 controller 或 service 撰寫檢核邏輯](#section2_3_3)  
    (四) [前端 AJAX 的處理](#section2_3_4)  

<hr />

<!--more-->

## 壹. 相關資源 <a id="section1"></a>

* (Google Blog) https://www.jasperstudy.com/2024/02/aspnet-mvc-aspnet-core-mvc.html
* (GitHub Repo) https://github.com/jasper-lai/20240223_ASPNETCore8ErrorHandling

<hr />

## 貳. 文章摘要 (建議的方案) <a id="section2"></a>  

### 一. ASP.NET Core MVC 訊息流程 <a id="section2_1"></a>  
 
 ![ASP.NETCoreMiddleware](pictures2/11-ASP.NET_Core_Middleware.png)  
 ![ASP.NETCoreMvcFilter](pictures2/12-ASP.NET_Core_MVC_Filters.png)

________________________________________

### 二. 實作 基礎 MVC 專案  <a id="section2_2"></a>  

該專案主要提供以下功能:  
(1) 自定義例外類別  
(2) 取得唯一 Request 識別碼的 Middleware  
(3) 例外攔截的 Middleware: 會在這裡把所有的例外, 轉為對應的 HTTP Response  
(4) 針對 Validation Attribute 撰寫 Action Filter  

#### (一) 定義例外類別 <a id="section2_2_1"></a>  
![CustomClientException](pictures2/21-CustomClientExceptions.png)  

#### (二) 撰寫產生 HTTP RQ/RP 的唯一識別碼 的 Middleware <a id="section2_2_2"></a>  

```csharp
public class TraceIdMiddleware
{
    private readonly RequestDelegate _next;

    public TraceIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.TraceIdentifier = Guid.NewGuid().ToString();
        string traceId = context.TraceIdentifier;
        context.Response.Headers["X-Trace-Id"] = traceId;
        await _next(context);
    }
}
```

#### (三) 撰寫例外攔截的 Middleware (採用 ProblemDetails 類別作為統一的回傳格式) <a id="section2_2_3"></a>  

```csharp
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly static JsonSerializerOptions _jsonOptions = new()
    {
        // 預設第一字母轉小寫 (camel), 若要改成 C# 的字首大寫格式 (Pascal), 要設為 null
        // 重要:
        // (1) 如果有設 options 的話, 就一定維持字首大寫, 因為沒設 PropertyNamingPolicy, 等同 null.
        // (2) 只有在完全沒設 options, 或設為 JsonNamingPolicy.CamelCase, 才會是小寫.
        // (3) 若設成字首小寫, 則自行在 ProblemDetails 擴增的欄位 (TraceId, ControllerName...), 並不會轉小寫 !!
        //
        //PropertyNamingPolicy = JsonNamingPolicy.CamelCase,  
        //PropertyNamingPolicy = null,  
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.CjkUnifiedIdeographs)
    };

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    #region 方式二: 回傳 ASP.NET Core 內建的 ProblemDetails 類別

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {

        // STEP 1: 取得 controller name / action name / trace id
        var routeData = context.GetRouteData();
        var controllerName = routeData?.Values["controller"]?.ToString();
        var actionName = routeData?.Values["action"]?.ToString();
        var traceId = context.TraceIdentifier;

        // STEP 2: 建立回傳物件
        ProblemDetails response = exception switch
        {
            MyParamNullException _ or
            MyOutRangeException _ or
            MyClientException _ => new ProblemDetails()
            {
                Title = HttpStatusCode.BadRequest.ToString(),
                Status = StatusCodes.Status400BadRequest,
            },

            MyDataNotExistException _ => new ProblemDetails()
            {
                Title = HttpStatusCode.NotFound.ToString(),
                Status = StatusCodes.Status404NotFound,
            },

            MyDataExistException _ => new ProblemDetails()
            {
                Title = HttpStatusCode.Conflict.ToString(),
                Status = StatusCodes.Status409Conflict,
            },

            MyUnauthorizedException _ => new ProblemDetails()
            {
                Title = HttpStatusCode.Unauthorized.ToString(),
                Status = StatusCodes.Status401Unauthorized,
            },

            MyForbiddenException _ => new ProblemDetails()
            {
                Title = HttpStatusCode.Forbidden.ToString(),
                Status = StatusCodes.Status403Forbidden,
            },

            _ => new()
            {
                Title = HttpStatusCode.InternalServerError.ToString(),
                Status = StatusCodes.Status500InternalServerError,
            }
        };
        if (response.Status != StatusCodes.Status500InternalServerError)
            response.Detail = exception.Message;
        else
            response.Detail = "伺服器發生未預期的錯誤";

        response.Instance = context.Request.Path;
        response.Extensions.Add("TraceId", traceId);
        response.Extensions.Add("ControllerName", controllerName);
        response.Extensions.Add("ActionName", actionName);

        // STEP 3: 設定回傳的 response header
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = response.Status ?? StatusCodes.Status500InternalServerError;

        // STEP 4: 寫入至 Log
        //var options = new JsonSerializerOptions()
        //{
        //    Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.CjkUnifiedIdeographs)
        //};
        string jsonString = JsonSerializer.Serialize(response, _jsonOptions);

        if (response.Status >= 400 && response.Status < 500)
            _logger.LogWarning("Controller={controllerName} Action={actionName} => Message={message}", controllerName, actionName, exception.Message);
        if (response.Status >= 500)
            _logger.LogError(exception, "Controller={controllerName} Action={actionName} => Message={message}", controllerName, actionName, exception.Message);

        _logger.LogInformation("{json}", jsonString);    //輸出完整的 json 字串

        // STEP 5: 回傳結果
        //await context.Response.WriteAsJsonAsync(response);
        await context.Response.WriteAsJsonAsync(response, _jsonOptions);
    }

    #endregion
}
```

#### (四) 撰寫未通過 Model Validation Attribute 的 Action Filter <a id="section2_2_4"></a>  

```csharp
public class ValidateModelAttribute : ActionFilterAttribute
{

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            // 方式1: 統一集中例外處理 if you want to throw a custom exception
            var description = ModelErrorToString(context.ModelState);
            throw new MyClientException(description);
            //// 方式2: 利用 context.Result 回傳, 但這就無法統一回傳格式了.
            //context.Result = new BadRequestObjectResult(context.ModelState);
        }
    }

    /// <summary>
    /// 將未通過 Model Validation 資料檢核的清單, 轉為字串
    /// </summary>
    /// <returns></returns>
    private string ModelErrorToString(ModelStateDictionary modelState)
    {
        // Your implementation to convert model state errors to a single string
        var errors = modelState.Values.SelectMany(v => v.Errors)
           .Select(e => e.ErrorMessage)
           .ToList();
        var description = string.Join(Environment.NewLine, errors);
        description = "輸入的資料有誤: " + Environment.NewLine + description;
        return description;
    }
}
```

________________________________________

### 三. 實作 範例 MVC 專案  <a id="section2_3"></a>  

主要是在未通過資料檢核時, 要拋出對應的 Exception.

#### (一) Program.cs  <a id="section2_3_1"></a>  

```csharp
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
```

```csharp
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
```

#### (二) Model Validation Attribute 的處理 <a id="section2_3_2"></a>  

```csharp
public class ProductViewModel : IValidatableObject
{
    [Display(Name = "產品代號")]
    [Required(ErrorMessage = "{0} 必須要有值")]
    public int Id { get; set; }

    [Display(Name = "產品名稱")]
    [Required(ErrorMessage = "{0} 必須要有值")]
    [StringLength(10, ErrorMessage = "產品名稱長度最多為 10 個字元")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "訂購數量")]
    [Range(1, 10, ErrorMessage = "{0} 的資料值, 必須介於 {1} ~ {2}.")]
    public int OrderQty { get; set; }

    [Display(Name = "產品單價")]
    public int UnitPrice { get; set; }


    /// <summary>
    /// Validates the specified validation context.
    /// </summary>
    /// <param name="validationContext">The validation context.</param>
    /// <remarks>
    /// 重要: 只有在前述的 Validation Attribute 都通過以後, 才會執行這裡的檢核.
    /// 亦即: (1) Validation Attribute 有誤, 前端只會看到 Validation Attribute 的錯誤.
    ///       (2) Validation Attribute 正確, 前端才看到以下的錯誤.  
    /// </remarks>
    /// <returns></returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Custom validation logic for UnitPrice
        if (UnitPrice < 1 || UnitPrice > 1000)
        {
            string[] memberNames = [nameof(UnitPrice)];
            yield return new ValidationResult("產品單價必須在 1 ~ 1000", memberNames);
        }
    }
}
```

```csharp
[HttpPost]
public IActionResult CreateAjaxJson([FromBody] ProductViewModel product)
{
    // -----------------
    // 以下已經移到 Action Filter 作處理了
    // -----------------
    //// 處理 validation attribute (model binding) 檢核未過的錯誤
    //if (!ModelState.IsValid)
    //{
    //    var description = this.ModelErrorToString();
    //    throw new MyClientException(description); 
    //    //return BadRequest(ModelState);
    //}
    var result = _service.Create(product);
    _logger.LogInformation("處理結果: {result}", result);
    return View("Create",product);
}
```

#### (三) 自行在 controller 或 service 撰寫檢核邏輯 <a id="section2_3_3"></a>  

```csharp
public IActionResult About(int id = 0)
{
    if (id == 1)
    {
        throw new MyClientException("傳入的參數值有誤");
    }
    return View();
}
```

#### (四) 前端 AJAX 的處理 <a id="section2_3_4"></a>  

可以在 error: 段落, 統一進行錯誤訊息的呈現.

```csharp
function btnDataNotExistException() {
    let product = makeProductObject();
    clearJsonResult();

    $.ajax({
        url: '@Url.Action("OccursDataNotExistException", "Product")', // Replace 'YOUR_ENDPOINT_URL' with the URL you're posting to
        type: 'POST',
        contentType: 'application/json', // This tells the server that you're sending JSON data
        data: JSON.stringify(product), // Convert the person object to a JSON string
        success: function (response) {
            // Handle success
            console.log('Success:', response);
        },
        error: function (xhr, status, error) {
            // 真正有用的, 只有 xhr 物件 !!!
            try {
                // 將回傳的錯誤內容, 轉換為 errorJson 物件
                var errorJson = JSON.parse(xhr.responseText);

                // 呈現完整 json 內容
                console.log('Error JSON:', errorJson);

                // 只取其中部份的 json 欄位
                console.log('Title:', errorJson.Title);
                console.log('Detail:', errorJson.Detail);

                // Pretty-print the JSON error object to make it readable
                // with a spacing of 4 characters for indentation, making it more readable.
                var prettyErrorJson = JSON.stringify(errorJson, null, 4);

                // Set the pretty-printed JSON as the text content of the #jsonResult element
                $('#jsonResult').text(prettyErrorJson);
            } catch (e) {
                console.log('Error parsing error response:', e);
                $('#jsonResult').text('An error occurred, but the error details could not be parsed.');
            }
        }
    });

    // 美化 <code></code> 的內容
    hljs.highlightAll();
}
```
