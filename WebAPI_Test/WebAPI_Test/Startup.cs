using System;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Security.OAuth;
using Owin;
using WebAPI_Test.Providers;

[assembly: OwinStartup(typeof(WebAPI_Test.Startup))]

namespace WebAPI_Test
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);

            hPayAuthorizationServerProvider cxcAccessTokenProvider = new hPayAuthorizationServerProvider();
            hPayRefreshTokenProvider cxcRefreshTokenProvider = new hPayRefreshTokenProvider();

            OAuthAuthorizationServerOptions options = new OAuthAuthorizationServerOptions
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromSeconds(30),
                Provider = cxcAccessTokenProvider,
                RefreshTokenProvider = cxcRefreshTokenProvider
            };

            app.UseOAuthAuthorizationServer(options);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());
            

            HttpConfiguration config = new HttpConfiguration();
            WebApiConfig.Register(config);
        }
    }
}
