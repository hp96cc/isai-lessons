using ISAI.Lessons.Models.Interfaces;
using ISAI.Lessons.Models.ViewModels;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISAI.Lessons.EntityFramework.Models
{
    public class GroupTutorialCustomer : BaseModel, IGroupTutorialCustomer
    {

        public int CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }

        public int GroupTutorialId { get; set; }

        [ForeignKey("GroupTutorialId")]
        public GroupTutorial GroupTutorial { get; set; }

    }
}
