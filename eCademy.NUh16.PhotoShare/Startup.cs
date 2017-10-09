using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(eCademy.NUh16.PhotoShare.Startup))]
namespace eCademy.NUh16.PhotoShare
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
