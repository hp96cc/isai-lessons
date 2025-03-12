namespace ISAI.Lessons.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TutorSubjectUser2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.User", "AllowedTutorialSubjectTutorUserJson", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.User", "AllowedTutorialSubjectTutorUserJson");
        }
    }
}
