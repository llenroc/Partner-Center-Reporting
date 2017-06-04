// -----------------------------------------------------------------------
// <copyright file="IGraphService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Reporting.Api.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models;

    /// <summary>
    /// Represents an object that interacts with Microsoft Graph.
    /// </summary>
    public interface IGraphService
    {
        /// <summary>
        /// Gets a list of roles that the specified user is associated with.
        /// </summary>
        /// <param name="objectId">Object identifier for the object to be checked.</param>
        /// <returns>A list of directory roles that the specified object identifier is associated with.</returns>
        Task<List<RoleModel>> GetRolesAsync(string objectId);
    }
}