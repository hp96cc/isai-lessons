using System.ComponentModel.DataAnnotations;

namespace ISAI.Lessons.Models.Enums
{
    public enum VideoVersion
    {
        [Display(Name = "Standard video")]
        Standard,
        
        [Display(Name = "British Sign Language video")]
        SignLanguage
    }
}