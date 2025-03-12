namespace ISAI.Lessons.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddStripeCallback2 : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.StripeCallback", newName: "StripeWebhookLog");
        }
        
        public override void Down()
        {
            RenameTable(name: "dbo.StripeWebhookLog", newName: "StripeCallback");
        }
    }
}
