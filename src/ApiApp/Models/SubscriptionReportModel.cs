// -----------------------------------------------------------------------
// <copyright file="SubscriptionReportModel.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Reporting.Api.Models
{
    using System.Collections.Generic;
    using PartnerCenter.Models.Invoices;
    using PartnerCenter.Models.Offers;
    using PartnerCenter.Models.Subscriptions;

    /// <summary>
    /// Represents a subscription model used for reporting purposes.
    /// </summary>
    public class SubscriptionReportModel
    {
        /// <summary>
        /// Gets the way billing is processed for the subscription.
        /// </summary>
        public BillingCycleType BillingCycle { get; set; }

        /// <summary>
        /// Gets or sets the way billing is processed for the subscription.
        /// </summary>
        public BillingType BillingType { get; set; }

        /// <summary>
        /// Gets or sets the friendly name for the subscription.
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        /// Gets or sets the offer name that was used to generate the subscription.
        /// </summary>
        public string OfferName { get; set; }

        /// <summary>
        /// Gets or sets the quantity. If the subscription is licensed based then this property is set to seat count.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the status for the subscription.
        /// </summary>
        public SubscriptionStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the identifier for the subscription.
        /// </summary>
        public string SubscriptionId { get; set; }

        /// <summary>
        ///  Gets or sets the units defining <see cref="Quantity"/> for the subscription.
        /// </summary>
        public string UnitType { get; set; }

        /// <summary>
        /// Gets or sets a list <see cref="UsageModel"/>s that represent the utilization for the subscription.
        /// </summary>
        /// <remarks>
        /// This property will only be populated for usage based subscriptions.
        /// </remarks>
        public List<UsageModel> Usage { get; set; }
    }
}