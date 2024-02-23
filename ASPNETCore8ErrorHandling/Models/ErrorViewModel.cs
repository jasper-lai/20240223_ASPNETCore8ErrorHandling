namespace ASPNETCore8ErrorHandling.Models
{
    using System.Net;

    public class ErrorViewModel
    {
        /// <summary>
        /// Http Request �ߤ@�ѧO�X
        /// </summary>
        /// <value>
        /// The trace identifier.
        /// </value>
        public string? TraceId { get; set; }

        ///// <summary>
        ///// �O�_�n��� Http Request �ߤ@�ѧO�X
        ///// </summary>
        ///// <value>
        /////   <c>true</c> if [show trace identifier]; otherwise, <c>false</c>.
        ///// </value>
        //public bool ShowTraceId => !string.IsNullOrEmpty(TraceId);

        /// <summary>
        /// HTTP ���A�X
        /// </summary>
        /// <value>
        /// The status code.
        /// </value>
        public int StatusCode { get; set; } = (int)(HttpStatusCode.OK);

        /// <summary>
        /// HTTP ���A�X�C�|�W��
        /// </summary>
        /// <value>
        /// The status code.
        /// </value>
        public string StatusCodeName { get; set; } = HttpStatusCode.OK.ToString();

        /// <summary>
        /// �T������
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the controller.
        /// </summary>
        /// <value>
        /// The name of the controller.
        /// </value>
        public string ControllerName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the action.
        /// </summary>
        /// <value>
        /// The name of the action.
        /// </value>
        public string ActionName { get; set; } = string.Empty;

        public ErrorViewModel() { }

    }
}
