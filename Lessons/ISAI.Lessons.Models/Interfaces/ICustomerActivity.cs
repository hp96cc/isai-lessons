using System;

namespace ISAI.Lessons.Models.Interfaces
{
    public interface ICustomerActivity : IBaseInterface
    {
        int CustomerDeviceId { get; set; }
        int LessonId { get; set; }
        DateTimeOffset StartDateTime { get; set; }
    }
}