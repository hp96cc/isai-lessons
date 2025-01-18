namespace ISAI.Lessons.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedTutorialSubjecttolessongroup : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.LessonGroup", "TutorialSubjectGroupId", "dbo.TutorialSubjectGroup");
            DropForeignKey("dbo.Lesson", "TutorialSubjectId", "dbo.TutorialSubject");
            DropIndex("dbo.Lesson", new[] { "TutorialSubjectId" });
            DropIndex("dbo.LessonGroup", new[] { "TutorialSubjectGroupId" });
            AddColumn("dbo.LessonGroup", "TutorialSubjectId", c => c.Int(nullable: false, defaultValue: 1));
            CreateIndex("dbo.LessonGroup", "TutorialSubjectId");
            AddForeignKey("dbo.LessonGroup", "TutorialSubjectId", "dbo.TutorialSubject", "Id");
            DropColumn("dbo.Lesson", "TutorialSubjectId");
            DropColumn("dbo.LessonGroup", "TutorialSubjectGroupId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.LessonGroup", "TutorialSubjectGroupId", c => c.Int());
            AddColumn("dbo.Lesson", "TutorialSubjectId", c => c.Int(nullable: true, defaultValue: 1));
            DropForeignKey("dbo.LessonGroup", "TutorialSubjectId", "dbo.TutorialSubject");
            DropIndex("dbo.LessonGroup", new[] { "TutorialSubjectId" });
            DropColumn("dbo.LessonGroup", "TutorialSubjectId");
            CreateIndex("dbo.LessonGroup", "TutorialSubjectGroupId");
            CreateIndex("dbo.Lesson", "TutorialSubjectId");
            AddForeignKey("dbo.Lesson", "TutorialSubjectId", "dbo.TutorialSubject", "Id");
            AddForeignKey("dbo.LessonGroup", "TutorialSubjectGroupId", "dbo.TutorialSubjectGroup", "Id");
        }
    }
}
