using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISAI.Lessons.EntityFramework.Models
{
    public class App : BaseModel
    {
        
        public string Name { get; set; }

        public bool AllowCollectAtStore { get; set; }
        
        public bool ShippingLoadingIsPercent { get; set; }
     
        public decimal ShippingLoading { get; set; }


        public string ShipStationAccessToken { get; set; }

        public string ShipStationFromPostcode { get; set; }

        public decimal MinimumOrder { get;set; }

        public string SquareAccessToken { get; set; }

        public string SquareSandboxAccessToken { get; set; }

        public bool SquareUseSandbox { get; set; }

        public string Currency { get; set; }

    }
}
