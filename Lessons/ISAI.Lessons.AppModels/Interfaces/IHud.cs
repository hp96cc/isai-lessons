using System;

namespace ISAI.Lessons.Models.Interfaces
{
    public interface IHud
    {

        void ShowSpinner(string message);

        void Dismiss();

        void ShowError(string message, TimeSpan time);


    }
}
