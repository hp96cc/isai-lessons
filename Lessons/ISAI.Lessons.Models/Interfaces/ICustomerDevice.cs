namespace ISAI.Lessons.Models.Interfaces
{
    public interface ICustomerDevice : IBaseInterface
    {
        int CustomerId { get; set; }
        string DeviceIdentifier { get; set; }
        string DeviceType { get; set; }
        string Name { get; set; }
    }
}