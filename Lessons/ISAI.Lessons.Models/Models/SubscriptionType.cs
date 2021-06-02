using System;
using System.Collections.Generic;
using System.Text;

namespace ISAI.Lessons.Models.Models
{
    public class SubscriptionType : BaseModel
    {
        public string Name { get; set; }

        public int? LessonGroupId { get; set; }

    }
}
