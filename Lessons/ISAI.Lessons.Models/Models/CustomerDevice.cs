namespace ISAI.Lessons.Models.Models
{
    public class CustomerDevice : BaseModel
    {

        public string Name { get; set; }

        public string DeviceType { get; set; }

        public int CustomerId { get; set; }

     
        public Customer Customer { get; set; }


    }
}
