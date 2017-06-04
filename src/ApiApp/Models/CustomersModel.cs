// -----------------------------------------------------------------------
// <copyright file="CustomersModel.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Reporting.Api.Models
{
    using System.Collections.Generic;
    using PartnerCenter.Models.Customers;

    /// <summary>
    /// Provides a model for listing customer(s).
    /// </summary>
    public class CustomersModel
    {
        /// <summary>
        /// Gets or sets the collection of customers that belong to the partner.
        /// </summary>
        public List<Customer> Customers { get; set; }
    }
}