using System;
using System.Net.NetworkInformation;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Owin;
using Website_Course_AVG.Attributes;
using Website_Course_AVG.Models;
using Microsoft.Owin.Host.SystemWeb;
using Microsoft.Owin.Security.Twitter;
using Website_Course_AVG.Managers;
using System.Web;


namespace Website_Course_AVG
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit https://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.  
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Enables the application to temporarily store user information when they are verifying the second factor in the two-factor authentication process.
            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

            // Enables the application to remember the second login verification factor such as phone or email.
            // Once you check this option, your second step of verification during the login process will be remembered on the device where you logged in from.
            // This is similar to the RememberMe option when you log in.
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

            // Uncomment the following lines to enable logging in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            string clientIdTW = Helpers.GetValueFromAppSetting("ClientIdTW");
            string clientSecretTW = Helpers.GetValueFromAppSetting("ClientSecretTW");
            app.UseTwitterAuthentication(new TwitterAuthenticationOptions()
            {
                ConsumerKey = clientIdTW,
                ConsumerSecret = clientSecretTW
            });

            //app.UseTwitterAuthentication(
            //   consumerKey: "ndnrokObeNhBfgyzsb2hnFYyc",
            //   consumerSecret: "Tfq1qMQIWUfHcO1P257QLtOg9vu7ruaCy1t4yWwz52Qxb4IABZ");

            string clientIdFB = Helpers.GetValueFromAppSetting("ClientIdFB");
            string clientSecretFB = Helpers.GetValueFromAppSetting("ClientSecretFB");

            app.UseFacebookAuthentication(
               appId: clientIdFB,
               appSecret: clientSecretFB
            );

            string clientIdGG = Helpers.GetValueFromAppSetting("ClientIdGG");
            string clientSecretGG = Helpers.GetValueFromAppSetting("ClientSecretGG");

            app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            {
                ClientId = clientIdGG,
                ClientSecret = clientSecretGG
            });
        }
    }
}