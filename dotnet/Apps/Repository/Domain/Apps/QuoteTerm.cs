// <copyright file="QuoteTerm.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("cd60cf6d-65ba-4e31-b85d-16c19fc0978b")]
    #endregion
    public partial class QuoteTerm : SalesTerm, Object
    {
        #region inherited properties

        public string TermValue { get; set; }

        public TermType TermType { get; set; }

        public string Description { get; set; }

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        public void Delete() { }

        #endregion
    }
}
