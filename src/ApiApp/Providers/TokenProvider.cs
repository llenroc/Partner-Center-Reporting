// -----------------------------------------------------------------------
// <copyright file="TokenProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Reporting.Api.Providers
{
    using System;
    using System.Configuration;
    using System.Linq;
    using System.Security;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Cache;
    using IdentityModel.Clients.ActiveDirectory;
    using Models;
    using Services;
    using Store.PartnerCenter;
    using Store.PartnerCenter.Extensions;

    /// <summary>
    /// Provides the ability to manage access tokens.
    /// </summary>
    internal sealed class TokenProvider : ITokenProvider
    {
        /// <summary>
        /// Provides access to core application providers.
        /// </summary>
        private readonly IAppService service;

        /// <summary>
        /// Type of the assertion representing the user when performing app + user authentication.
        /// </summary>
        private const string AssertionType = "urn:ietf:params:oauth:grant-type:jwt-bearer";

        /// <summary>
        /// Key utilized to retrieve and store Partner Center access tokens. 
        /// </summary>
        private const string PartnerCenterCacheKey = "Resource::PartnerCenter::AppOnly";

        /// <summary>
        /// Initializes a new instance of <see cref="TokenProvider"/> class.
        /// </summary>
        /// <param name="service">Provides access to core application providers.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="service"/> is null.
        /// </exception>
        public TokenProvider(IAppService service)
        {
            service.AssertNotNull(nameof(service));
            this.service = service;
        }

        /// <summary>
        /// Gets the token for the current authenticated user.
        /// </summary>
        private static string UserAssertionToken
        {
            get
            {
                System.IdentityModel.Tokens.BootstrapContext bootstrapContext;

                try
                {
                    bootstrapContext = ClaimsPrincipal.Current.Identities.First().BootstrapContext as System.IdentityModel.Tokens.BootstrapContext;

                    return bootstrapContext?.Token;
                }
                finally
                {
                    bootstrapContext = null;
                }
            }
        }

        /// <summary>
        /// Gets an access token from the authority using app only authentication.
        /// </summary>
        /// <param name="authority">Address of the authority to issue the token.</param>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <returns>An instance of <see cref="AuthenticationToken"/> that represented the access token.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="authority"/> is empty or null.
        /// or
        /// <paramref name="resource"/> is empty or null.
        /// </exception>
        public async Task<AuthenticationResult> GetAppOnlyTokenAsync(string authority, string resource)
        {
            AuthenticationContext authContext;
            AuthenticationResult authResult;
            DistributedTokenCache tokenCache;
            ISecureClientSecret secret;

            authority.AssertNotEmpty(nameof(authority));
            resource.AssertNotEmpty(nameof(resource));

            try
            {
                tokenCache = new DistributedTokenCache(service, resource, $"AppOnly::{authority}::{resource}");
                authContext = new AuthenticationContext(authority, tokenCache);

                using (SecureString applicationSecret = await service.Vault.GetAsync("ApplicationSecret"))
                {
                    secret = new SecureClientSecret(applicationSecret);

                    authResult = await authContext.AcquireTokenAsync(
                        resource,
                        new ClientCredential(
                            service.Configuration.ApplicationId,
                            secret));
                }

                return authResult;
            }
            finally
            {
                authContext = null;
                authResult = null;
                secret = null;
                tokenCache = null;
            }
        }

        /// <summary>
        /// Gets an access token from the authority using app only authentication.
        /// </summary>
        /// <param name="authority">Address of the authority to issue the token.</param>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <param name="scope">Permissions the requested token will need.</param>
        /// <returns>A string that represented the access token.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="authority"/> is empty or null.
        /// or
        /// <paramref name="resource"/> is empty or null.
        /// </exception>
        public async Task<string> GetAppOnlyTokenAsync(string authority, string resource, string scope)
        {
            AuthenticationContext authContext;
            AuthenticationResult authResult;
            ISecureClientSecret secret;

            authority.AssertNotEmpty(nameof(authority));
            resource.AssertNotEmpty(nameof(resource));

            try
            {
                authContext = new AuthenticationContext(authority);

                using (SecureString keyVaultApplicationSecret = ConfigurationManager.AppSettings["KeyVaultApplicationSecret"].ToSecureString())
                {
                    secret = new SecureClientSecret(keyVaultApplicationSecret);

                    authResult = await authContext.AcquireTokenAsync(
                    resource,
                    new ClientCredential(
                        service.Configuration.KeyVaultApplicationId,
                        secret));
                }

                return authResult.AccessToken;
            }
            finally
            {
                authContext = null;
                authResult = null;
                secret = null;
            }
        }

        /// <summary>
        /// Gets an access token from the authority using app + user authentication.
        /// </summary>
        /// <param name="authority">Address of the authority to issue the token.</param>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <returns>An instance of <see cref="AuthenticationToken"/> that represented the access token.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="authority"/> is empty or null.
        /// or
        /// <paramref name="resource"/> is empty or null.
        /// </exception>
        public async Task<AuthenticationResult> GetAppPlusUserTokenAsync(string authority, string resource)
        {
            AuthenticationContext authContext;
            AuthenticationResult authResult;
            DistributedTokenCache tokenCache;
            ISecureClientSecret secret;

            authority.AssertNotEmpty(nameof(authority));
            resource.AssertNotEmpty(nameof(resource));

            try
            {
                tokenCache = new DistributedTokenCache(service, resource);
                authContext = new AuthenticationContext(authority, tokenCache);

                using (SecureString applicationSecret = await service.Vault.GetAsync("ApplicationSecret"))
                {
                    secret = new SecureClientSecret(applicationSecret);

                    try
                    {
                        authResult = await authContext.AcquireTokenAsync(
                            resource,
                            new ClientCredential(
                                service.Configuration.ApplicationId,
                                secret),
                            new UserAssertion(UserAssertionToken, AssertionType));
                    }
                    catch (AdalServiceException ex)
                    {
                        if (ex.ErrorCode.Equals("AADSTS70002", StringComparison.CurrentCultureIgnoreCase))
                        {
                            await service.Cache.DeleteAsync(CacheDatabaseType.Authentication);

                            authResult = await authContext.AcquireTokenAsync(
                                resource,
                                new ClientCredential(
                                    service.Configuration.ApplicationId,
                                    secret),
                                new UserAssertion(UserAssertionToken, AssertionType));
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                return authResult;
            }
            finally
            {
                authContext = null;
                authResult = null;
                secret = null;
                tokenCache = null;
            }
        }

        /// <summary>
        /// Gets an instance of <see cref="IPartnerCredentials"/> used to access the Partner Center Managed API.
        /// </summary>
        /// <param name="authority">Address of the authority to issue the token.</param>
        /// <returns>
        /// An instance of <see cref="IPartnerCredentials" /> that represents the access token.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="authority"/> is empty or null.
        /// </exception>
        public async Task<IPartnerCredentials> GetPartnerCenterAppOnlyCredentialsAsync(string authority)
        {
            authority.AssertNotEmpty(nameof(authority));

            // Attempt to obtain the Partner Center token from the cache.
            IPartnerCredentials credentials =
                 await service.Cache.FetchAsync<PartnerCenterTokenModel>(
                     CacheDatabaseType.Authentication, PartnerCenterCacheKey);

            if (credentials != null && !credentials.IsExpired())
            {
                return credentials;
            }

            using (SecureString partnerCenterApplicationSecret = await service.Vault.GetAsync("PartnerCenterApplicationSecret"))
            {
                // The access token has expired, so a new one must be requested.
                credentials = await PartnerCredentials.Instance.GenerateByApplicationCredentialsAsync(
                    service.Configuration.PartnerCenterApplicationId,
                    partnerCenterApplicationSecret.ToUnsecureString(),
                    service.Configuration.PartnerCenterApplicationTenantId);
            }

            await service.Cache.StoreAsync(CacheDatabaseType.Authentication, PartnerCenterCacheKey, credentials);

            return credentials;
        }

        /// <summary>
        /// Gets an instance of <see cref="IPartnerCredentials"/> used to access the Partner Center API.
        /// </summary>
        /// <param name="authority">Address of the authority to issue the token.</param>
        /// <returns>
        /// An instance of <see cref="IPartnerCredentials" /> that represents the access token.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="authority"/> is empty or null.
        /// </exception>
        public async Task<IPartnerCredentials> GetPartnerCenterAppPlusUserCredentialsAsync(string authority)
        {
            authority.AssertNotEmpty(nameof(authority));

            string key = $"Resource::PartnerCenter::{ClaimsPrincipal.Current.Identities.First().FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value}";

            IPartnerCredentials credentials =
                 await service.Cache.FetchAsync<PartnerCenterTokenModel>(CacheDatabaseType.Authentication, key);

            if (credentials != null && !credentials.IsExpired())
            {
                return credentials;
            }

            AuthenticationResult token = await GetAppPlusUserTokenAsync(
                 authority,
                 service.Configuration.PartnerCenterEndpoint);

            credentials = await PartnerCredentials.Instance.GenerateByUserCredentialsAsync(
                service.Configuration.PartnerCenterApplicationId,
                new AuthenticationToken(token.AccessToken, token.ExpiresOn));

            await service.Cache.StoreAsync(
               CacheDatabaseType.Authentication, key, credentials);

            return credentials;
        }
    }
}