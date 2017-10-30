namespace eCademy.NUh16.PhotoShare.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class User_Image : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Images", "User_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.Images", "User_Id");
            AddForeignKey("dbo.Images", "User_Id", "dbo.AspNetUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Images", "User_Id", "dbo.AspNetUsers");
            DropIndex("dbo.Images", new[] { "User_Id" });
            DropColumn("dbo.Images", "User_Id");
        }
    }
}
