namespace ISAI.Lessons.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedStripePriceIdstoApp : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.App", "TutorialStripPriceId20Minutes", c => c.String());
            AddColumn("dbo.App", "TutorialStripPriceId40Minutes", c => c.String());
            AddColumn("dbo.App", "TutorialStripPriceId60Minutes", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.App", "TutorialStripPriceId60Minutes");
            DropColumn("dbo.App", "TutorialStripPriceId40Minutes");
            DropColumn("dbo.App", "TutorialStripPriceId20Minutes");
        }
    }
}
