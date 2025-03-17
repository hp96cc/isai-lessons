using System;

namespace ISAI.Lessons.Models.Interfaces
{
    public interface IGroupTutorial : IBaseInterface
    {
        int AppId { get; set; }

        DateTimeOffset DateTimeEnd { get; set; }
        DateTimeOffset DateTimeStart { get; set; }
        int DurationInMinutes { get; set; }

        decimal TutorialCostPerPerson { get; set; }
        string Name { get; set; }

        string Description { get; set; }

        string TeamsId { get; set; }
        string TeamsLink { get; set; }
        string TutorUserId { get; set; }
    }
}