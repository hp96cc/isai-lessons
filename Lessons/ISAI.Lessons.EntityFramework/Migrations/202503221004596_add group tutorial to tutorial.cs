namespace ISAI.Lessons.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addgrouptutorialtotutorial : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Tutorial", "GroupTutorialId", c => c.Int());
            CreateIndex("dbo.Tutorial", "GroupTutorialId");
            AddForeignKey("dbo.Tutorial", "GroupTutorialId", "dbo.GroupTutorial", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Tutorial", "GroupTutorialId", "dbo.GroupTutorial");
            DropIndex("dbo.Tutorial", new[] { "GroupTutorialId" });
            DropColumn("dbo.Tutorial", "GroupTutorialId");
        }
    }
}
