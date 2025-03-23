using ISAI.Lessons.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISAI.Lessons.EntityFramework.Models
{
    public class GroupTutorial : BaseModel, IGroupTutorial
    {
        public int AppId { get; set; }

        [ForeignKey("AppId")]
        public App App { get; set; }
        public string TutorUserId { get; set; }

        [ForeignKey("TutorUserId")]
        public User TutorUser { get; set; }
        public DateTimeOffset DateTimeEnd { get; set; }
        public DateTimeOffset DateTimeStart { get; set; }
        public int DurationInMinutes { get; set; }
        public decimal TutorialCostPerPerson { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string TeamsId { get; set; }
        public string TeamsLink { get; set; }

        public virtual List<Tutorial> Tutorials { get; set; }

    }
}
