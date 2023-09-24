using System;

namespace ReppadL.Common
{
    public class APIResponse<T>
    {
        public int ResponseCode { get; set; }
        public T? Result { get; set; }
        public string Message { get; set; } = "";
    }
}
