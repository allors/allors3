// <copyright file="ValidationC1.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("2361c456-b624-493a-8377-2dd1e697e17a")]
    #endregion
    public partial class ValidationC1 : Object, ValidationI12
    {
        #region inherited properties
        public Guid UniqueId { get; set; }

        #endregion

        #region inherited methods

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        #endregion
    }
}
