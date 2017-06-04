// -----------------------------------------------------------------------
// <copyright file="AuthenticationTokenProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Reporting.Api.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models;
    using Owin.Security.Infrastructure;
    using Practices.Unity;
    using Services;

    /// <summary>
    /// Authentication token provider used to create and validate OAuth access tokens.
    /// </summary>
    public class AuthenticationTokenProvider : IAuthenticationTokenProvider
    {
        /// <summary>
        /// Creates an authentication token.  
        /// </summary>
        /// <param name="context">Context used for token creation.</param>
        public void Create(AuthenticationTokenCreateContext context)
        {
        }

        /// <summary>
        /// Creates an authentication token.
        /// </summary>
        /// <param name="context">Context used for token creation.</param>
        /// <returns>An instance of <see cref="Task"/> that represents the asynchronous operation.</returns>
        public async Task CreateAsync(AuthenticationTokenCreateContext context)
        {
            await Task.FromResult(0);
        }

        /// <summary>
        /// Receives the bearer token the client application will be providing to web application.
        /// </summary>
        /// <param name="context">Context for the authentication token received.</param>
        public void Receive(AuthenticationTokenReceiveContext context)
        {
        }

        /// <summary>
        /// Receives the bearer token the client application will be providing to web application.
        /// </summary>
        /// <param name="context">Context for the authentication token received.</param>
        /// <returns>An instance of <see cref="Task"/> that represents asynchronous operation.</returns>
        public async Task ReceiveAsync(AuthenticationTokenReceiveContext context)
        {
            IAppService service;
            IGraphService graph;
            List<RoleModel> roles;
            string userTenantId;
            string signedInUserObjectId;

            try
            {
                service = WebApiApplication.UnityContainer.Resolve<IAppService>();

                try
                {
                    context.DeserializeTicket(context.Token);
                }
                catch (Exception ex)
                {
                    service.Telemetry.TrackException(ex);
                }

                userTenantId = context.Ticket.Identity.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;
                signedInUserObjectId = context.Ticket.Identity.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;

                graph = new GraphService(service, userTenantId);

                roles = await graph.GetRolesAsync(signedInUserObjectId);

                foreach (RoleModel role in roles)
                {
                    context.Ticket.Identity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, role.DisplayName));
                }

                context.Ticket.Identity.AddClaim(new System.Security.Claims.Claim("CustomerId", userTenantId));
            }
            finally
            {
                graph = null;
                roles = null;
                service = null;
            }
        }
    }
}