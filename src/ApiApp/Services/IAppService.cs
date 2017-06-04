// -----------------------------------------------------------------------
// <copyright file="IAppService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Reporting.Api.Services
{
    using Providers;

    public interface IAppService
    {
        /// <summary>
        /// Gets the reference to the cache provider.
        /// </summary>
        ICacheProvider Cache { get; }

        /// <summary>
        /// Gets the reference to the configuration provider.
        /// </summary>
        IConfigurationProvider Configuration { get; }

        /// <summary>
        /// Gets the reference to the telemetry provider.
        /// </summary>
        ITelemetryProvider Telemetry { get; }

        /// <summary>
        /// Gets the reference to the token provider.
        /// </summary>
        ITokenProvider Token { get; }

        /// <summary>
        /// Gets the reference to the vault provider.k
        /// </summary>
        IVaultProvider Vault { get; }
    }
}