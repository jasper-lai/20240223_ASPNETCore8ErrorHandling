namespace ASPNETCore8ErrorHandling.Filters
{
    using System.Runtime.Serialization;

    /// <summary>
    /// 抽象的自定義例外基底類別
    /// </summary>
    public abstract class MyExceptionBase : Exception
    {
        protected MyExceptionBase()
        {
        }

        protected MyExceptionBase(string? message) : base(message)
        {
        }

        protected MyExceptionBase(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }

    }

    #region 此區例外, 將會回傳 HTTP 4XX

    /// <summary>
    /// 用戶端資料未通過檢核: 發生空值
    /// </summary>
    /// <seealso cref="ASPNETCore8ErrorHandling.Filters.MyExceptionBase" />
    /// <remarks>
    /// 會回傳 HTTP 400 Bad Request
    /// </remarks>
    public class MyParamNullException : MyExceptionBase
    {
        public override string Message { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MyParamNullException"/> class.
        /// </summary>
        /// <param name="fieldDescription">中文名稱描述</param>
        public MyParamNullException(string fieldDescription)
        {
            this.Message = $"{fieldDescription} 不能為空!";
        }
    }

    /// <summary>
    /// 用戶端資料未通過檢核: 超出範圍. 
    /// </summary>
    /// <seealso cref="ASPNETCore8ErrorHandling.Filters.MyExceptionBase" />
    /// <remarks>
    /// 會回傳 HTTP 400 Bad Request
    /// </remarks>
    public class MyOutRangeException : MyExceptionBase
    {
        public override string Message { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MyOutRangeException"/> class.
        /// </summary>
        /// <param name="fieldDescription">中文名稱描述</param>
        public MyOutRangeException(string fieldDescription)
        {
            this.Message = $"{fieldDescription} 超出範圍!";
        }
    }

    /// <summary>
    /// 用戶端資料未通過檢核: 資料不存在. 通常用在 查詢/更新/刪除
    /// </summary>
    /// <seealso cref="ASPNETCore8ErrorHandling.Filters.MyExceptionBase" />
    /// <remarks>
    /// 會回傳 HTTP 404 Not Found
    /// </remarks>
    public class MyDataNotExistException : MyExceptionBase
    {
        public override string Message { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MyDataNotExistException"/> class.
        /// </summary>
        /// <param name="fieldDescription">中文名稱描述</param>
        public MyDataNotExistException(string fieldDescription)
        {
            this.Message = $"{fieldDescription} 資料不存在!";
        }
    }

    /// <summary>
    /// 用戶端資料未通過檢核: 資料已存在. 通常用在 新增.
    /// </summary>
    /// <seealso cref="ASPNETCore8ErrorHandling.Filters.MyExceptionBase" />
    /// <remarks>
    /// 會回傳 HTTP 409 Conflict
    /// </remarks>
    public class MyDataExistException : MyExceptionBase
    {
        public override string Message { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MyDataExistException"/> class.
        /// </summary>
        /// <param name="fieldDescription">中文名稱描述</param>
        public MyDataExistException(string fieldDescription)
        {
            this.Message = $"{fieldDescription} 資料已存在!";
        }
    }


    /// <summary>
    /// 用戶端資料未通過檢核: 未通過認證及授權
    /// </summary>
    /// <seealso cref="ASPNETCore8ErrorHandling.Filters.MyExceptionBase" />
    /// <remarks>
    /// 會回傳 HTTP 401 Unauthorized
    /// </remarks>
    public class MyUnauthorizedException : MyExceptionBase
    {
        public override string Message { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MyUnauthorizedException"/> class.
        /// </summary>
        public MyUnauthorizedException()
        {
            this.Message = $"您未通過認證及授權!";
        }
    }

    /// <summary>
    /// 用戶端資料未通過檢核: 無存取此頁面功能的權限
    /// </summary>
    /// <seealso cref="ASPNETCore8ErrorHandling.Filters.MyExceptionBase" />
    /// <remarks>
    /// 會回傳 HTTP 403 Forbidden
    /// </remarks>
    public class MyForbiddenException : MyExceptionBase
    {
        public override string Message { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MyForbiddenException"/> class.
        /// </summary>
        public MyForbiddenException()
        {
            this.Message = $"您無存取此頁面功能的權限!";
        }
    }

    /// <summary>
    /// 用戶端資料未通過檢核: 其它狀況
    /// </summary>
    /// <seealso cref="ASPNETCore8ErrorHandling.Filters.MyExceptionBase" />
    /// <remarks>
    /// 會回傳 HTTP 400 Bad Request
    /// 例如: throw new MyClientException("產品單價 欄位不可小於 0")
    /// </remarks>
    public class MyClientException : MyExceptionBase
    {
        public override string Message { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MyClientException"/> class.
        /// </summary>
        /// <param name="description">錯誤描述</param>
        public MyClientException(string description)
        {
            this.Message = $"{description}";
        }
    }

    #endregion

    
 }
