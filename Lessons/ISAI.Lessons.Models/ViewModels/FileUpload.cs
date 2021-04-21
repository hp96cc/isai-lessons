using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISAI.Lessons.EntityFramework.ViewModels
{
    public class FileUpload
    {

        public int size { get; set; }
        public string type { get; set; }
        public string uniqueId { get; set; }
        public string name { get; set; }

        public byte[] bytes { get; set; }
    }

}