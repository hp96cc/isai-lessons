using Syncfusion.Maui.Toolkit.Charts;

namespace ISAI.Lessons.Mobile.SampleCode.Pages.Controls
{
    public class LegendExt : ChartLegend
    {
        protected override double GetMaximumSizeCoefficient()
        {
            return 0.5;
        }
    }
}
