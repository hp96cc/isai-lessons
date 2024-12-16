namespace ISAI.Lessons.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedTutorial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Tutorial",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AppId = c.Int(nullable: false),
                        CustomerId = c.Int(nullable: false),
                        TutorUserId = c.String(maxLength: 128),
                        LessonId = c.Int(),
                        Name = c.String(),
                        DateTimeStart = c.DateTimeOffset(nullable: false, precision: 7),
                        DateTimeEnd = c.DateTimeOffset(nullable: false, precision: 7),
                        DurationInMinutes = c.Int(nullable: false),
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
                .ForeignKey("dbo.Customer", t => t.CustomerId)
                .ForeignKey("dbo.Lesson", t => t.LessonId)
                .ForeignKey("dbo.User", t => t.ModifiedUserId)
                .ForeignKey("dbo.User", t => t.TutorUserId)
                .Index(t => t.AppId)
                .Index(t => t.CustomerId)
                .Index(t => t.TutorUserId)
                .Index(t => t.LessonId)
                .Index(t => t.ModifiedUserId)
                .Index(t => t.CreatedUserId);
            
            AddColumn("dbo.User", "IsTutor", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Tutorial", "TutorUserId", "dbo.User");
            DropForeignKey("dbo.Tutorial", "ModifiedUserId", "dbo.User");
            DropForeignKey("dbo.Tutorial", "LessonId", "dbo.Lesson");
            DropForeignKey("dbo.Tutorial", "CustomerId", "dbo.Customer");
            DropForeignKey("dbo.Tutorial", "CreatedUserId", "dbo.User");
            DropForeignKey("dbo.Tutorial", "AppId", "dbo.App");
            DropIndex("dbo.Tutorial", new[] { "CreatedUserId" });
            DropIndex("dbo.Tutorial", new[] { "ModifiedUserId" });
            DropIndex("dbo.Tutorial", new[] { "LessonId" });
            DropIndex("dbo.Tutorial", new[] { "TutorUserId" });
            DropIndex("dbo.Tutorial", new[] { "CustomerId" });
            DropIndex("dbo.Tutorial", new[] { "AppId" });
            DropColumn("dbo.User", "IsTutor");
            DropTable("dbo.Tutorial");
        }
    }
}
