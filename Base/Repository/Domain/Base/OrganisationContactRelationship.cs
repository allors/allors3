// <copyright file="OrganisationContactRelationship.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;

    #region Allors
    [Id("956ecb86-097d-43d4-83b5-a7f45ea75448")]
    #endregion
    public partial class OrganisationContactRelationship : PartyRelationship
    {
        #region inherited properties

        public DateTime FromDate { get; set; }

        public DateTime ThroughDate { get; set; }

        public Agreement[] Agreements { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Party[] Parties { get; set; }

        #endregion

        #region Allors
        [Id("0ca367d2-0ce2-440d-a4e7-cbf089c1efed")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace]
        public Person Contact { get; set; }

        #region Allors
        [Id("96f4c9af-eeff-477f-8a93-1168cc383b4c")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace]
        public Organisation Organisation { get; set; }

        #region Allors
        [Id("af7e007e-c325-453a-923e-55299eda2a8c")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace]
        public OrganisationContactKind[] ContactKinds { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        public void Delete() { }
        #endregion
    }
}
