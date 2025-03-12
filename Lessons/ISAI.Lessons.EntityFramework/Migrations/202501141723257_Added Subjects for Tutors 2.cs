namespace ISAI.Lessons.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedSubjectsforTutors2 : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.SubjectUser", newName: "SubjectTutorUser");
            RenameColumn(table: "dbo.SubjectTutorUser", name: "UserId", newName: "TutorUserId");
            RenameIndex(table: "dbo.SubjectTutorUser", name: "IX_UserId", newName: "IX_TutorUserId");
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
            
            CreateTable(
                "dbo.StripeWebhookLog",
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
            
            AddColumn("dbo.User", "IsTutor", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.StripeWebhookLog", "ModifiedUserId", "dbo.User");
            DropForeignKey("dbo.StripeWebhookLog", "CreatedUserId", "dbo.User");
            DropForeignKey("dbo.Tutorial", "TutorUserId", "dbo.User");
            DropForeignKey("dbo.Tutorial", "ModifiedUserId", "dbo.User");
            DropForeignKey("dbo.Tutorial", "LessonId", "dbo.Lesson");
            DropForeignKey("dbo.Tutorial", "CustomerId", "dbo.Customer");
            DropForeignKey("dbo.Tutorial", "CreatedUserId", "dbo.User");
            DropForeignKey("dbo.Tutorial", "AppId", "dbo.App");
            DropIndex("dbo.StripeWebhookLog", new[] { "CreatedUserId" });
            DropIndex("dbo.StripeWebhookLog", new[] { "ModifiedUserId" });
            DropIndex("dbo.Tutorial", new[] { "CreatedUserId" });
            DropIndex("dbo.Tutorial", new[] { "ModifiedUserId" });
            DropIndex("dbo.Tutorial", new[] { "LessonId" });
            DropIndex("dbo.Tutorial", new[] { "TutorUserId" });
            DropIndex("dbo.Tutorial", new[] { "CustomerId" });
            DropIndex("dbo.Tutorial", new[] { "AppId" });
            DropColumn("dbo.User", "IsTutor");
            DropTable("dbo.StripeWebhookLog");
            DropTable("dbo.Tutorial");
            RenameIndex(table: "dbo.SubjectTutorUser", name: "IX_TutorUserId", newName: "IX_UserId");
            RenameColumn(table: "dbo.SubjectTutorUser", name: "TutorUserId", newName: "UserId");
            RenameTable(name: "dbo.SubjectTutorUser", newName: "SubjectUser");
        }
    }
}
