namespace eCademy.NUh16.PhotoShare.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CascaseDelete : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.UserRatings", "Image_Id", "dbo.Images");
            DropIndex("dbo.UserRatings", new[] { "Image_Id" });
            AlterColumn("dbo.UserRatings", "Image_Id", c => c.Int(nullable: false));
            CreateIndex("dbo.UserRatings", "Image_Id");
            AddForeignKey("dbo.UserRatings", "Image_Id", "dbo.Images", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserRatings", "Image_Id", "dbo.Images");
            DropIndex("dbo.UserRatings", new[] { "Image_Id" });
            AlterColumn("dbo.UserRatings", "Image_Id", c => c.Int());
            CreateIndex("dbo.UserRatings", "Image_Id");
            AddForeignKey("dbo.UserRatings", "Image_Id", "dbo.Images", "Id");
        }
    }
}
