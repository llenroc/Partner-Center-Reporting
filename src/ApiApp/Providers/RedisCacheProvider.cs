// -----------------------------------------------------------------------
// <copyright file="RedisCacheProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Reporting.Api.Providers
{
    using System;
    using System.Security;
    using System.Threading.Tasks;
    using Cache;
    using Newtonsoft.Json;
    using Security;
    using Services;
    using StackExchange.Redis;

    /// <summary>
    /// Provides the ability to cache resources using an instance of Redis Cache.
    /// </summary>
    internal sealed class RedisCacheProvider : ICacheProvider
    {
        /// <summary>
        /// Provides access to core application providers.
        /// </summary>
        private readonly IAppService service;

        /// <summary>
        /// Provides the ability to protect data.
        /// </summary>
        private readonly IDataProtector protector;

        /// <summary>
        /// Provides the ability to interact with an instance of Redis Cache.
        /// </summary>
        private IConnectionMultiplexer connection;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisCacheProvider"/> class.
        /// </summary>
        /// <param name="service">Provides access to core services.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="service"/> is null.
        /// </exception>
        public RedisCacheProvider(IAppService service)
        {
            service.AssertNotNull(nameof(service));

            protector = new MachineKeyDataProtector(new[] { typeof(RedisCacheProvider).FullName });
            this.service = service;
        }

        /// <summary>
        /// Removes all entities from the specified cache database. 
        /// </summary>
        /// <param name="database">Cache database type where the data is stored.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public async Task ClearAsync(CacheDatabaseType database)
        {
            await Task.FromResult(0);
        }

        /// <summary>
        /// Deletes the entity with specified key.
        /// </summary>
        /// <param name="database">Cache database type where the data is stored.</param>
        /// <param name="key">The key of the entity to be deleted.</param>
        /// <returns>An instance of <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="key"/> is empty or null.
        /// </exception>
        public async Task DeleteAsync(CacheDatabaseType database, string key = null)
        {
            IDatabase cache = await GetCacheReferenceAsync(database);
            await cache.KeyDeleteAsync(key);
        }

        /// <summary>
        /// Fetches the specified entity from the cache.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity in cache.</typeparam>
        /// <param name="database">Cache database type where the data is stored.</param>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <returns>
        /// The entity associated with the specified key.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="key"/> is empty or null.
        /// </exception>
        public TEntity Fetch<TEntity>(CacheDatabaseType database, string key) where TEntity : class
        {
            key.AssertNotEmpty(nameof(key));

            IDatabase cache = GetCacheReference(database);
            RedisValue value = cache.StringGet(key);

            return value.HasValue ? JsonConvert.DeserializeObject<TEntity>(protector.Unprotect(value)) : null;
        }

        /// <summary>
        /// Fetches the specified entity from the cache.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity in cache.</typeparam>
        /// <param name="database">Cache database type where the data is stored.</param>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <returns>
        /// The entity associated with the specified key.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="key"/> is empty or null.
        /// </exception>
        public async Task<TEntity> FetchAsync<TEntity>(CacheDatabaseType database, string key) where TEntity : class
        {
            key.AssertNotEmpty(nameof(key));

            IDatabase cache = await GetCacheReferenceAsync(database);
            RedisValue value = await cache.StringGetAsync(key);

            return value.HasValue ? JsonConvert.DeserializeObject<TEntity>(protector.Unprotect(value)) : null;
        }

        /// <summary>
        /// Stores the specified entity in the cache.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity in cache.</typeparam>
        /// <param name="database">Cache database type where the data should be stored.</param>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <param name="entity">The object to be cached.</param>
        /// <param name="expiration">When the entity in the cache should expire.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="key"/> is empty or null.
        /// </exception>
        /// <exception cref="ArgumentNullException"> 
        /// entity
        /// </exception>
        public async Task StoreAsync<TEntity>(CacheDatabaseType database, string key, TEntity entity, TimeSpan? expiration = null)
            where TEntity : class
        {
            key.AssertNotEmpty(nameof(key));
            entity.AssertNotNull(nameof(entity));

            IDatabase cache = await GetCacheReferenceAsync(database);

            await cache.StringSetAsync(
                key, protector.Protect(JsonConvert.SerializeObject(entity)), expiration);
        }

        /// <summary>
        /// Removes all entities from the specified cache database. 
        /// </summary>
        /// <param name="database">Cache database type where the data is stored.</param>
        public void Clear(CacheDatabaseType database)
        {
        }

        /// <summary>
        /// Deletes the entity with specified key.
        /// </summary>
        /// <param name="database">Cache database type where the data is stored.</param>
        /// <param name="key">The key of the entity to be deleted.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="key"/> is empty or null.
        /// </exception>
        public void Delete(CacheDatabaseType database, string key = null)
        {
            IDatabase cache = GetCacheReference(database);
            cache.KeyDelete(key);
        }

        /// <summary>
        /// Stores the specified entity in the cache.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity in cache.</typeparam>
        /// <param name="database">Cache database type where the data should be stored.</param>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <param name="entity">The object to be cached.</param>
        /// <param name="expiration">When the entity in the cache should expire.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="key"/> is empty or null.
        /// </exception>
        /// <exception cref="ArgumentNullException"> 
        /// entity
        /// </exception>
        public void Store<TEntity>(CacheDatabaseType database, string key, TEntity entity, TimeSpan? expiration = null)
            where TEntity : class
        {
            key.AssertNotEmpty(nameof(key));
            entity.AssertNotNull(nameof(entity));

            IDatabase cache = GetCacheReference(database);

            cache.StringSet(
                key, protector.Protect(JsonConvert.SerializeObject(entity)), expiration);
        }

        /// <summary>
        /// Obtains a reference to the specified cache database.
        /// </summary>
        /// <param name="database">Cache database type where the data is stored.</param>
        /// <returns>A reference to the appropriate cache database.</returns>
        private IDatabase GetCacheReference(CacheDatabaseType database)
        {
            if (connection == null)
            {
                using (SecureString redisCacheConnectionString = SynchronousExecute(() => service.Vault.GetAsync("RedisCacheConnectionString")))
                {
                    connection = ConnectionMultiplexer.Connect(
                        redisCacheConnectionString.ToUnsecureString());
                }
            }

            return connection.GetDatabase((int)database);
        }

        /// <summary>
        /// Obtains a reference to the specified cache database.
        /// </summary>
        /// <param name="database">Cache database type where the data is stored.</param>
        /// <returns>A reference to the appropriate cache database.</returns>
        private async Task<IDatabase> GetCacheReferenceAsync(CacheDatabaseType database)
        {
            if (connection == null)
            {
                using (SecureString redisCacheConnectionString = await service.Vault.GetAsync("RedisCacheConnectionString"))
                {
                    connection = await ConnectionMultiplexer.ConnectAsync(
                        redisCacheConnectionString.ToUnsecureString());
                }
            }

            return connection.GetDatabase((int)database);
        }


        /// <summary>
        /// Executes an asynchronous method synchronously
        /// </summary>
        /// <typeparam name="T">The type to be returned.</typeparam>
        /// <param name="operation">The asynchronous operation to be executed.</param>
        /// <returns>The result from the operation.</returns>
        private static T SynchronousExecute<T>(Func<Task<T>> operation)
        {
            try
            {
                return Task.Run(async () => await operation()).Result;
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException != null)
                {
                    throw ex.InnerException;
                }

                throw;
            }
        }
    }
}