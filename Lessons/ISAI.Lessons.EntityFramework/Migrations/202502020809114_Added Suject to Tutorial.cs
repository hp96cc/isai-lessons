namespace ISAI.Lessons.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedSujecttoTutorial : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Tutorial", "TutorialSubjectId", c => c.Int());
            CreateIndex("dbo.Tutorial", "TutorialSubjectId");
            AddForeignKey("dbo.Tutorial", "TutorialSubjectId", "dbo.TutorialSubject", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Tutorial", "TutorialSubjectId", "dbo.TutorialSubject");
            DropIndex("dbo.Tutorial", new[] { "TutorialSubjectId" });
            DropColumn("dbo.Tutorial", "TutorialSubjectId");
        }
    }
}
