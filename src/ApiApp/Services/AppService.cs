// -----------------------------------------------------------------------
// <copyright file="AppService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Reporting.Api.Services
{
    using Providers;

    /// <summary>
    /// Provides access to core application providers.
    /// </summary>
    public class AppService : IAppService
    {
        /// <summary>
        /// Provides the ability to cache often used objects. 
        /// </summary>
        private static ICacheProvider cache;

        /// <summary>
        /// Provides the ability to access various configurations.
        /// </summary>
        private static IConfigurationProvider configuration;

        /// <summary>
        /// Provides the ability to manage access tokens.
        /// </summary>
        private static ITokenProvider token;

        /// <summary>
        /// Provides the ability to track telemetry data.
        /// </summary>
        private static ITelemetryProvider telemetry;

        /// <summary>
        ///  Provides the ability to store and retrieve information securely.
        /// </summary>
        private static IVaultProvider vault;

        /// <summary>
        /// Gets or sets the reference to the cache provider.
        /// </summary>
        public ICacheProvider Cache => cache ?? (cache = new RedisCacheProvider(this));

        /// <summary>
        /// Gets the reference to the configuration provider.
        /// </summary>
        public IConfigurationProvider Configuration => configuration ?? (configuration = new ConfigurationProvider(this));

        /// <summary>
        /// Gets or sets the reference to the telemetry provider.
        /// </summary>
        public ITelemetryProvider Telemetry
        {
            get
            {
                if (telemetry != null)
                {
                    return telemetry;
                }

                if (string.IsNullOrEmpty(Configuration.InstrumentationKey))
                {
                    telemetry = new EmptyTelemetryProvider();
                }
                else
                {
                    telemetry = new ApplicationInsightsTelemetryProvider();
                }

                return telemetry;
            }
        }

        /// <summary>
        /// Gets the reference to the token provider.
        /// </summary>
        public ITokenProvider Token => token ?? (token = new TokenProvider(this));

        /// <summary>
        /// Gets the reference to the vault provider.
        /// </summary>
        public IVaultProvider Vault => vault ?? (vault = new KeyVaultProvider(this));
    }
}