// -----------------------------------------------------------------------
// <copyright file="KeyVaultProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Reporting.Api.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Security;
    using System.Threading.Tasks;
    using Azure.KeyVault;
    using Azure.KeyVault.Models;
    using Services;

    /// <summary>
    /// Provides a secure mechanism for retrieving and store information. 
    /// </summary>
    internal sealed class KeyVaultProvider : IVaultProvider
    {
        /// <summary>
        /// Provides access to core application providers.
        /// </summary>
        private readonly IAppService service;

        /// <summary>
        /// Error code returned when a secret is not found in the vault.
        /// </summary>
        private const string NotFoundErrorCode = "SecretNotFound";

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyVaultProvider"/> class.
        /// </summary>
        /// <param name="service">Provides access to core application providers.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="service"/> is null.
        /// </exception>
        public KeyVaultProvider(IAppService service)
        {
            service.AssertNotNull(nameof(service));
            this.service = service;
        }

        /// <summary>
        /// Gets the specified entity from the vault. 
        /// </summary>
        /// <param name="identifier">Identifier of the entity to be retrieved.</param>
        /// <returns>The value retrieved from the vault.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="identifier"/> is empty or null.
        /// </exception>
        public async Task<SecureString> GetAsync(string identifier)
        {
            DateTime startTime;
            Dictionary<string, double> eventMetrics;
            Dictionary<string, string> eventProperties;
            SecretBundle bundle;

            identifier.AssertNotEmpty(nameof(identifier));

            try
            {
                startTime = DateTime.Now;

                using (IKeyVaultClient client = new KeyVaultClient(service.Token.GetAppOnlyTokenAsync))
                {
                    try
                    {
                        bundle = await client.GetSecretAsync(service.Configuration.KeyVaultBaseAddress, identifier);
                    }
                    catch (KeyVaultErrorException ex)
                    {
                        if (ex.Body.Error.Code.Equals(NotFoundErrorCode, StringComparison.CurrentCultureIgnoreCase))
                        {
                            bundle = null;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                // Track the event measurements for analysis.
                eventMetrics = new Dictionary<string, double>
                {
                    { "ElapsedMilliseconds", DateTime.Now.Subtract(startTime).TotalMilliseconds }
                };

                // Capture the request for the customer summary for analysis.
                eventProperties = new Dictionary<string, string>
                {
                    { "Identifier", identifier }
                };

                service.Telemetry.TrackEvent("Vault/GetAsync", eventProperties, eventMetrics);

                return bundle?.Value.ToSecureString();
            }
            finally
            {
                bundle = null;
                eventMetrics = null;
                eventProperties = null;
            }
        }

        /// <summary>
        /// Stores the specified value in the vault.
        /// </summary>
        /// <param name="identifier">Identifier of the entity to be stored.</param>
        /// <param name="value">The value to be stored.</param>
        /// <returns>An instance of <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="identifier"/> is empty or null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="value"/> is null.
        /// </exception>
        public async Task StoreAsync(string identifier, SecureString value)
        {
            DateTime startTime;
            Dictionary<string, double> eventMetrics;
            Dictionary<string, string> eventProperties;

            identifier.AssertNotEmpty(nameof(identifier));
            value.AssertNotNull(nameof(value));

            try
            {
                startTime = DateTime.Now;

                using (IKeyVaultClient client = new KeyVaultClient(service.Token.GetAppOnlyTokenAsync))
                {
                    await client.SetSecretAsync(
                        service.Configuration.KeyVaultBaseAddress, identifier, value.ToUnsecureString());
                }

                // Track the event measurements for analysis.
                eventMetrics = new Dictionary<string, double>
                {
                    { "ElapsedMilliseconds", DateTime.Now.Subtract(startTime).TotalMilliseconds }
                };

                // Capture the request for the customer summary for analysis.
                eventProperties = new Dictionary<string, string>
                {
                    { "Identifier", identifier }
                };

                service.Telemetry.TrackEvent("Vault/StoreAsync", eventProperties, eventMetrics);
            }
            finally
            {
                eventMetrics = null;
                eventProperties = null;
            }
        }
    }
}