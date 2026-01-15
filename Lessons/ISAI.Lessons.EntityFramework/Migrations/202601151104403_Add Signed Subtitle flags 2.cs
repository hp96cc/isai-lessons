namespace ISAI.Lessons.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSignedSubtitleflags2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Lesson", "IsSigndSubtitlesAvailable", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Lesson", "IsSigndSubtitlesAvailable");
        }
    }
}
