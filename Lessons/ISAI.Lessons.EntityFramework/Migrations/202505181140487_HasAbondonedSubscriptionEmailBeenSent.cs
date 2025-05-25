namespace ISAI.Lessons.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class HasAbondonedSubscriptionEmailBeenSent : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Subscription", "HasAbondonedSubscriptionEmailBeenSent", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Subscription", "HasAbondonedSubscriptionEmailBeenSent");
        }
    }
}
