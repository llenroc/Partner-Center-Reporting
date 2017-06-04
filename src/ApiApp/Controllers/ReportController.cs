// -----------------------------------------------------------------------
// <copyright file="ReportController.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Reporting.Api.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Filters;
    using Models;
    using Operations;
    using PartnerCenter.Models.Customers;
    using PartnerCenter.Models.Invoices;
    using PartnerCenter.Models.Subscriptions;
    using Security;
    using Services;
    using Swashbuckle.Swagger.Annotations;

    /// <summary>
    /// Controller that handles reporting requests.
    /// </summary>
    [Authorize]
    [RoutePrefix("api/reporting")]
    public class ReportController : BaseApiController
    {
        private readonly IPartnerOperations operations;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportController"/> class.
        /// </summary>
        /// <param name="service">Provides access to core application providers.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="operations"/> is null.
        /// </exception>
        public ReportController(IAppService service, IPartnerOperations operations) : base(service)
        {
            operations.AssertNotNull(nameof(operations));
            this.operations = operations;
        }

        /// <summary>
        /// Gets a list of all customers.
        /// </summary>
        /// <returns>
        /// A list of customers utilized for rendering purpose.
        /// </returns>
        [AuthorizationFilter(Roles = UserRoles.Partner | UserRoles.GlobalAdmin)]
        [HttpGet]
        [Route("customers")]
        [SwaggerOperation("customers")]
        [SwaggerResponse(HttpStatusCode.OK)]
        public async Task<CustomersModel> GetCustomersAsync()
        {
            CustomerPrincipal principal;
            CustomersModel viewModel;
            DateTime startTime;
            Dictionary<string, double> eventMeasurements;
            Dictionary<string, string> eventProperties;
            List<Customer> customers;

            try
            {
                startTime = DateTime.Now;

                principal = new CustomerPrincipal(ClaimsPrincipal.Current);

                viewModel = new CustomersModel()
                {
                    Customers = new List<Customer>()
                };

                customers = await operations.GetCustomersAsync();
                viewModel.Customers.AddRange(customers);

                // Capture the request for the customer summary for analysis.
                eventProperties = new Dictionary<string, string>
                {
                    { "Email", principal.Email },
                    { "PrincipalCustomerId", principal.CustomerId }
                };

                // Track the event measurements for analysis.
                eventMeasurements = new Dictionary<string, double>
                {
                    { "CustomerCount", viewModel.Customers.Count },
                    { "ElapsedMilliseconds", DateTime.Now.Subtract(startTime).TotalMilliseconds }
                };

                Service.Telemetry.TrackEvent("/api/reporting/customers", eventProperties, eventMeasurements);

                return viewModel;
            }
            finally
            {
                customers = null;
                eventMeasurements = null;
                eventProperties = null;
                principal = null;
            }
        }

        /// <summary>
        /// Gets the usage report details.
        /// </summary>
        /// <returns>A list of <see cref="UsageReportModel"/>s that represents the details for the report.</returns>
        [AuthorizationFilter(Roles = UserRoles.Partner | UserRoles.GlobalAdmin)]
        [HttpGet]
        [Route("usage")]
        [SwaggerOperation("usage")]
        [SwaggerResponse(HttpStatusCode.OK)]
        public async Task<List<UsageReportModel>> GetUsageAsync()
        {
            CustomerPrincipal principal;
            CustomersModel customers;
            DateTime startTime;
            Dictionary<string, double> eventMeasurements;
            Dictionary<string, string> eventProperties;
            List<UsageReportModel> models;

            try
            {
                startTime = DateTime.Now;
                principal = new CustomerPrincipal(ClaimsPrincipal.Current);

                customers = await GetCustomersAsync();

                models = new List<UsageReportModel>();

                foreach (Customer c in customers.Customers)
                {
                    models.Add(new UsageReportModel
                    {
                        Subscriptions = await GetSubscriptionReportAsync(c.Id),
                        CustomerId = c.Id
                    });
                }

                // Capture the request for the customer summary for analysis.
                eventProperties = new Dictionary<string, string>
                {
                    { "PrincipalCustomerId", principal.CustomerId }
                };

                // Track the event measurements for analysis.
                eventMeasurements = new Dictionary<string, double>
                {
                    { "ElapsedMilliseconds", DateTime.Now.Subtract(startTime).TotalMilliseconds }
                };

                Service.Telemetry.TrackEvent("/api/reporting/usage", eventProperties, eventMeasurements);

                return models;
            }
            finally
            {
                eventMeasurements = null;
                eventProperties = null;
                principal = null;
            }
        }

        /// <summary>
        /// Gets the subscription report details.
        /// </summary>
        /// <param name="customerId">Identifier for the customer.</param>
        /// <returns>A list of <see cref="SubscriptionReportModel"/>s that represents the subscription details.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="customerId"/> is empty or null.
        /// </exception>
        private async Task<List<SubscriptionReportModel>> GetSubscriptionReportAsync(string customerId)
        {
            List<Subscription> subscriptions;
            List<UsageModel> usage;
            List<SubscriptionReportModel> models;

            customerId.AssertNotEmpty(nameof(customerId));

            try
            {
                subscriptions = await operations.GetSubscriptionsAsync(customerId);
                models = new List<SubscriptionReportModel>();

                foreach (Subscription s in subscriptions.Where(x => x.BillingType == BillingType.Usage && x.Status != SubscriptionStatus.Deleted))
                {
                    usage = await operations.GetUsageAsync(
                        customerId,
                        s.Id,
                        DateTimeOffset.Now.AddMonths(-3),
                        DateTimeOffset.Now);

                    models.Add(new SubscriptionReportModel
                    {
                        BillingCycle = s.BillingCycle,
                        BillingType = s.BillingType,
                        FriendlyName = s.FriendlyName,
                        OfferName = s.OfferName,
                        Quantity = s.Quantity,
                        Status = s.Status,
                        SubscriptionId = s.Id,
                        UnitType = s.UnitType,
                        Usage = usage
                    });
                }

                return models;
            }
            finally
            {
                subscriptions = null;
                usage = null;
            }
        }
    }
}