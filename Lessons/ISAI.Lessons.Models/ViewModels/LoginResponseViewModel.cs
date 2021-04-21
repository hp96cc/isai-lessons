using ISAI.Lessons.EntityFramework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISAI.Lessons.EntityFramework.ViewModels
{
    public class LoginResponseViewModel
    {
        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public List<string> Errors { get; set; }
    }
}
