using ISAI.Lessons.Models.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ISAI.Lessons.Models.Interfaces
{
    public interface IAuthService
    {

        Task<Auth> GetAuth();

        Task SetAuth(Auth auth);

    }
}
