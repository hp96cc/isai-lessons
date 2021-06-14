using System;
using System.Collections.Generic;
using System.Text;

namespace ISAI.Lessons.Models.Enums
{
    public enum ResponseStatus
    {

        OK,
        Failed,
        UserAlreadyExists,
        LicenceExpired,
        InvalidLicence,
        TooManyDevices

    }
}
