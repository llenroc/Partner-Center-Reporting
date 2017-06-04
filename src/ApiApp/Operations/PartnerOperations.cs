// -----------------------------------------------------------------------
// <copyright file="PartnerOperations.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Reporting.Api.Operations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Cache;
    using Models;
    using Practices.Unity;
    using Security;
    using Services;
    using Store.PartnerCenter;
    using Store.PartnerCenter.Enumerators;
    using Store.PartnerCenter.Models;
    using Store.PartnerCenter.Models.Customers;
    using Store.PartnerCenter.Models.RateCards;
    using Store.PartnerCenter.Models.Subscriptions;
    using Store.PartnerCenter.Models.Utilizations;
    using Store.PartnerCenter.RequestContext;

    /// <summary>
    /// Provides the ability to perform various partner operations.
    /// </summary>
    public class PartnerOperations : IPartnerOperations
    {
        /// <summary>
        /// Key utilized when interacting with the cache for available offers.
        /// </summary>
        private const string OffersKey = "AvailableOffers";

        /// <summary>
        /// Key utilized when interacting with the cachce for Azure RateCard.
        /// </summary>
        private const string RateCardKey = "RateCard";

        /// <summary>
        /// Provides the ability to perform partner operation using app only authentication.
        /// </summary>
        private IAggregatePartner appOperations;

        /// <summary>
        /// Provides access to core application providers.
        /// </summary>
        private IAppService service;

        /// <summary>
        /// Provides a way to ensure that <see cref="appOperations"/> is only being modified 
        /// by one thread at a time. 
        /// </summary>
        private object appLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="PartnerOperations"/> class.
        /// </summary>
        public PartnerOperations()
        {
            service = WebApiApplication.UnityContainer.Resolve<IAppService>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartnerOperations"/> class.
        /// </summary>
        /// <param name="service">Provides access to core application providers.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="service"/> is null.
        /// </exception>
        public PartnerOperations(IAppService service)
        {
            service.AssertNotNull(nameof(service));
            this.service = service;
        }

        /// <summary>
        /// Checks if the specified domain is available.
        /// </summary>
        /// <param name="domain">Domain to be checked.</param>
        /// <returns><c>true</c> if the domain is available; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="domain"/> is empty or null.
        /// </exception>
        public async Task<bool> CheckDomainAsync(string domain)
        {
            CustomerPrincipal principal;
            DateTime startTime;
            Dictionary<string, double> eventMetrics;
            Dictionary<string, string> eventProperties;
            Guid correlationId;
            IPartner operations;
            bool exists;

            domain.AssertNotEmpty(nameof(domain));

            try
            {
                startTime = DateTime.Now;
                correlationId = Guid.NewGuid();
                operations = await GetAppOperationsAsync(correlationId);

                principal = new CustomerPrincipal(ClaimsPrincipal.Current);

                exists = await operations.Domains.ByDomain(domain).ExistsAsync();

                // Track the event measurements for analysis.
                eventMetrics = new Dictionary<string, double>
                {
                    { "ElapsedMilliseconds", DateTime.Now.Subtract(startTime).TotalMilliseconds }
                };

                // Capture the request for the customer summary for analysis.
                eventProperties = new Dictionary<string, string>
                {
                    { "CustomerId", principal.CustomerId },
                    { "Domain", domain },
                    { "Exists", exists.ToString() },
                    { "Name", principal.Name },
                    { "ParternCenterCorrelationId", correlationId.ToString() }
                };

                service.Telemetry.TrackEvent("CheckDomainAsync", eventProperties, eventMetrics);

                return exists;
            }
            finally
            {
                eventMetrics = null;
                eventProperties = null;
                operations = null;
                principal = null;
            }
        }

        /// <summary>
        /// Gets the specified customer.
        /// </summary>
        /// <param name="customerId">Identifier for the customer.</param>
        /// <returns>An instance of <see cref="Customer"/> that represents the specified customer.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="customerId"/> is empty or null.
        /// </exception>
        public async Task<Customer> GetCustomerAsync(string customerId)
        {
            Customer customer;
            CustomerPrincipal principal;
            DateTime startTime;
            Dictionary<string, double> eventMetrics;
            Dictionary<string, string> eventProperties;
            Guid correlationId;
            IPartner operations;

            customerId.AssertNotEmpty(nameof(customerId));

            try
            {
                startTime = DateTime.Now;
                correlationId = Guid.NewGuid();
                operations = await GetAppOperationsAsync(correlationId);

                principal = new CustomerPrincipal(ClaimsPrincipal.Current);

                if (principal.CustomerId.Equals(service.Configuration.PartnerCenterApplicationTenantId))
                {
                    customer = await operations.Customers.ById(customerId).GetAsync();
                }
                else
                {
                    customer = await operations.Customers.ById(principal.CustomerId).GetAsync();
                }

                // Track the event measurements for analysis.
                eventMetrics = new Dictionary<string, double>
                {
                    { "ElapsedMilliseconds", DateTime.Now.Subtract(startTime).TotalMilliseconds }
                };

                // Capture the request for the customer summary for analysis.
                eventProperties = new Dictionary<string, string>
                {
                    { "CustomerId", principal.CustomerId },
                    { "Name", principal.Name },
                    { "ParternCenterCorrelationId", correlationId.ToString() }
                };

                service.Telemetry.TrackEvent("GetCustomerAsync", eventProperties, eventMetrics);

                return customer;
            }
            finally
            {
                eventMetrics = null;
                eventProperties = null;
                operations = null;
                principal = null;
            }
        }

        /// <summary>
        /// Gets the available customers.
        /// </summary>
        /// <returns>A list of available customers.</returns>
        public async Task<List<Customer>> GetCustomersAsync()
        {
            Customer customer;
            CustomerPrincipal principal;
            DateTime startTime;
            Dictionary<string, double> eventMetrics;
            Dictionary<string, string> eventProperties;
            Guid correlationId;
            IPartner operations;
            IResourceCollectionEnumerator<SeekBasedResourceCollection<Customer>> customersEnumerator;
            List<Customer> customers;
            SeekBasedResourceCollection<Customer> seekCustomers;

            try
            {
                startTime = DateTime.Now;
                correlationId = Guid.NewGuid();
                operations = await GetAppOperationsAsync(correlationId);

                principal = new CustomerPrincipal(ClaimsPrincipal.Current);

                customers = new List<Customer>();

                if (principal.CustomerId.Equals(service.Configuration.PartnerCenterApplicationTenantId))
                {
                    seekCustomers = await operations.Customers.GetAsync();
                    customersEnumerator = operations.Enumerators.Customers.Create(seekCustomers);

                    while (customersEnumerator.HasValue)
                    {
                        customers.AddRange(customersEnumerator.Current.Items);
                        await customersEnumerator.NextAsync();
                    }
                }
                else
                {
                    customer = await operations.Customers.ById(principal.CustomerId).GetAsync();
                    customers.Add(customer);
                }

                // Track the event measurements for analysis.
                eventMetrics = new Dictionary<string, double>
                {
                    { "ElapsedMilliseconds", DateTime.Now.Subtract(startTime).TotalMilliseconds },
                    { "NumberOfCustomers", customers.Count }
                };

                // Capture the request for the customer summary for analysis.
                eventProperties = new Dictionary<string, string>
                {
                    { "CustomerId", principal.CustomerId },
                    { "Name", principal.Name },
                    { "ParternCenterCorrelationId", correlationId.ToString() }
                };

                service.Telemetry.TrackEvent("GetCustomersAsync", eventProperties, eventMetrics);

                return customers;
            }
            finally
            {
                customersEnumerator = null;
                eventMetrics = null;
                eventProperties = null;
                operations = null;
                principal = null;
                seekCustomers = null;
            }
        }

        /// <summary>
        /// Gets the specified subscription.
        /// </summary>
        /// <param name="customerId">Identifier for the customer.</param>
        /// <param name="subscriptionId">Identifier for the subscription.</param>
        /// <returns>An instance of <see cref="Subscription"/> that represents the subscription.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="customerId"/> is empty or null.
        /// or 
        /// <paramref name="subscriptionId"/> is empty or null.
        /// </exception>
        public async Task<Subscription> GetSubscriptionAsync(string customerId, string subscriptionId)
        {
            CustomerPrincipal principal;
            DateTime startTime;
            Dictionary<string, double> eventMetrics;
            Dictionary<string, string> eventProperties;
            Guid correlationId;
            IPartner operations;
            Subscription subscription;

            customerId.AssertNotEmpty(nameof(customerId));
            subscriptionId.AssertNotEmpty(nameof(subscriptionId));

            try
            {
                startTime = DateTime.Now;
                correlationId = Guid.NewGuid();
                operations = await GetAppOperationsAsync(correlationId);

                principal = new CustomerPrincipal(ClaimsPrincipal.Current);

                if (principal.CustomerId.Equals(service.Configuration.PartnerCenterApplicationTenantId))
                {
                    subscription = await operations.Customers.ById(customerId).Subscriptions.ById(subscriptionId).GetAsync();
                }
                else
                {
                    subscription = await operations.Customers.ById(principal.CustomerId).Subscriptions.ById(subscriptionId).GetAsync();
                }

                // Track the event measurements for analysis.
                eventMetrics = new Dictionary<string, double>
                {
                    { "ElapsedMilliseconds", DateTime.Now.Subtract(startTime).TotalMilliseconds }
                };

                // Capture the request for the customer summary for analysis.
                eventProperties = new Dictionary<string, string>
                {
                    { "CustomerId", principal.CustomerId },
                    { "Name", principal.Name },
                    { "ParternCenterCorrelationId", correlationId.ToString() }
                };

                service.Telemetry.TrackEvent("GetSubscriptionAsync", eventProperties, eventMetrics);

                return subscription;
            }
            finally
            {
                eventMetrics = null;
                eventProperties = null;
                operations = null;
                principal = null;
            }
        }

        /// <summary>
        /// Gets the subscriptions for the specified customer.
        /// </summary>
        /// <param name="customerId">Identifier for the customer.</param>
        /// <returns>A list of subscriptions for the customer.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="customerId"/> is empty or null.
        /// </exception>
        public async Task<List<Subscription>> GetSubscriptionsAsync(string customerId)
        {
            CustomerPrincipal principal;
            DateTime startTime;
            Dictionary<string, double> eventMetrics;
            Dictionary<string, string> eventProperties;
            Guid correlationId;
            IPartner operations;
            ResourceCollection<Subscription> subscriptions;

            customerId.AssertNotEmpty(nameof(customerId));

            try
            {
                startTime = DateTime.Now;
                correlationId = Guid.NewGuid();
                operations = await GetAppOperationsAsync(correlationId);

                principal = new CustomerPrincipal(ClaimsPrincipal.Current);

                if (principal.CustomerId.Equals(service.Configuration.PartnerCenterApplicationTenantId))
                {
                    subscriptions = await operations.Customers.ById(customerId).Subscriptions.GetAsync();
                }
                else
                {
                    subscriptions = await operations.Customers.ById(principal.CustomerId).Subscriptions.GetAsync();
                }

                // Track the event measurements for analysis.
                eventMetrics = new Dictionary<string, double>
                {
                    { "ElapsedMilliseconds", DateTime.Now.Subtract(startTime).TotalMilliseconds },
                    { "NumberOfSubscriptions", subscriptions.TotalCount }
                };

                // Capture the request for the customer summary for analysis.
                eventProperties = new Dictionary<string, string>
                {
                    { "CustomerId", principal.CustomerId },
                    { "Name", principal.Name },
                    { "ParternCenterCorrelationId", correlationId.ToString() }
                };

                service.Telemetry.TrackEvent("GetSubscriptionsAsync", eventProperties, eventMetrics);

                return new List<Subscription>(subscriptions.Items);
            }
            finally
            {
                eventMetrics = null;
                eventProperties = null;
                operations = null;
                principal = null;
            }
        }

        /// <summary>
        /// Gets utilization records for the Azure specified subscription.
        /// </summary>
        /// <param name="customerId">Identifier for the customer.</param>
        /// <param name="subscriptionId">Identifier for the subscription.</param>
        /// <param name="startTime">The starting time of when the utilization was metered in the billing system.</param>
        /// <param name="endTime">The ending time of when the utilization was metered in the billing system.</param>
        /// <returns>A list of <see cref="UsageModel"/>s that represent the utilization for the specified subscription.</returns>
        public async Task<List<UsageModel>> GetUsageAsync(string customerId, string subscriptionId, DateTimeOffset startTime, DateTimeOffset endTime)
        {
            AzureMeter meter;
            AzureRateCard rateCard;
            CustomerPrincipal principal;
            DateTime invokeTime;
            Dictionary<string, double> eventMetrics;
            Dictionary<string, string> eventProperties;
            Guid correlationId;
            IPartner operations;
            IResourceCollectionEnumerator<ResourceCollection<AzureUtilizationRecord>> usageEnumerator;
            List<AzureUtilizationRecord> usageRecords;
            List<UsageModel> models;
            ResourceCollection<AzureUtilizationRecord> records;
            decimal overage = 0, rate = 0;

            customerId.AssertNotEmpty(nameof(customerId));
            subscriptionId.AssertNotEmpty(nameof(subscriptionId));

            try
            {
                invokeTime = DateTime.Now;
                correlationId = Guid.NewGuid();
                operations = await GetAppOperationsAsync(correlationId);

                principal = new CustomerPrincipal(ClaimsPrincipal.Current);

                rateCard = await service.Cache.FetchAsync<AzureRateCard>(CacheDatabaseType.DataStructures, RateCardKey);

                models = new List<UsageModel>();
                usageRecords = new List<AzureUtilizationRecord>();

                if (rateCard == null)
                {
                    rateCard = await operations.RateCards.Azure.GetAsync();
                    await service.Cache.StoreAsync(CacheDatabaseType.DataStructures, RateCardKey, rateCard, TimeSpan.FromDays(1));
                }

                if (principal.CustomerId.Equals(service.Configuration.PartnerCenterApplicationTenantId))
                {
                    records = await operations.Customers.ById(customerId).Subscriptions.ById(subscriptionId)
                        .Utilization.Azure.QueryAsync(startTime, endTime);

                    usageEnumerator = operations.Enumerators.Utilization.Azure.Create(records);

                    while (usageEnumerator.HasValue)
                    {
                        usageRecords.AddRange(usageEnumerator.Current.Items);
                        await usageEnumerator.NextAsync();
                    }
                }
                else
                {
                    records = await operations.Customers.ById(principal.CustomerId).Subscriptions.ById(subscriptionId)
                        .Utilization.Azure.QueryAsync(startTime, endTime);

                    usageEnumerator = operations.Enumerators.Utilization.Azure.Create(records);

                    while (usageEnumerator.HasValue)
                    {
                        usageRecords.AddRange(usageEnumerator.Current.Items);
                        await usageEnumerator.NextAsync();
                    }
                }

                foreach (AzureUtilizationRecord record in usageRecords)
                {
                    try
                    {
                        // Obtain a reference to the meter associated with the usage record.
                        meter = rateCard.Meters.Single(x => x.Id.Equals(record.Resource.Id));
                    }
                    catch (InvalidOperationException ex)
                    {
                        service.Telemetry.TrackException(ex);
                        continue;
                    }

                    // Calculate the billable quantity by substracting the included quantity value.
                    overage = record.Quantity - meter.IncludedQuantity;

                    if (overage > 0)
                    {
                        // Obtain the rate for the given quantity. Some resources have tiered pricing
                        // so this assignment statement will select the appropriate rate based upon the
                        // quantity consumed, excluding the included quantity. 
                        rate = meter.Rates.Where(x => x.Key <= overage).Select(x => x.Value).Last();
                    }
                    else
                    {
                        overage = 0;
                    }

                    models.Add(new UsageModel
                    {
                        Price = (overage * rate),
                        UtilizationRecord = record
                    });
                }

                // Track the event measurements for analysis.
                eventMetrics = new Dictionary<string, double>
                {
                    { "ElapsedMilliseconds", DateTime.Now.Subtract(invokeTime).TotalMilliseconds },
                    { "NumberOfUsageRecords", usageRecords.Count }
                };

                // Capture the request for the customer summary for analysis.
                eventProperties = new Dictionary<string, string>
                {
                    { "CustomerId", principal.CustomerId },
                    { "Name", principal.Name },
                    { "ParternCenterCorrelationId", correlationId.ToString() }
                };

                service.Telemetry.TrackEvent("GetUsageAsync", eventProperties, eventMetrics);

                return models;
            }
            finally
            {
                eventMetrics = null;
                eventProperties = null;
                meter = null;
                operations = null;
                principal = null;
                rateCard = null;
                usageEnumerator = null;
                usageRecords = null;
            }
        }

        /// <summary>
        /// Gets an instance of the partner service that utilizes app only authentication.
        /// </summary>
        /// <param name="correlationId">Correlation identifier for the operation.</param>
        /// <returns>An instance of the partner service.</returns>
        private async Task<IPartner> GetAppOperationsAsync(Guid correlationId)
        {
            if (appOperations == null || appOperations.Credentials.ExpiresAt > DateTime.UtcNow)
            {
                IPartnerCredentials credentials = await service.Token
                    .GetPartnerCenterAppOnlyCredentialsAsync(
                        $"{service.Configuration.ActiveDirectoryEndpoint}/{service.Configuration.PartnerCenterApplicationTenantId}");

                lock (appLock)
                {
                    appOperations = PartnerService.Instance.CreatePartnerOperations(credentials);
                }

                PartnerService.Instance.ApplicationName = Constants.ApplicationName;
            }

            return appOperations.With(RequestContextFactory.Instance.Create(correlationId));
        }
    }
}