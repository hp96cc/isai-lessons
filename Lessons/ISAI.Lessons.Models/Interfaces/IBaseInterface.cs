using System;

namespace ISAI.Lessons.Models.Interfaces
{ 

    public interface IBaseInterface
    {
        int Id { get; set; }
        bool Deleted { get; set; }
        DateTimeOffset DateCreated { get; set; }
        DateTimeOffset DateModified { get; set; }
        string ModifiedUserId { get; set; }
        string CreatedUserId { get; set; }

    }

}