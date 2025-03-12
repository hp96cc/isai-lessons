namespace ISAI.Lessons.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSupportforStripeApplicationfee2 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.App", "TutorialStripApplicationFee20Minutes", c => c.Decimal(nullable: true, precision: 14, scale: 4, defaultValue: 0));
            AlterColumn("dbo.App", "TutorialStripApplicationFee40Minutes", c => c.Decimal(nullable: true, precision: 14, scale: 4, defaultValue: 0));
            AlterColumn("dbo.App", "TutorialStripApplicationFee60Minutes", c => c.Decimal(nullable: true, precision: 14, scale: 4, defaultValue: 0));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.App", "TutorialStripApplicationFee60Minutes", c => c.String());
            AlterColumn("dbo.App", "TutorialStripApplicationFee40Minutes", c => c.String());
            AlterColumn("dbo.App", "TutorialStripApplicationFee20Minutes", c => c.String());
        }
    }
}
