// -----------------------------------------------------------------------
// <copyright file="IConfigurationProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Reporting.Api.Providers
{
    /// <summary>
    /// Represents a mechanism for obtian various configuration values.
    /// </summary>
    public interface IConfigurationProvider
    {
        /// <summary>
        /// Gets the Active Directory endpoint address.
        /// </summary>
        string ActiveDirectoryEndpoint { get; }

        /// <summary>
        /// Gets the identifier for the application.
        /// </summary>
        string ApplicationId { get; }

        /// <summary>
        /// Gets the identifier for the tenant where the application is provisioned.
        /// </summary>
        string ApplicationTenantId { get; }

        /// <summary>
        /// Gets the audience for authentication purposes.
        /// </summary>
        string Audience { get; }

        /// <summary>
        /// Gets the Microsoft Graph endpoint address.
        /// </summary>
        string GraphEndpoint { get; }

        /// <summary>
        /// Gets the Application Insights instrumentation key.
        /// </summary>
        string InstrumentationKey { get; }

        /// <summary>
        /// Gets the Key Vault application identifier.
        /// </summary>
        string KeyVaultApplicationId { get; }

        /// <summary>
        /// Gets the Key Vault application tenant identifier.
        /// </summary>
        string KeyVaultApplicationTenantId { get; }

        /// <summary>
        /// Gets the Key Vault base address.
        /// </summary>
        string KeyVaultBaseAddress { get; }

        /// <summary>
        /// Gets the identifier for the Partner Center application.
        /// </summary>
        string PartnerCenterApplicationId { get; }

        /// <summary>
        /// Gets the identifier for the tenant where the Partner Center application is provisioned.
        /// </summary>
        string PartnerCenterApplicationTenantId { get; }

        /// <summary>
        /// Gets the address for the Partner Center API.
        /// </summary>
        string PartnerCenterEndpoint { get; }
    }
}