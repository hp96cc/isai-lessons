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

        public Customer Customer { get; set; }

        public List<string> Errors { get; set; }
    }
}
