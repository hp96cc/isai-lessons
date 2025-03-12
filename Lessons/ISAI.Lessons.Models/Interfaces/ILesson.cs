namespace ISAI.Lessons.Models.Interfaces
{
    public interface ILesson : IBaseInterface
    {
        int AppId { get; set; }
        string AssetId { get; set; }
        string BitmovinId { get; set; }
        string Description { get; set; }
        int LessonGroupId { get; set; }
        string LessonNotes { get; set; }
        int ListOrder { get; set; }
        string Name { get; set; }
        bool PendingDownload { get; set; }
        string SourceUrl { get; set; }
        int? SubscriptionTypeId { get; set; }
        
    }
}