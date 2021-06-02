using System;
using System.Collections.Generic;
using System.Text;

namespace ISAI.Lessons.Models.ViewModels
{
    public class SendEmailRequestViewModel
    {
        public string Name { get; set; }

        public string Email { get; set; }

        public string Subject { get; set; }

        public string Message { get; set; }
    }
}
