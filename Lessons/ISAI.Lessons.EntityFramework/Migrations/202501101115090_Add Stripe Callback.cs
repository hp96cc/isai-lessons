namespace ISAI.Lessons.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddStripeCallback : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.StripeCallback",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CallBackName = c.String(),
                        Description = c.String(),
                        Deleted = c.Boolean(nullable: false),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        ModifiedUserId = c.String(maxLength: 128),
                        CreatedUserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.CreatedUserId)
                .ForeignKey("dbo.User", t => t.ModifiedUserId)
                .Index(t => t.ModifiedUserId)
                .Index(t => t.CreatedUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.StripeCallback", "ModifiedUserId", "dbo.User");
            DropForeignKey("dbo.StripeCallback", "CreatedUserId", "dbo.User");
            DropIndex("dbo.StripeCallback", new[] { "CreatedUserId" });
            DropIndex("dbo.StripeCallback", new[] { "ModifiedUserId" });
            DropTable("dbo.StripeCallback");
        }
    }
}
