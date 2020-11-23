// <copyright file="Country.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    public partial class Country : GeographicBoundary, CityBound
    {
        #region inherited properties

        public string Abbreviation { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public Guid UniqueId { get; set; }

        public City[] Cities { get; set; }

        #endregion

        #region Allors
        [Id("13010743-231f-43a8-9539-b95b83ab15da")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        public VatRate[] VatRates { get; set; }

        #region Allors
        [Id("2ecb8cfb-011d-4c31-a9cd-ed5a13ae23a4")]
        #endregion
        [Workspace(Default)]
        public int IbanLength { get; set; }

        #region Allors
        [Id("6553ee71-66dd-45f2-9de9-5656b011d2fc")]
        #endregion
        [Workspace(Default)]
        public bool EuMemberState { get; set; }

        #region Allors
        [Id("7f0adb03-db73-44f2-a4a2-ece00f4908a2")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string TelephoneCode { get; set; }

        #region Allors
        [Id("a2aa65d7-e0ef-4f6f-a194-9aeb49a1d898")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string IbanRegex { get; set; }

        #region Allors
        [Id("b829da1c-2eb7-495b-a4a9-98e335cd87f9")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        public VatForm VatForm { get; set; }

        #region Allors
        [Id("c231ce68-bf03-4122-8699-c3c6473ab90a")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string UriExtension { get; set; }

        #region inherited methods

        #endregion
    }
}
