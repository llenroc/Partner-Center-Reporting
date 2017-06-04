// -----------------------------------------------------------------------
// <copyright file="DistributedTokenCache.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Reporting.Api.Cache
{
    using System;
    using System.Linq;
    using System.Security.Claims;
    using IdentityModel.Clients.ActiveDirectory;
    using Services;

    /// <summary>
    /// Custom implementation of the <see cref="TokenCache"/> class.
    /// </summary>
    /// <seealso cref="TokenCache" />
    public class DistributedTokenCache : TokenCache
    {
        /// <summary>
        /// Claim type for the object identifier claim.
        /// </summary>
        private const string ObjectIdClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";

        /// <summary>
        /// The unique identifier for the cache entry.
        /// </summary>
        private readonly string keyValue;

        /// <summary>
        /// Resource that is being accessed.
        /// </summary>
        private readonly string resource;

        /// <summary>
        /// Provides access to the core services.
        /// </summary>
        private readonly IAppService service;

        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedTokenCache"/> class.
        /// </summary>
        /// <param name="service">Provides access to core services.</param>
        /// <param name="resource">The resource being accessed.</param>
        /// <param name="key">The unique identifier for the cache entry.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="resource"/> is empty or null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="service"/> is null.
        /// </exception>
        public DistributedTokenCache(IAppService service, string resource, string key = null)
        {
            service.AssertNotNull(nameof(service));
            resource.AssertNotEmpty(nameof(resource));

            this.service = service;
            keyValue = key;
            this.resource = resource;

            AfterAccess = AfterAccessNotification;
            BeforeAccess = BeforeAccessNotification;
        }

        /// <summary>
        /// Gets the unique identifier for the cache entry.
        /// </summary>
        private string Key => string.IsNullOrEmpty(keyValue) ? $"Resource::{resource}::Identifier::{ClaimsPrincipal.Current.Identities.First().FindFirst(ObjectIdClaimType).Value}" : keyValue;

        /// <summary>
        /// Notification method called after any library method accesses the cache.
        /// </summary>
        /// <param name="args">Contains parameters used by the ADAL call accessing the cache.</param>
        public void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            if (!HasStateChanged)
            {
                return;
            }

            if (Count > 0)
            {
                service.Cache.Store(
                    CacheDatabaseType.Authentication, Key, Convert.ToBase64String(Serialize()));
            }
            else
            {
                service.Cache.Delete(CacheDatabaseType.Authentication, Key);
            }

            HasStateChanged = false;
        }

        /// <summary>
        /// Notification method called before any library method accesses the cache.
        /// </summary>
        /// <param name="args">Contains parameters used by the ADAL call accessing the cache.</param>
        public void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            string value = service.Cache.Fetch<string>(CacheDatabaseType.Authentication, Key);

            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            Deserialize(Convert.FromBase64String(value));
        }

        /// <summary>
        /// Clears the cache by deleting all the items. Note that if the cache is the default shared cache, clearing it would
        /// impact all the instances of <see cref="T:Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext" /> which share that cache.
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            service.Cache.Clear(CacheDatabaseType.Authentication);
        }

        /// <summary>
        /// Deletes an item from the cache.
        /// </summary>
        /// <param name="item">The item to delete from the cache.</param>
        public override void DeleteItem(TokenCacheItem item)
        {
            base.DeleteItem(item);
            service.Cache.Delete(CacheDatabaseType.Authentication, Key);
        }
    }
}