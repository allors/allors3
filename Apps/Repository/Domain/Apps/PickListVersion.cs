// <copyright file="PickListVersion.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("15B7482D-17F1-4184-9C57-222D41215553")]
    #endregion
    public partial class PickListVersion : Version
    {
        #region inherited properties

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid DerivationId { get; set; }

        public DateTime DerivationTimeStamp { get; set; }

        public User LastModifiedBy { get; set; }

        #endregion

        #region Allors
        [Id("4231c38e-e54c-480d-9e0f-2fe8bd101da1")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public PickListState PickListState { get; set; }

        #region Allors
        [Id("C793C53B-23DB-44E1-8BAB-B62E3C65FD5F")]
        #endregion
        [Required]
        [Workspace(Default)]
        public DateTime CreationDate { get; set; }

        #region Allors
        [Id("18C30DFC-988C-49E3-BC47-1EF70E54E004")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public PickListItem[] PickListItems { get; set; }

        #region Allors
        [Id("30DE831B-2D14-4E6C-9629-EEA958FDA6DD")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Person Picker { get; set; }

        #region Allors
        [Id("854D73F1-4D46-4B6D-9084-7D9A5497CA1A")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Party ShipToParty { get; set; }

        #region Allors
        [Id("A0EC7FD3-298F-4453-A611-549125C2B646")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Store Store { get; set; }

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
