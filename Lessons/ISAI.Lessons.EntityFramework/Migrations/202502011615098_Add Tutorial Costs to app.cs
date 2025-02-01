namespace ISAI.Lessons.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTutorialCoststoapp : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.App", "TutorialCost20Minutes", c => c.Decimal(nullable: false, precision: 14, scale: 4));
            AddColumn("dbo.App", "TutorialCost40Minutes", c => c.Decimal(nullable: false, precision: 14, scale: 4));
            AddColumn("dbo.App", "TutorialCost60Minutes", c => c.Decimal(nullable: false, precision: 14, scale: 4));
            AddColumn("dbo.Tutorial", "TutorialCost", c => c.Decimal(nullable: false, precision: 14, scale: 4));
            AddColumn("dbo.SubscriptionType", "SubscriptionLengthInMonths", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SubscriptionType", "SubscriptionLengthInMonths");
            DropColumn("dbo.Tutorial", "TutorialCost");
            DropColumn("dbo.App", "TutorialCost60Minutes");
            DropColumn("dbo.App", "TutorialCost40Minutes");
            DropColumn("dbo.App", "TutorialCost20Minutes");
        }
    }
}
