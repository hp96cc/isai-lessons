using System;

namespace ISAI.Lessons.Models.Interfaces
{
    public interface IGroupTutorialGroup : IBaseInterface
    {
        string Name { get; set; }

        int ListOrder { get; set; }

    }
}