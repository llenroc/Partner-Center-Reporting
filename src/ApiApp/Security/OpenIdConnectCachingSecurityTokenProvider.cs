// -----------------------------------------------------------------------
// <copyright file="OpenIdConnectCachingSecurityTokenProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Reporting.Api.Security
{
    using System.Collections.Generic;
    using System.IdentityModel.Tokens;
    using System.Threading;
    using IdentityModel.Protocols;
    using Owin.Security.Jwt;

    public class OpenIdConnectCachingSecurityTokenProvider : IIssuerSecurityTokenProvider
    {
        public ConfigurationManager<OpenIdConnectConfiguration> configManager;
        private string issuer;
        private IEnumerable<SecurityToken> tokens;
        private readonly string metadataEndpoint;

        private readonly ReaderWriterLockSlim synclock = new ReaderWriterLockSlim();

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenIdConnectCachingSecurityTokenProvider"/> class.
        /// </summary>
        /// <param name="metadataEndpoint"></param>
        public OpenIdConnectCachingSecurityTokenProvider(string metadataEndpoint)
        {
            this.metadataEndpoint = metadataEndpoint;
            configManager = new ConfigurationManager<OpenIdConnectConfiguration>(metadataEndpoint);

            RetrieveMetadata();
        }

        /// <summary>
        /// Gets the issuer the credentials are for.
        /// </summary>
        public string Issuer
        {
            get
            {
                RetrieveMetadata();

                synclock.EnterReadLock();

                try
                {
                    return issuer;
                }
                finally
                {
                    synclock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Gets all known security tokens.
        /// </summary>
        public IEnumerable<SecurityToken> SecurityTokens
        {
            get
            {
                RetrieveMetadata();

                synclock.EnterReadLock();

                try
                {
                    return tokens;
                }
                finally
                {
                    synclock.ExitReadLock();
                }
            }
        }

        private void RetrieveMetadata()
        {
            synclock.EnterWriteLock();

            try
            {
                OpenIdConnectConfiguration config = configManager.GetConfigurationAsync().Result;
                issuer = config.Issuer;
                tokens = config.SigningTokens;
            }
            finally
            {
                synclock.ExitWriteLock();
            }
        }
    }
}