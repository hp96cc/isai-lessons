namespace ISAI.Lessons.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddStripePriceidtoSubscriptiontype : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SubscriptionType", "StripePriceId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.SubscriptionType", "StripePriceId");
        }
    }
}
