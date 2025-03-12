using System;

namespace ISAI.Lessons.Models.Interfaces.App
{
    public interface IHud
    {

        void ShowSpinner(string message);

        void Dismiss();

        void ShowError(string message, TimeSpan time);


    }
}
