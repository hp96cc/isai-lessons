using CommunityToolkit.Mvvm.Input;
using ISAI.Lessons.Mobile.SampleCode.Models;

namespace ISAI.Lessons.Mobile.SampleCode.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}