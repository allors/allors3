// <copyright file="PartNumber.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("73F2577B-D217-47B1-B5DC-19D18C102554")]
    #endregion
    public partial class PartNumber : ProductIdentification
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public string Identification { get; set; }

        public ProductIdentificationType ProductIdentificationType { get; set; }

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
