namespace ISAI.Lessons.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSupportforStripeApplicationfee : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.App", "TutorialStripApplicationFee20Minutes", c => c.String());
            AddColumn("dbo.App", "TutorialStripApplicationFee40Minutes", c => c.String());
            AddColumn("dbo.App", "TutorialStripApplicationFee60Minutes", c => c.String());
            AddColumn("dbo.User", "TutorEmail", c => c.String());
            AddColumn("dbo.User", "TutorStripeId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.User", "TutorStripeId");
            DropColumn("dbo.User", "TutorEmail");
            DropColumn("dbo.App", "TutorialStripApplicationFee60Minutes");
            DropColumn("dbo.App", "TutorialStripApplicationFee40Minutes");
            DropColumn("dbo.App", "TutorialStripApplicationFee20Minutes");
        }
    }
}
