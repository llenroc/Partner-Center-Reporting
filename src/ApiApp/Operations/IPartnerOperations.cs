// -----------------------------------------------------------------------
// <copyright file="IPartnerOperations.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Reporting.Api.Operations
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models;
    using Store.PartnerCenter.Models.Customers;
    using Store.PartnerCenter.Models.Subscriptions;

    /// <summary>
    /// Represents the ability to perform various partner operations.
    /// </summary>
    public interface IPartnerOperations
    {
        /// <summary>
        /// Checks if the specified domain is available.
        /// </summary>
        /// <param name="domain">Domain to be checked.</param>
        /// <returns><c>true</c> if the domain is available; otherwise <c>false</c>.</returns>
        Task<bool> CheckDomainAsync(string domain);

        /// <summary>
        /// Gets the specified customer.
        /// </summary>
        /// <param name="customerId">Identifier for the customer.</param>
        /// <returns>An instance of <see cref="Customer"/> that represents the specified customer.</returns>
        Task<Customer> GetCustomerAsync(string customerId);

        /// <summary>
        /// Gets the available customers.
        /// </summary>
        /// <returns>A list of available customers.</returns>
        Task<List<Customer>> GetCustomersAsync();

        /// <summary>
        /// Gets the specified subscription.
        /// </summary>
        /// <param name="customerId">Identifier for the customer.</param>
        /// <param name="subscriptionId">Identifier for the subscription.</param>
        /// <returns>An instance of <see cref="Subscription"/> that represents the subscription.</returns>
        Task<Subscription> GetSubscriptionAsync(string customerId, string subscriptionId);

        /// <summary>
        /// Gets the subscriptions for the specified customer.
        /// </summary>
        /// <param name="customerId">Identifier for the customer.</param>
        /// <returns>A list of <see cref="Subscription"/>s for the customer.</returns>
        Task<List<Subscription>> GetSubscriptionsAsync(string customerId);

        /// <summary>
        /// Gets utilization records for the Azure specified subscription.
        /// </summary>
        /// <param name="customerId">Identifier for the customer.</param>
        /// <param name="subscriptionId">Identifier for the subscription.</param>
        /// <param name="startTime">The starting time of when the utilization was metered in the billing system.</param>
        /// <param name="endTime">The ending time of when the utilization was metered in the billing system.</param>
        /// <returns>A list of <see cref="UsageModel"/>s that represent the utilization for the specified subscription.</returns>
        Task<List<UsageModel>> GetUsageAsync(string customerId, string subscriptionId, DateTimeOffset startTime, DateTimeOffset endTime);
    }
}