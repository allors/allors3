// <copyright file="PickList.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("27b6630a-35d0-4352-9223-b5b6c8d7496b")]
    #endregion
    public partial class PickList : Deletable, Printable, Transitional, Versioned
    {
        #region inherited properties

        public ObjectState[] PreviousObjectStates { get; set; }

        public ObjectState[] LastObjectStates { get; set; }

        public ObjectState[] ObjectStates { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public Permission[] TransitionalDeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public PrintDocument PrintDocument { get; set; }

        public User CreatedBy { get; set; }

        public User LastModifiedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        #endregion

        #region ObjectStates
        #region PickListState
        #region Allors
        [Id("87B1275D-A60B-46B7-8510-CA42EBAAEF97")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public PickListState PreviousPickListState { get; set; }

        #region Allors
        [Id("86EEF807-1C0B-4053-82EF-D90CD379A6D8")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public PickListState LastPickListState { get; set; }

        #region Allors
        [Id("FDFC9DF1-D4A6-4F4F-BA5E-4D523DA7D00A")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public PickListState PickListState { get; set; }
        #endregion
        #endregion

        #region Versioning
        #region Allors
        [Id("7BF3DC9C-258D-4744-8EAC-8DBD3702C178")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public PickListVersion CurrentVersion { get; set; }

        #region Allors
        [Id("2C188CDB-CCE2-43D3-B1A3-D2F33716B02C")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public PickListVersion[] AllVersions { get; set; }
        #endregion

        #region Allors
        [Id("3bb68c85-4e2b-42b8-b5fb-18a66c58c283")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        public PickListItem[] PickListItems { get; set; }

        #region Allors
        [Id("6572f862-31b2-4be9-b7dc-7fff5d21f620")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Person Picker { get; set; }

        #region Allors
        [Id("ae75482e-2c41-46d4-ab73-f3aac368fd50")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Party ShipToParty { get; set; }

        #region Allors
        [Id("e334e938-35e7-4217-91fa-efb231f71a37")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Store Store { get; set; }

        #region Allors
        [Id("46CB3076-14AE-48C1-8C9F-F4EFB4B060EB")]
        #endregion
        [Workspace(Default)]
        public void Hold() { }

        #region Allors
        [Id("F3D35303-BA28-4CF0-B393-7E7D76F5B86D")]
        #endregion
        [Workspace(Default)]
        public void Continue() { }

        #region Allors
        [Id("8753A40E-FAA1-44E7-86B1-6CA6712989B5")]
        #endregion
        [Workspace(Default)]
        public void Cancel() { }

        #region Allors
        [Id("CA2ADD8E-C43E-4C95-A8A4-B279FEE4CB0A")]
        #endregion
        [Workspace(Default)]
        public void SetPicked() { }

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

        public void Print() { }

        #endregion
    }
}
