namespace eCademy.NUh16.PhotoShare.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Rating : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserRatings",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Image_Id = c.Int(),
                        User_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Images", t => t.Image_Id)
                .ForeignKey("dbo.AspNetUsers", t => t.User_Id)
                .Index(t => t.Image_Id)
                .Index(t => t.User_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserRatings", "User_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.UserRatings", "Image_Id", "dbo.Images");
            DropIndex("dbo.UserRatings", new[] { "User_Id" });
            DropIndex("dbo.UserRatings", new[] { "Image_Id" });
            DropTable("dbo.UserRatings");
        }
    }
}
