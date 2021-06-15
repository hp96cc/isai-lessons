using System;
using System.Collections.Generic;
using System.Text;

namespace ISAI.Lessons.Models.ViewModels
{
    public class UpdatePasswordViewModel
    {

        public string Digest { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }
    }
}
