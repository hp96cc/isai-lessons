namespace ISAI.Lessons.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTutorialNotes : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Tutorial", "Notes", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Tutorial", "Notes");
        }
    }
}
