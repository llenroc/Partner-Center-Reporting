// -----------------------------------------------------------------------
// <copyright file="ConfigurationProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Reporting.Api.Providers
{
    using System.Configuration;
    using Services;

    /// <summary>
    /// Provides a mechanism for obtian various configuration values.
    /// </summary>
    internal sealed class ConfigurationProvider : IConfigurationProvider
    {
        /// <summary>
        /// Provides access to core application providers.
        /// </summary>
        private readonly IAppService service;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationProvider"/> class.
        /// </summary>
        /// <param name="service">Provides access to core application providers.</param>
        public ConfigurationProvider(IAppService service)
        {
            service.AssertNotNull(nameof(service));
            this.service = service;
        }

        /// <summary>
        /// Gets the Active Directory endpoint address.
        /// </summary>
        public string ActiveDirectoryEndpoint => ConfigurationManager.AppSettings["ActiveDirectoryEndpoint"];

        /// <summary>
        /// Gets the identifier for the application.
        /// </summary>
        public string ApplicationId => ConfigurationManager.AppSettings["ApplicationId"];

        /// <summary>
        /// Gets the identifier for the tenant where the application is provisioned.
        /// </summary>
        public string ApplicationTenantId => ConfigurationManager.AppSettings["ApplicationTenantId"];

        /// <summary>
        /// Gets the audience for authentication purposes.
        /// </summary>
        public string Audience => ConfigurationManager.AppSettings["Audience"];

        /// <summary>
        /// Gets the Microsoft Graph endpoint address.
        /// </summary>
        public string GraphEndpoint => ConfigurationManager.AppSettings["GraphEndpoint"];

        /// <summary>
        /// Gets the Application Insights instrumentation key.
        /// </summary>
        public string InstrumentationKey => ConfigurationManager.AppSettings["InstrumentationKey"];

        /// <summary>
        /// Gets the Key Vault application identifier.
        /// </summary>
        public string KeyVaultApplicationId => ConfigurationManager.AppSettings["KeyVaultApplicationId"];

        /// <summary>
        /// Gets the Key Vault application tenant identifier.
        /// </summary>
        public string KeyVaultApplicationTenantId => ConfigurationManager.AppSettings["KeyVaultApplicationTenantId"];

        /// <summary>
        /// Gets the Key Vault base address.
        /// </summary>
        public string KeyVaultBaseAddress => $"https://{ConfigurationManager.AppSettings["KeyVaultName"]}.vault.azure.net";

        /// <summary>
        /// Gets the identifier for the Partner Center application.
        /// </summary>
        public string PartnerCenterApplicationId => ConfigurationManager.AppSettings["PartnerCenterApplicationId"];

        /// <summary>
        /// Gets the identifier for the tenant where the Partner Center application is provisioned.
        /// </summary>
        public string PartnerCenterApplicationTenantId => ConfigurationManager.AppSettings["PartnerCenterApplicationTenantId"];

        /// <summary>
        /// Gets the address for the Partner Center API.
        /// </summary>
        public string PartnerCenterEndpoint => ConfigurationManager.AppSettings["PartnerCenterEndpoint"];
    }
}