using System;

namespace ISAI.Lessons.EntityFramework.ViewModels
{
    public class GroupTutorialPurchaseRequestViewModel
    {
        public int GroupTutorialId { get; set; }
        public string SuccessUrl { get; set; }
        public string CancelUrl { get; set; }

    }
}
