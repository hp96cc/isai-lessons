namespace ISAI.Lessons.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedGroupTutorial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GroupTutorial",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AppId = c.Int(nullable: false),
                        TutorUserId = c.String(maxLength: 128),
                        DateTimeEnd = c.DateTimeOffset(nullable: false, precision: 7),
                        DateTimeStart = c.DateTimeOffset(nullable: false, precision: 7),
                        DurationInMinutes = c.Int(nullable: false),
                        TutorialCostPerPerson = c.Decimal(nullable: false, precision: 14, scale: 4),
                        Name = c.String(),
                        Description = c.String(),
                        TeamsId = c.String(),
                        TeamsLink = c.String(),
                        Deleted = c.Boolean(nullable: false),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        ModifiedUserId = c.String(maxLength: 128),
                        CreatedUserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.App", t => t.AppId)
                .ForeignKey("dbo.User", t => t.CreatedUserId)
                .ForeignKey("dbo.User", t => t.ModifiedUserId)
                .ForeignKey("dbo.User", t => t.TutorUserId)
                .Index(t => t.AppId)
                .Index(t => t.TutorUserId)
                .Index(t => t.ModifiedUserId)
                .Index(t => t.CreatedUserId);
            
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
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.CreatedUserId)
                .ForeignKey("dbo.Customer", t => t.CustomerId)
                .ForeignKey("dbo.GroupTutorial", t => t.GroupTutorialId)
                .ForeignKey("dbo.User", t => t.ModifiedUserId)
                .Index(t => t.CustomerId)
                .Index(t => t.GroupTutorialId)
                .Index(t => t.ModifiedUserId)
                .Index(t => t.CreatedUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.GroupTutorialCustomer", "ModifiedUserId", "dbo.User");
            DropForeignKey("dbo.GroupTutorialCustomer", "GroupTutorialId", "dbo.GroupTutorial");
            DropForeignKey("dbo.GroupTutorialCustomer", "CustomerId", "dbo.Customer");
            DropForeignKey("dbo.GroupTutorialCustomer", "CreatedUserId", "dbo.User");
            DropForeignKey("dbo.GroupTutorial", "TutorUserId", "dbo.User");
            DropForeignKey("dbo.GroupTutorial", "ModifiedUserId", "dbo.User");
            DropForeignKey("dbo.GroupTutorial", "CreatedUserId", "dbo.User");
            DropForeignKey("dbo.GroupTutorial", "AppId", "dbo.App");
            DropIndex("dbo.GroupTutorialCustomer", new[] { "CreatedUserId" });
            DropIndex("dbo.GroupTutorialCustomer", new[] { "ModifiedUserId" });
            DropIndex("dbo.GroupTutorialCustomer", new[] { "GroupTutorialId" });
            DropIndex("dbo.GroupTutorialCustomer", new[] { "CustomerId" });
            DropIndex("dbo.GroupTutorial", new[] { "CreatedUserId" });
            DropIndex("dbo.GroupTutorial", new[] { "ModifiedUserId" });
            DropIndex("dbo.GroupTutorial", new[] { "TutorUserId" });
            DropIndex("dbo.GroupTutorial", new[] { "AppId" });
            DropTable("dbo.GroupTutorialCustomer");
            DropTable("dbo.GroupTutorial");
        }
    }
}
