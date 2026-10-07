// <copyright file="User.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    // What the plug-in adds to a user of its host: a key and its normalized form, which a rule of
    // Plugin1 derives, as an authentication plug-in adds its fields to Core's User.
    public partial interface User
    {
        #region Allors
        [Id("d72c1be3-7afd-4fe1-b003-d9dbff2faa3d")]
        #endregion
        [Size(256)]
        string Plugin1Key { get; set; }

        #region Allors
        [Id("fa86cbe5-a7cc-4fd6-a99e-c78e11ef0ca0")]
        #endregion
        [Size(256)]
        [Derived]
        string Plugin1NormalizedKey { get; set; }
    }
}
