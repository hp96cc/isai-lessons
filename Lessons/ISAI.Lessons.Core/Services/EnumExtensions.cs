using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Scf.Core.Services
{
    public static class EnumExtensions
    {

        public static string GetDisplayName(this Enum enumValue)
        {
            if (enumValue == null) return "";

            return enumValue.GetType()
                .GetMember(enumValue.ToString())
                .First()
                .GetCustomAttribute<DisplayAttribute>()
                .GetName();
        }


        public static string GetDisplayDescription(this Enum value)
        {
            DescriptionAttribute attribute = value.GetType()
          .GetField(value.ToString())
          .GetCustomAttributes(typeof(DescriptionAttribute), false)
          .SingleOrDefault() as DescriptionAttribute;
            return attribute == null ? value.ToString() : attribute.Description;
        }

    }

}