using ISAI.Lessons.Models.Enums;

namespace ISAI.Lessons.Models.Interfaces.App
{   public interface IDeviceOrientation
    {
        /// <summary>
        ///     Gets current device orientation
        /// </summary>
        DeviceOrientations CurrentOrientation { get; }

        /// <summary>
        ///     Lock orientation in the specified position
        /// </summary>
        /// <param name="orientation">Position for lock.</param>
        void LockOrientation(DeviceOrientations orientation);

        /// <summary>
        ///     Unlock orientation
        /// </summary>
        void UnlockOrientation();
    }
}
