namespace ISAI.Lessons.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addedaddtionalemailtemplates : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Subscription", "HasFreeTrialEmailBeenSent", c => c.Boolean(nullable: false));
            AddColumn("dbo.Subscription", "HasFreeTrialExpiryEmailBeenSent", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Subscription", "HasFreeTrialExpiryEmailBeenSent");
            DropColumn("dbo.Subscription", "HasFreeTrialEmailBeenSent");
        }
    }
}
