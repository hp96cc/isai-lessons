namespace ISAI.Lessons.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GroupTutorialGroup : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GroupTutorialGroup",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        ListOrder = c.Int(nullable: false),
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
            
            AddColumn("dbo.GroupTutorial", "GroupTutorialGroupId", c => c.Int());
            CreateIndex("dbo.GroupTutorial", "GroupTutorialGroupId");
            AddForeignKey("dbo.GroupTutorial", "GroupTutorialGroupId", "dbo.GroupTutorialGroup", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.GroupTutorial", "GroupTutorialGroupId", "dbo.GroupTutorialGroup");
            DropForeignKey("dbo.GroupTutorialGroup", "ModifiedUserId", "dbo.User");
            DropForeignKey("dbo.GroupTutorialGroup", "CreatedUserId", "dbo.User");
            DropIndex("dbo.GroupTutorialGroup", new[] { "CreatedUserId" });
            DropIndex("dbo.GroupTutorialGroup", new[] { "ModifiedUserId" });
            DropIndex("dbo.GroupTutorial", new[] { "GroupTutorialGroupId" });
            DropColumn("dbo.GroupTutorial", "GroupTutorialGroupId");
            DropTable("dbo.GroupTutorialGroup");
        }
    }
}
