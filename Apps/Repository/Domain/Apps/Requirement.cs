// <copyright file="Requirement.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("d3f90525-b7fe-4f81-bccd-adf4f57260bc")]
    #endregion
    public partial class Requirement : Transitional, UniquelyIdentifiable, Versioned
    {
        #region inherited properties

        public Permission[] DeniedPermissions { get; set; }

        public Permission[] TransitionalDeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public ObjectState[] PreviousObjectStates { get; set; }

        public ObjectState[] LastObjectStates { get; set; }

        public ObjectState[] ObjectStates { get; set; }

        public Guid UniqueId { get; set; }

        public User CreatedBy { get; set; }

        public User LastModifiedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastModifiedDate { get; set; }
        #endregion

        #region ObjectStates
        #region RequirementState
        #region Allors
        [Id("048F8002-E484-4AB7-880A-DB57B9F3634A")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public RequirementState PreviousRequirementState { get; set; }

        #region Allors
        [Id("DCCF39B6-E085-4778-B732-F45A51BA4CA2")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public RequirementState LastRequirementState { get; set; }

        #region Allors
        [Id("B0009B12-8439-4F1A-81F6-7126E5B10D47")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public RequirementState RequirementState { get; set; }
        #endregion
        #endregion

        #region Versioning
        #region Allors
        [Id("3651CD57-2472-44F1-8140-4260434C1A1C")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public RequirementVersion CurrentVersion { get; set; }

        #region Allors
        [Id("A473CA42-DDB5-4021-B629-8D407A59EB1A")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public RequirementVersion[] AllVersions { get; set; }
        #endregion

        #region Allors
        [Id("0f2c9ca2-9f2a-403e-8110-311fc0622326")]
        #endregion
        [Workspace(Default)]
        public DateTime RequiredByDate { get; set; }

        #region Allors
        [Id("2202F95A-9D57-4792-BD8F-E35DECDD515E")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public RequirementType RequirementType { get; set; }

        #region Allors
        [Id("2b828f2b-201d-4ae2-b64c-b2c5be713653")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Party Authorizer { get; set; }

        #region Allors
        [Id("3a6ba1d0-3efb-44f3-b90b-7e504ed11140")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        public string Reason { get; set; }

        #region Allors
        [Id("3ecf2b1e-ac3d-4533-9da1-341111fca04d")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        public Requirement[] Children { get; set; }

        #region Allors
        [Id("43e11ee6-dcee-4a2c-80a7-8e04ee36ceb8")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Party NeededFor { get; set; }

        #region Allors
        [Id("5ed2803c-02d4-4187-8155-bee79e1a0829")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Party Originator { get; set; }

        #region Allors
        [Id("b6b7e1e9-6cce-4ca0-a085-0afd3a58ec50")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Facility Facility { get; set; }

        #region Allors
        [Id("bfce13c0-b5c2-46f0-b0fd-d0d288f8dc07")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Party ServicedBy { get; set; }

        #region Allors
        [Id("c34694b4-bd8e-46e9-8bf1-fb1296738ab4")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal EstimatedBudget { get; set; }

        #region Allors
        [Id("d902fe48-c91f-43fe-b402-e0d87606124a")]
        #endregion
        [Required]
        [Size(-1)]
        [Workspace(Default)]
        public string Description { get; set; }

        #region Allors
        [Id("f553ad3c-675f-4b97-95c9-42f4d85eb5f9")]
        #endregion
        [Workspace(Default)]
        public int Quantity { get; set; }

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

        #region Allors

        [Id("8B09FA26-51AC-4286-8304-439E54A1CB2A")]

        #endregion
        public void Reopen()
        {
        }

        #region Allors

        [Id("F96CD431-5143-463E-9C6E-1703AFC2F5E1")]

        #endregion
        public void Cancel()
        {
        }

        #region Allors
        [Id("5C5C6AA9-C8C8-428E-976F-76BC355A1602")]
        #endregion
        public void Hold()
        {
        }

        #region Allors
        [Id("00FBB6C0-BDE5-4913-AF34-2F80AA759B3A")]
        #endregion
        public void Close()
        {
        }
    }
}
