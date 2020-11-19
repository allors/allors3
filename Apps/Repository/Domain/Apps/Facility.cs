// <copyright file="Facility.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("cdd79e23-a132-48b0-b88f-a03bd029f49d")]
    #endregion
    [Plural("Facilities")]
    public partial class Facility : GeoLocatable, Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid UniqueId { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        #endregion

        #region Allors
        [Id("0E81E9A0-8E7C-4FC9-930A-10D3E67EA17A")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace(Default)]
        public FacilityType FacilityType { get; set; }

        #region Allors
        [Id("1a7f255a-3e94-41df-b71d-10ab36f38ffb")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Facility ParentFacility { get; set; }

        #region Allors
        [Id("1daad895-cf57-4110-a4e0-117e0212c3e4")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal SquareFootage { get; set; }

        #region Allors
        [Id("2df0999d-97cb-4c76-9f3e-076376e60e38")]
        #endregion
        [Workspace(Default)]
        [Size(-1)]
        public string Description { get; set; }

        #region Allors
        [Id("4b55ee38-64e9-4c11-a204-36e2f460c5f8")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        public ContactMechanism[] FacilityContactMechanisms { get; set; }

        #region Allors
        [Id("b8f50794-848b-42be-9114-5eea579f5f71")]
        #endregion
        [Required]
        [Size(256)]
        [Workspace(Default)]
        public string Name { get; set; }

        #region Allors
        [Id("c73693db-9eae-4d81-a801-2ef4d619544b")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public InternalOrganisation Owner { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        #endregion
    }
}
