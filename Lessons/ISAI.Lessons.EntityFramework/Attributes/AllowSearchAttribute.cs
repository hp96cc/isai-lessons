using System;
using System.Collections.Generic;
using System.Text;

namespace ISAI.Lessons.EntityFramework
{
    [AttributeUsage(AttributeTargets.Property)]
    public class AllowSearchAttribute : Attribute { }
}
