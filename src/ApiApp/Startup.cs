// -----------------------------------------------------------------------
// <copyright file="Startup.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

[assembly: Microsoft.Owin.OwinStartup(typeof(Microsoft.Store.PartnerCenter.Reporting.Api.Startup))]

namespace Microsoft.Store.PartnerCenter.Reporting.Api
{
    using global::Owin;

    /// <summary>
    /// Manages the application startup.
    /// </summary>
    public partial class Startup
    {
        /// <summary>
        /// Configures the specified application.
        /// </summary>
        /// <param name="app">The application.</param>
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}