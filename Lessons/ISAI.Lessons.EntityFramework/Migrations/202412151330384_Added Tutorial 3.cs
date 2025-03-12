namespace ISAI.Lessons.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedTutorial3 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Tutorial", "User_Id", "dbo.User");
            DropIndex("dbo.Tutorial", new[] { "User_Id" });
            DropColumn("dbo.Tutorial", "User_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Tutorial", "User_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.Tutorial", "User_Id");
            AddForeignKey("dbo.Tutorial", "User_Id", "dbo.User", "Id");
        }
    }
}
