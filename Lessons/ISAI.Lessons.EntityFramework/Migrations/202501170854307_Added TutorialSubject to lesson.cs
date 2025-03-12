namespace ISAI.Lessons.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedTutorialSubjecttolesson : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Lesson", "TutorialSubjectId", c => c.Int(nullable: true));
            CreateIndex("dbo.Lesson", "TutorialSubjectId");
            AddForeignKey("dbo.Lesson", "TutorialSubjectId", "dbo.TutorialSubject", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Lesson", "TutorialSubjectId", "dbo.TutorialSubject");
            DropIndex("dbo.Lesson", new[] { "TutorialSubjectId" });
            DropColumn("dbo.Lesson", "TutorialSubjectId");
        }
    }
}
