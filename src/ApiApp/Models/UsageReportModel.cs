// -----------------------------------------------------------------------
// <copyright file="UsageReportModel.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Reporting.Api.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a model for the usage report.
    /// </summary>
    public class UsageReportModel
    {
        /// <summary>
        /// Gets or sets the customer identifier that owns the subscriptions.
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// Gets or sets a list of subscriptions included in the usage report.
        /// </summary>
        public List<SubscriptionReportModel> Subscriptions { get; set; }
    }
}