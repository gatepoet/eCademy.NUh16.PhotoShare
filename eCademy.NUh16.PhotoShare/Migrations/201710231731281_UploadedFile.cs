namespace eCademy.NUh16.PhotoShare.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UploadedFile : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UploadedFiles",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        ImageData = c.Binary(),
                        Filename = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Images", "File_Id", c => c.Guid());
            CreateIndex("dbo.Images", "File_Id");
            AddForeignKey("dbo.Images", "File_Id", "dbo.UploadedFiles", "Id");
            DropColumn("dbo.Images", "ImageData");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Images", "ImageData", c => c.Binary());
            DropForeignKey("dbo.Images", "File_Id", "dbo.UploadedFiles");
            DropIndex("dbo.Images", new[] { "File_Id" });
            DropColumn("dbo.Images", "File_Id");
            DropTable("dbo.UploadedFiles");
        }
    }
}
