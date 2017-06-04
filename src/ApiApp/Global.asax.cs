// -----------------------------------------------------------------------
// <copyright file="Global.asax.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Reporting.Api
{
    using System.Web;
    using System.Web.Http;
    using Practices.Unity;
    using Services;

    /// <summary>
    /// Defines the methods and properties that are common to application objects.
    /// </summary>
    /// <seealso cref="HttpApplication" />
    public class WebApiApplication : HttpApplication
    {
        /// <summary>
        /// Gets the unity container for the application.
        /// </summary>
        internal static IUnityContainer UnityContainer { get; private set; }

        /// <summary>
        /// Called when the application starts.
        /// </summary>
        protected void Application_Start()
        {
            IAppService service;

            try
            {
                UnityContainer = UnityConfig.GetConfiguredContainer();

                service = UnityContainer.Resolve<IAppService>();

                ApplicationInsights.Extensibility.TelemetryConfiguration.Active.InstrumentationKey =
                    service.Configuration.InstrumentationKey;

                GlobalConfiguration.Configure(WebApiConfig.Register);
            }
            finally
            {
                service = null;
            }
        }
    }
}