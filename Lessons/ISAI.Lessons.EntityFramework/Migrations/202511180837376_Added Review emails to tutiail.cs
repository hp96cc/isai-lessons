namespace ISAI.Lessons.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedReviewemailstotutiail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Tutorial", "PendingReviewEmailConfirmationUser", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Tutorial", "PendingReviewEmailConfirmationUser");
        }
    }
}
