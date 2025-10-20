using ISAI.Lessons.Models.Interfaces;

namespace ISAI.Lessons.EntityFramework.Models
{
    public class GroupTutorialGroup : BaseModel, IGroupTutorialGroup
    {
        public string Name { get; set; }
        public int ListOrder { get; set; }
    }
}
