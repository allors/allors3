// <copyright file="Person.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    // The user class of the concrete domain. It implements what Core gives a User and what
    // Plugin1 adds to it; it has the class id of the Person of the other trees.
    #region Allors
    [Id("6E026CC2-1979-413A-A4B2-54B41667E013")]
    #endregion
    public partial class Person : User
    {
        #region inherited properties

        public Guid UniqueId { get; set; }

        public SecurityToken OwnerSecurityToken { get; set; }

        public Grant OwnerGrant { get; set; }

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public string Plugin1Key { get; set; }

        public string Plugin1NormalizedKey { get; set; }

        #endregion

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit() { }

        public void OnPostDerive() { }

        public void Delete() { }

        #endregion
    }
}
