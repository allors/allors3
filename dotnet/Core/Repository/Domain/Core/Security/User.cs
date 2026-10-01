// <copyright file="User.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using Attributes;

    // Authorization needs the object id of a user only. The authentication fields, such as the user
    // name and the password hash, belong to an authentication plug-in: the Identity domain declares
    // them on this interface in its own tree.
    #region Allors
    [Id("a0309c3b-6f80-4777-983e-6e69800df5be")]
    #endregion
    public partial interface User : UniquelyIdentifiable, SecurityTokenOwner, Deletable
    {
    }
}
