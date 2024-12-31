namespace ISAI.Lessons.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Addedemailpendingnotifications : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Tutorial", "PendingEmailConfirmationUser", c => c.Boolean(nullable: false));
            AddColumn("dbo.Tutorial", "PendingEmailConfirmationTutor", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Tutorial", "PendingEmailConfirmationTutor");
            DropColumn("dbo.Tutorial", "PendingEmailConfirmationUser");
        }
    }
}
