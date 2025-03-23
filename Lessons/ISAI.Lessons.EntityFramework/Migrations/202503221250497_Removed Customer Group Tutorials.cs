namespace ISAI.Lessons.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedCustomerGroupTutorials : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.GroupTutorialCustomer", "CreatedUserId", "dbo.User");
            DropForeignKey("dbo.GroupTutorialCustomer", "CustomerId", "dbo.Customer");
            DropForeignKey("dbo.GroupTutorialCustomer", "GroupTutorialId", "dbo.GroupTutorial");
            DropForeignKey("dbo.GroupTutorialCustomer", "ModifiedUserId", "dbo.User");
            DropIndex("dbo.GroupTutorialCustomer", new[] { "CustomerId" });
            DropIndex("dbo.GroupTutorialCustomer", new[] { "GroupTutorialId" });
            DropIndex("dbo.GroupTutorialCustomer", new[] { "ModifiedUserId" });
            DropIndex("dbo.GroupTutorialCustomer", new[] { "CreatedUserId" });
            DropTable("dbo.GroupTutorialCustomer");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.GroupTutorialCustomer",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CustomerId = c.Int(nullable: false),
                        GroupTutorialId = c.Int(nullable: false),
                        Deleted = c.Boolean(nullable: false),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        ModifiedUserId = c.String(maxLength: 128),
                        CreatedUserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.GroupTutorialCustomer", "CreatedUserId");
            CreateIndex("dbo.GroupTutorialCustomer", "ModifiedUserId");
            CreateIndex("dbo.GroupTutorialCustomer", "GroupTutorialId");
            CreateIndex("dbo.GroupTutorialCustomer", "CustomerId");
            AddForeignKey("dbo.GroupTutorialCustomer", "ModifiedUserId", "dbo.User", "Id");
            AddForeignKey("dbo.GroupTutorialCustomer", "GroupTutorialId", "dbo.GroupTutorial", "Id");
            AddForeignKey("dbo.GroupTutorialCustomer", "CustomerId", "dbo.Customer", "Id");
            AddForeignKey("dbo.GroupTutorialCustomer", "CreatedUserId", "dbo.User", "Id");
        }
    }
}
