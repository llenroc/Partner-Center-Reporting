// -----------------------------------------------------------------------
// <copyright file="AuthorizationFilterAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Reporting.Api.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using Security;

    /// <summary>
    /// Authorization filter attribute used to verify authenticated user has the specified claim and value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class AuthorizationFilterAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// Gets or sets the required roles.
        /// </summary>
        public new UserRoles Roles { get; set; }

        /// <summary>
        /// Indicates whether the authenticated user is authorized.
        /// </summary>
        /// <param name="actionContext">The action context.</param>
        /// <returns><c>true</c> if the user is authorized; otherwise <c>false</c>.</returns>
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            CustomerPrincipal principal;

            try
            {
                principal = new CustomerPrincipal(actionContext.RequestContext.Principal as ClaimsPrincipal); 

                foreach (string role in GetRoles(Roles))
                {
                    if (principal.HasClaim(ClaimTypes.Role, role))
                    {
                        return true;
                    }
                }

                return false;
            }
            finally
            {
                principal = null;
            }
        }

        /// <summary>
        /// Gets a list of roles that required to perform the operation.
        /// </summary>
        /// <param name="requiredRole">User role required to perform the operation.</param>
        /// <returns>A list of roles that required to perform the operation.</returns>
        private List<string> GetRoles(UserRoles requiredRole)
        {
            List<string> required = new List<string>();

            if (requiredRole.HasFlag(UserRoles.AdminAgents))
            {
                required.Add(UserRoles.AdminAgents.GetDescription());
            }

            if (requiredRole.HasFlag(UserRoles.BillingAdmin))
            {
                required.Add(UserRoles.BillingAdmin.GetDescription());
            }

            if (requiredRole.HasFlag(UserRoles.GlobalAdmin))
            {
                required.Add(UserRoles.GlobalAdmin.GetDescription());
            }

            if (requiredRole.HasFlag(UserRoles.HelpdeskAgent))
            {
                required.Add(UserRoles.HelpdeskAgent.GetDescription());
            }

            if (requiredRole.HasFlag(UserRoles.SalesAgent))
            {
                required.Add(UserRoles.SalesAgent.GetDescription());
            }

            if (requiredRole.HasFlag(UserRoles.User))
            {
                required.Add(UserRoles.User.GetDescription());
            }

            if (requiredRole.HasFlag(UserRoles.UserAdministrator))
            {
                required.Add(UserRoles.UserAdministrator.GetDescription());
            }

            return required;
        }
    }
}