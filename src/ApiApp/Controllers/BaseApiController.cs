// -----------------------------------------------------------------------
// <copyright file="BaseApiController.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Reporting.Api.Controllers
{
    using System.Web.Http;
    using Services;

    /// <summary>
    /// Base class for all Api controllers.
    /// </summary>
    public abstract class BaseApiController : ApiController
    {
        /// <summary>
        /// Provides access to the core application providers.
        /// </summary>
        private readonly IAppService service;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseApiController"/> class.
        /// </summary>
        /// <param name="service">Provides access to the core application providers.</param>
        protected BaseApiController(IAppService service)
        {
            service.AssertNotNull(nameof(service));
            this.service = service;
        }

        /// <summary>
        /// Provides access to the core services.
        /// </summary>
        protected IAppService Service => service;
    }
}