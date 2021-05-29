// <copyright file="UnitOfMeasureConversion.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("2e216901-eab9-42e3-9e49-7fe8e88291d3")]
    #endregion
    public partial class UnitOfMeasureConversion : Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("3ae94702-ee60-4057-a649-f655ff4e2865")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public IUnitOfMeasure ToUnitOfMeasure { get; set; }

        #region Allors
        [Id("5d7ed801-4a2e-4abc-a32d-d869210132af")]
        #endregion

        public DateTime StartDate { get; set; }

        #region Allors
        [Id("835118da-148a-4c42-ab07-75b213a8e1f7")]
        #endregion
        [Required]
        [Precision(19)]
        [Scale(9)]
        public decimal ConversionFactor { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        #endregion

    }
}
