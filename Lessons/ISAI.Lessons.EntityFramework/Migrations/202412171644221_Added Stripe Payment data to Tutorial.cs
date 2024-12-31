namespace ISAI.Lessons.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedStripePaymentdatatoTutorial : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Tutorial", "StripePaymentSessionId", c => c.String());
            AddColumn("dbo.Tutorial", "StripePaymentId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Tutorial", "StripePaymentId");
            DropColumn("dbo.Tutorial", "StripePaymentSessionId");
        }
    }
}
