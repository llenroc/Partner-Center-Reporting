// -----------------------------------------------------------------------
// <copyright file="WebApiConfig.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Reporting.Api
{
    using System.Web.Http;
    using Unity.WebApi;

    /// <summary>
    /// Provides the configurations for the Web API. 
    /// </summary>
    public static class WebApiConfig
    {
        /// <summary>
        /// Registers the dependency resolver and routes.
        /// </summary>
        /// <param name="config">An instance of <see cref="HttpConfiguration"/> to be configured.</param>
        public static void Register(HttpConfiguration config)
        {
            config.DependencyResolver = new UnityDependencyResolver(WebApiApplication.UnityContainer);
            config.MapHttpAttributeRoutes();
        }
    }
}