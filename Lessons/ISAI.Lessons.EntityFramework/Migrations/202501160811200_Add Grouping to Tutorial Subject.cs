namespace ISAI.Lessons.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddGroupingtoTutorialSubject : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.Subject", newName: "TutorialSubjectGroup");
            RenameTable(name: "dbo.SubjectTutorUser", newName: "TutorialSubjectTutorUser");
            DropForeignKey("dbo.SubjectLessonGroup", "CreatedUserId", "dbo.User");
            DropForeignKey("dbo.SubjectLessonGroup", "LessonGroupId", "dbo.LessonGroup");
            DropForeignKey("dbo.SubjectLessonGroup", "ModifiedUserId", "dbo.User");
            DropForeignKey("dbo.SubjectLessonGroup", "SubjectId", "dbo.Subject");
            DropForeignKey("dbo.SubjectTutorUser", "SubjectId", "dbo.Subject");
            DropIndex("dbo.SubjectLessonGroup", new[] { "LessonGroupId" });
            DropIndex("dbo.SubjectLessonGroup", new[] { "SubjectId" });
            DropIndex("dbo.SubjectLessonGroup", new[] { "ModifiedUserId" });
            DropIndex("dbo.SubjectLessonGroup", new[] { "CreatedUserId" });
            DropIndex("dbo.TutorialSubjectTutorUser", new[] { "SubjectId" });
            CreateTable(
                "dbo.TutorialSubject",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TutorialSubjectGroupId = c.Int(nullable: false),
                        Name = c.String(),
                        Deleted = c.Boolean(nullable: false),
                        DateCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        DateModified = c.DateTimeOffset(nullable: false, precision: 7),
                        ModifiedUserId = c.String(maxLength: 128),
                        CreatedUserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TutorialSubjectGroup", t => t.TutorialSubjectGroupId)
                .Index(t => t.TutorialSubjectGroupId)
                .Index(t => t.ModifiedUserId)
                .Index(t => t.CreatedUserId);
            
            AddColumn("dbo.LessonGroup", "TutorialSubjectGroupId", c => c.Int());
            AddColumn("dbo.TutorialSubjectGroup", "AppId", c => c.Int(nullable: false));
            AddColumn("dbo.TutorialSubjectTutorUser", "TutorialSubjectId", c => c.Int(nullable: false));
            CreateIndex("dbo.LessonGroup", "TutorialSubjectGroupId");
            CreateIndex("dbo.TutorialSubjectGroup", "AppId");
            CreateIndex("dbo.TutorialSubjectTutorUser", "TutorialSubjectId");
            AddForeignKey("dbo.TutorialSubjectGroup", "AppId", "dbo.App", "Id");
            AddForeignKey("dbo.LessonGroup", "TutorialSubjectGroupId", "dbo.TutorialSubjectGroup", "Id");
            AddForeignKey("dbo.TutorialSubjectTutorUser", "TutorialSubjectId", "dbo.TutorialSubject", "Id");
            DropColumn("dbo.TutorialSubjectTutorUser", "SubjectId");
            DropTable("dbo.SubjectLessonGroup");
        }
        
        public override void Down()
        {
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
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.TutorialSubjectTutorUser", "SubjectId", c => c.Int(nullable: false));
            DropForeignKey("dbo.TutorialSubjectTutorUser", "TutorialSubjectId", "dbo.TutorialSubject");
            DropForeignKey("dbo.TutorialSubject", "TutorialSubjectGroupId", "dbo.TutorialSubjectGroup");
            DropForeignKey("dbo.LessonGroup", "TutorialSubjectGroupId", "dbo.TutorialSubjectGroup");
            DropForeignKey("dbo.TutorialSubjectGroup", "AppId", "dbo.App");
            DropIndex("dbo.TutorialSubjectTutorUser", new[] { "TutorialSubjectId" });
            DropIndex("dbo.TutorialSubject", new[] { "CreatedUserId" });
            DropIndex("dbo.TutorialSubject", new[] { "ModifiedUserId" });
            DropIndex("dbo.TutorialSubject", new[] { "TutorialSubjectGroupId" });
            DropIndex("dbo.TutorialSubjectGroup", new[] { "AppId" });
            DropIndex("dbo.LessonGroup", new[] { "TutorialSubjectGroupId" });
            DropColumn("dbo.TutorialSubjectTutorUser", "TutorialSubjectId");
            DropColumn("dbo.TutorialSubjectGroup", "AppId");
            DropColumn("dbo.LessonGroup", "TutorialSubjectGroupId");
            DropTable("dbo.TutorialSubject");
            CreateIndex("dbo.TutorialSubjectTutorUser", "SubjectId");
            CreateIndex("dbo.SubjectLessonGroup", "CreatedUserId");
            CreateIndex("dbo.SubjectLessonGroup", "ModifiedUserId");
            CreateIndex("dbo.SubjectLessonGroup", "SubjectId");
            CreateIndex("dbo.SubjectLessonGroup", "LessonGroupId");
            AddForeignKey("dbo.SubjectTutorUser", "SubjectId", "dbo.Subject", "Id");
            AddForeignKey("dbo.SubjectLessonGroup", "SubjectId", "dbo.Subject", "Id");
            AddForeignKey("dbo.SubjectLessonGroup", "ModifiedUserId", "dbo.User", "Id");
            AddForeignKey("dbo.SubjectLessonGroup", "LessonGroupId", "dbo.LessonGroup", "Id");
            AddForeignKey("dbo.SubjectLessonGroup", "CreatedUserId", "dbo.User", "Id");
            RenameTable(name: "dbo.TutorialSubjectTutorUser", newName: "SubjectTutorUser");
            RenameTable(name: "dbo.TutorialSubjectGroup", newName: "Subject");
        }
    }
}
