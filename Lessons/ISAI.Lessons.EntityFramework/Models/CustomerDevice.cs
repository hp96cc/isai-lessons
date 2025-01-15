using System.ComponentModel.DataAnnotations.Schema;

namespace ISAI.Lessons.EntityFramework.Models
{
    public class CustomerDevice : BaseModel
    {

        public string Name { get; set; }

        public string DeviceType { get; set; }

        public string DeviceIdentifier { get; set; }

        public int CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }


    }
}
