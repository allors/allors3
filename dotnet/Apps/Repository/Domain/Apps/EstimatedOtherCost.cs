// <copyright file="EstimatedOtherCost.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("9b637b39-f61a-4985-bb1b-876ed769f448")]
    #endregion
    public partial class EstimatedOtherCost : EstimatedProductCost
    {
        #region inherited properties
        public decimal Cost { get; set; }

        public Currency Currency { get; set; }

        public Organisation Organisation { get; set; }

        public string Description { get; set; }

        public GeographicBoundary GeographicBoundary { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ThroughDate { get; set; }

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
