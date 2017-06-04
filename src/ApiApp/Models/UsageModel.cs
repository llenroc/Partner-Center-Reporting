// -----------------------------------------------------------------------
// <copyright file="UsageModel.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Reporting.Api.Models
{
    using Store.PartnerCenter.Models.Utilizations;

    /// <summary>
    /// Represent an Azure usage record merged with Rate Card data.
    /// </summary>
    public class UsageModel
    {
        /// <summary>
        /// Gets or sets the price for the usage consumed.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Gets or sets the utilization record data.
        /// </summary>
        public AzureUtilizationRecord UtilizationRecord { get; set; }
    }
}