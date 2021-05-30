using System;
using System.Threading.Tasks;

namespace ISAI.Lessons.Models.Interfaces
{
    public interface IAppTrackingService
    {
        Task<bool> RequestAppTracking();
    }

}
