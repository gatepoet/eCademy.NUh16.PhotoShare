namespace eCademy.NUh16.PhotoShare.Migrations
{
    using eCademy.NUh16.PhotoShare.Models;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
    {
        private const string AdminRole = "admin";
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "eCademy.NUh16.PhotoShare.Models.ApplicationDbContext";
        }

        protected override void Seed(ApplicationDbContext context)
        {
            if (!context.Roles.Any(r => r.Name == AdminRole)) {
                var store = new RoleStore<IdentityRole>(context);
                var manager = new RoleManager<IdentityRole>(store);
                manager.Create(new IdentityRole { Name = AdminRole });
            }


            CreateOrUpdateUser(context, "radioactive");
            CreateOrUpdateUser(context, "lifeisbeautiful");
        }

        private static void CreateOrUpdateUser(ApplicationDbContext context, string username)
        {
            if (!context.Users.Any(u => u.UserName == username))
            {
                var store = new UserStore<ApplicationUser>(context);
                var manager = new UserManager<ApplicationUser>(store);
                var user = new ApplicationUser { UserName = username, Email = username + "@gmail.com" };
                manager.Create(
                    user,
                    "PhotoShare123."
                );
                manager.AddToRole(user.Id, AdminRole);
            }
        }
    }
}
