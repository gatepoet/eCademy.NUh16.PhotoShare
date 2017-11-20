namespace eCademy.NUh16.PhotoShare.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserRating : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserRatings", "Rating", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserRatings", "Rating");
        }
    }
}
