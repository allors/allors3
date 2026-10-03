// <copyright file="User.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Attributes;
    using static Workspaces;

    // The Entra identity of a user, and what the directory says about the person. Core's User has
    // no authentication field; the Entra domain declares its own.
    //
    // The identity is the pair of the tenant id and the object id, the tid and oid claims of a
    // token. The user name, the display name and the e-mail are the preferred_username, name and
    // email claims. They are for showing, never for finding a user: the directory can change them,
    // and it does not verify the e-mail. A guest of the tenant, invited from another organization,
    // keeps its home in the identity provider, the idp claim, and is marked as a guest by the acct
    // claim; a member of the tenant has the tenant's own issuer as its identity provider.
    //
    // All seven are derived: the plug-in takes them from a validated token, and nobody writes them
    // through the API.
    public partial interface User
    {
        #region Allors
        [Id("7aa7ffb4-41cc-42a2-a0ef-0a7a008356cc")]
        #endregion
        [Derived]
        Guid EntraTenantId { get; set; }

        #region Allors
        [Id("ca46db31-fc03-4d54-bebb-32422dcc5aa7")]
        #endregion
        [Indexed]
        [Derived]
        Guid EntraObjectId { get; set; }

        #region Allors
        [Id("c72d5dee-49b4-4c27-90cc-7507bcf4973a")]
        #endregion
        [Size(256)]
        [Derived]
        [Workspace(Default)]
        string EntraUserName { get; set; }

        #region Allors
        [Id("87c7fc93-9f05-4a6e-9f9a-7836b95a6037")]
        #endregion
        [Size(256)]
        [Derived]
        [Workspace(Default)]
        string EntraDisplayName { get; set; }

        #region Allors
        [Id("400604b9-5ca8-493f-829c-357b7f63bd47")]
        #endregion
        [Size(256)]
        [Derived]
        [Workspace(Default)]
        string EntraEmail { get; set; }

        #region Allors
        [Id("5ad2f5cc-2f4b-480d-b05d-60f7e6782b6b")]
        #endregion
        [Size(256)]
        [Derived]
        [Workspace(Default)]
        string EntraIdentityProvider { get; set; }

        #region Allors
        [Id("f07e6ea2-87ca-4b6d-8d35-131e4d8aa451")]
        #endregion
        [Derived]
        [Workspace(Default)]
        bool EntraIsGuest { get; set; }
    }
}
