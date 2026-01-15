namespace ISAI.Lessons.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSignedSubtitleflags : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Lesson", "IsSignedAvailable", c => c.Boolean(nullable: false));
            AddColumn("dbo.Lesson", "IsSubtitlesAvailable", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Lesson", "IsSubtitlesAvailable");
            DropColumn("dbo.Lesson", "IsSignedAvailable");
        }
    }
}
