namespace ISAI.Lessons.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTutorPriceids : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.User", "TutorialStripPriceId20Minutes", c => c.String());
            AddColumn("dbo.User", "TutorialStripPriceId40Minutes", c => c.String());
            AddColumn("dbo.User", "TutorialStripPriceId60Minutes", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.User", "TutorialStripPriceId60Minutes");
            DropColumn("dbo.User", "TutorialStripPriceId40Minutes");
            DropColumn("dbo.User", "TutorialStripPriceId20Minutes");
        }
    }
}
