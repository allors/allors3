// <copyright file="ValidationC2.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("c7563dd3-77b2-43ff-92f9-a4f98db36acf")]
    #endregion
    public partial class ValidationC2 : Object, ValidationI12
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
