using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISAI.Lessons.EntityFramework.Interfaces
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