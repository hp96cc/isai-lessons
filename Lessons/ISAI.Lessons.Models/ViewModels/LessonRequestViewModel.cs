using ISAI.Lessons.Models.Enums;
using ISAI.Lessons.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISAI.Lessons.EntityFramework.ViewModels
{
    public class LessonRequestViewModel
    {
        public CustomerDeviceViewModel CustomerDevice { get; set; }
        public int LessonId { get; set; }

        public RemoteMediaType RemoteMediaType { get; set; }
    }


    public class CustomerDeviceViewModel
    {

        public string Name { get; set; }

        public string DeviceType { get; set; }

        public string DeviceIdentifier { get; set; }


    }
}
