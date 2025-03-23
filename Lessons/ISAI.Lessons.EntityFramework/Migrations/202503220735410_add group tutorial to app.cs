namespace ISAI.Lessons.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addgrouptutorialtoapp : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.App", "GroupTutorialStripePriceId", c => c.String());
            DropColumn("dbo.App", "TutorialStripPriceId20Minutes");
            DropColumn("dbo.App", "TutorialStripPriceId40Minutes");
            DropColumn("dbo.App", "TutorialStripPriceId60Minutes");
        }
        
        public override void Down()
        {
            AddColumn("dbo.App", "TutorialStripPriceId60Minutes", c => c.String());
            AddColumn("dbo.App", "TutorialStripPriceId40Minutes", c => c.String());
            AddColumn("dbo.App", "TutorialStripPriceId20Minutes", c => c.String());
            DropColumn("dbo.App", "GroupTutorialStripePriceId");
        }
    }
}
