// <copyright file="Equipment.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("da852ff9-0c87-4fa6-a93a-90d97d28029c")]
    #endregion
    [Plural("Equipments")]
    public partial class Equipment : FixedAsset
    {
        #region inherited properties

        public string Comment { get; set; }

        public LocalisedText[] LocalisedComments { get; set; }

        public DateTime LastServiceDate { get; set; }

        public DateTime AcquiredDate { get; set; }

        public string Description { get; set; }

        public LocalisedText[] LocalisedDescriptions { get; set; }

        public decimal ProductionCapacity { get; set; }

        public DateTime NextServiceDate { get; set; }

        public string Keywords { get; set; }

        public LocalisedText[] LocalisedKeywords { get; set; }

        public string SearchString { get; set; }

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Media[] PublicElectronicDocuments { get; set; }

        public LocalisedMedia[] PublicLocalisedElectronicDocuments { get; set; }

        public Media[] PrivateElectronicDocuments { get; set; }

        public LocalisedMedia[] PrivateLocalisedElectronicDocuments { get; set; }

        public string DisplayName { get; set; }

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
