// <copyright file="MarketingMaterial.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("6d4739a9-c3c4-4570-a337-49f667c6243b")]
    #endregion
    public partial class MarketingMaterial : Document
    {
        #region inherited properties
        public string Name { get; set; }

        public string Description { get; set; }

        public string Text { get; set; }

        public string DocumentLocation { get; set; }

        public PrintDocument PrintDocument { get; set; }

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public string Comment { get; set; }

        public LocalisedText[] LocalisedComments { get; set; }

        #endregion

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        public void Print() { }

        #endregion
    }
}
