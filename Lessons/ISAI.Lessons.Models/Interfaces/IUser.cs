using System;
using System.Collections.Generic;
using System.Text;

namespace ISAI.Lessons.Models.Interfaces
{
    public interface IUser { 

         string Fullname { get; set; }

         string Firstname { get; set; }

         string Surname { get; set; }

         string Role { get; set; }

         bool Deleted { get; set; }

         DateTime DateCreated { get; set; }

         DateTime DateModified { get; set; }

         string ModifiedBy { get; set; }

         string CreatedBy { get; set; }
    }
}
