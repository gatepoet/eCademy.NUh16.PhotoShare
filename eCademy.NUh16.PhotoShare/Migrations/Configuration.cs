namespace eCademy.NUh16.PhotoShare.Migrations
{
    using eCademy.NUh16.PhotoShare.Models;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<eCademy.NUh16.PhotoShare.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "eCademy.NUh16.PhotoShare.Models.ApplicationDbContext";
        }

        protected override void Seed(eCademy.NUh16.PhotoShare.Models.ApplicationDbContext context)
        {
            CreateOrUpdateUser(context, "radioactive");
            CreateOrUpdateUser(context, "lifeisbeautiful");
        }

        private static void CreateOrUpdateUser(ApplicationDbContext context, string username)
        {
            if (!context.Users.Any(u => u.UserName == username))
            {
                var store = new UserStore<ApplicationUser>(context);
                var manager = new UserManager<ApplicationUser>(store);
                manager.Create(
                    new ApplicationUser { UserName = username, Email = username + "@gmail.com" },
                    "PhotoShare123."
                );
            }
        }
    }
}
