// -----------------------------------------------------------------------
// <copyright file="Startup.Auth.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Reporting.Api
{
    using System.IdentityModel.Tokens;
    using global::Owin;
    using Microsoft.Owin.Security.Jwt;
    using Providers;
    using Owin.Security.OAuth;
    using Practices.Unity;
    using Security;
    using Services;

    /// <summary>
    /// Provides methods and properties used to start the application.
    /// </summary>
    public partial class Startup
    {
        /// <summary>
        /// Configures authentication for the application.
        /// </summary>
        /// <param name="app">The application to be configured.</param>
        public void ConfigureAuth(IAppBuilder app)
        {
            IAppService service = WebApiApplication.UnityContainer.Resolve<IAppService>();

            TokenValidationParameters tvps = new TokenValidationParameters
            {
                SaveSigninToken = true,
                ValidAudience = service.Configuration.Audience,
                ValidateIssuer = false,
            };

            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions
            {
                AccessTokenFormat = new JwtFormat(tvps, new OpenIdConnectCachingSecurityTokenProvider("https://login.microsoftonline.com/common/.well-known/openid-configuration")),
                AccessTokenProvider = new AuthenticationTokenProvider(),
                Challenge = $"authorization_uri=\"{service.Configuration.ActiveDirectoryEndpoint}/common/oauth2/authorize\""
        });
        }
    }
}