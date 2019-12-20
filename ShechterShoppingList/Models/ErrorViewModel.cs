using System;

namespace ShechterShoppingList.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public string ErrorMessage {get;set;}
        public string AWS_SECRET_ACCESS_KEY {get;set;}
        public string AWS_ACCESS_KEY_ID {get;set;}
        public string AWS_REGION {get;set;}
    }
}
