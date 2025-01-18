using ISAI.Lessons.Models.Interfaces;
using System.Collections.Generic;

namespace ISAI.Lessons.EntityFramework.ViewModels
{
    public class RegisterResponseViewModel
    {

        public ICustomer Customer { get; set; }

        public List<string> Errors { get; set; }
    }
}
