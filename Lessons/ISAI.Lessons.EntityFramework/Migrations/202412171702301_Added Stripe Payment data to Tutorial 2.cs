namespace ISAI.Lessons.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedStripePaymentdatatoTutorial2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Tutorial", "HasCompletedCheckout", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Tutorial", "HasCompletedCheckout");
        }
    }
}
