using ISAI.Lessons.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ISAI.Lessons.Models.Models
{
    public class ResponseData<T> : ResponseBase
    {
        public T Content { get; set; }
    }

    public class ResponseBase
    {
        private AggregateException _aggregateException;
        public ResponseStatus Status { get; set; }

        public AggregateException AggregateException
        {
            get { return _aggregateException; }
            set { _aggregateException = value; }
        }

        public List<ErrorResponse> ErrorResponse { get; set; }
    }

    public class ErrorResponse
    {
        public ErrorCode Code { get; set; }
        public string Message { get; set; }
        public string SessionId { get; set; }
        public string ErrorDescription { get; set; }
    }

}
