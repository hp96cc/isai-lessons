namespace ISAI.Lessons.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedSubjectsforTutors : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.StripeWebhookLog", newName: "Subject");
            DropForeignKey("dbo.Tutorial", "AppId", "dbo.App");
            DropForeignKey("dbo.Tutorial", "CreatedUserId", "dbo.User");
            DropForeignKey("dbo.Tutorial", "CustomerId", "dbo.Customer");
            DropForeignKey("dbo.Tutorial", "LessonId", "dbo.Lesson");
            DropForeignKey("dbo.Tutorial", "ModifiedUserId", "dbo.User");
            DropForeignKey("dbo.Tutorial", "TutorUserId", "dbo.User");
            DropIndex("dbo.Tutorial", new[] { "AppId" });
            DropIndex("dbo.Tutorial", new[] { "CustomerId" });
            DropIndex("dbo.Tutorial", new[] { "TutorUserId" });
            DropIndex("dbo.Tutorial", new[] { "LessonId" });
            DropIndex("dbo.Tutorial", new[] { "ModifiedUserId" });
            DropIndex("dbo.Tutorial", new[] { "CreatedUserId" });
            CreateTable(
                "dbo.SubjectLessonGroup",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LessonGroupId = c.Int(nullable: false),
                        SubjectId = c.Int(nullable: false),
                        Deleted = c.Boolean(nullable: false),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        ModifiedUserId = c.String(maxLength: 128),
                        CreatedUserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.CreatedUserId)
                .ForeignKey("dbo.LessonGroup", t => t.LessonGroupId)
                .ForeignKey("dbo.User", t => t.ModifiedUserId)
                .ForeignKey("dbo.Subject", t => t.SubjectId)
                .Index(t => t.LessonGroupId)
                .Index(t => t.SubjectId)
                .Index(t => t.ModifiedUserId)
                .Index(t => t.CreatedUserId);
            
            CreateTable(
                "dbo.SubjectUser",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SubjectId = c.Int(nullable: false),
                        UserId = c.String(maxLength: 128),
                        Deleted = c.Boolean(nullable: false),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        ModifiedUserId = c.String(maxLength: 128),
                        CreatedUserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.CreatedUserId)
                .ForeignKey("dbo.User", t => t.ModifiedUserId)
                .ForeignKey("dbo.Subject", t => t.SubjectId)
                .ForeignKey("dbo.User", t => t.UserId)
                .Index(t => t.SubjectId)
                .Index(t => t.UserId)
                .Index(t => t.ModifiedUserId)
                .Index(t => t.CreatedUserId);
            
            AddColumn("dbo.Subject", "Name", c => c.String());
            DropColumn("dbo.User", "IsTutor");
            DropColumn("dbo.Subject", "CallBackName");
            DropColumn("dbo.Subject", "Description");
            DropTable("dbo.Tutorial");
        }
        
        public override void Down()
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
                        StripePaymentSessionId = c.String(),
                        StripePaymentId = c.String(),
                        HasCompletedCheckout = c.Boolean(nullable: false),
                        PendingEmailConfirmationUser = c.Boolean(nullable: false),
                        PendingEmailConfirmationTutor = c.Boolean(nullable: false),
                        Deleted = c.Boolean(nullable: false),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        ModifiedUserId = c.String(maxLength: 128),
                        CreatedUserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Subject", "Description", c => c.String());
            AddColumn("dbo.Subject", "CallBackName", c => c.String());
            AddColumn("dbo.User", "IsTutor", c => c.Boolean(nullable: false));
            DropForeignKey("dbo.SubjectUser", "UserId", "dbo.User");
            DropForeignKey("dbo.SubjectUser", "SubjectId", "dbo.Subject");
            DropForeignKey("dbo.SubjectUser", "ModifiedUserId", "dbo.User");
            DropForeignKey("dbo.SubjectUser", "CreatedUserId", "dbo.User");
            DropForeignKey("dbo.SubjectLessonGroup", "SubjectId", "dbo.Subject");
            DropForeignKey("dbo.SubjectLessonGroup", "ModifiedUserId", "dbo.User");
            DropForeignKey("dbo.SubjectLessonGroup", "LessonGroupId", "dbo.LessonGroup");
            DropForeignKey("dbo.SubjectLessonGroup", "CreatedUserId", "dbo.User");
            DropIndex("dbo.SubjectUser", new[] { "CreatedUserId" });
            DropIndex("dbo.SubjectUser", new[] { "ModifiedUserId" });
            DropIndex("dbo.SubjectUser", new[] { "UserId" });
            DropIndex("dbo.SubjectUser", new[] { "SubjectId" });
            DropIndex("dbo.SubjectLessonGroup", new[] { "CreatedUserId" });
            DropIndex("dbo.SubjectLessonGroup", new[] { "ModifiedUserId" });
            DropIndex("dbo.SubjectLessonGroup", new[] { "SubjectId" });
            DropIndex("dbo.SubjectLessonGroup", new[] { "LessonGroupId" });
            DropColumn("dbo.Subject", "Name");
            DropTable("dbo.SubjectUser");
            DropTable("dbo.SubjectLessonGroup");
            CreateIndex("dbo.Tutorial", "CreatedUserId");
            CreateIndex("dbo.Tutorial", "ModifiedUserId");
            CreateIndex("dbo.Tutorial", "LessonId");
            CreateIndex("dbo.Tutorial", "TutorUserId");
            CreateIndex("dbo.Tutorial", "CustomerId");
            CreateIndex("dbo.Tutorial", "AppId");
            AddForeignKey("dbo.Tutorial", "TutorUserId", "dbo.User", "Id");
            AddForeignKey("dbo.Tutorial", "ModifiedUserId", "dbo.User", "Id");
            AddForeignKey("dbo.Tutorial", "LessonId", "dbo.Lesson", "Id");
            AddForeignKey("dbo.Tutorial", "CustomerId", "dbo.Customer", "Id");
            AddForeignKey("dbo.Tutorial", "CreatedUserId", "dbo.User", "Id");
            AddForeignKey("dbo.Tutorial", "AppId", "dbo.App", "Id");
            RenameTable(name: "dbo.Subject", newName: "StripeWebhookLog");
        }
    }
}
