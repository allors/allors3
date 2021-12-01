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
    public partial class WorkRequirement : Requirement, Versioned
    {
        #region inherited properties
        public RequirementState PreviousRequirementState { get; set; }

        public RequirementState LastRequirementState { get; set; }

        public RequirementState RequirementState { get; set; }

        public string RequirementNumber { get; set; }

        public int SortableRequirementNumber { get; set; }

        public DateTime RequiredByDate { get; set; }

        public RequirementType RequirementType { get; set; }

        public Party Authorizer { get; set; }

        public string Reason { get; set; }

        public Requirement[] Children { get; set; }

        public Party NeededFor { get; set; }

        public Party Originator { get; set; }

        public Facility Facility { get; set; }

        public Organisation ServicedBy { get; set; }

        public Priority Priority { get; set; }

        public decimal EstimatedBudget { get; set; }

        public string Description { get; set; }

        public int Quantity { get; set; }

        public Media[] Pictures { get; set; }

        public Revocation[] Revocations { get; set; }

        public Revocation[] TransitionalRevocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public ObjectState[] PreviousObjectStates { get; set; }

        public ObjectState[] LastObjectStates { get; set; }

        public ObjectState[] ObjectStates { get; set; }

        public string RequirementStateName { get; set; }

        public string RequirementTypeName { get; set; }

        public string PriorityName { get; set; }

        public Guid UniqueId { get; set; }

        public User CreatedBy { get; set; }

        public User LastModifiedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastModifiedDate { get; set; }
        #endregion

        #region Versioning
        #region Allors
        [Id("3651CD57-2472-44F1-8140-4260434C1A1C")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public WorkRequirementVersion CurrentVersion { get; set; }

        #region Allors
        [Id("A473CA42-DDB5-4021-B629-8D407A59EB1A")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public WorkRequirementVersion[] AllVersions { get; set; }
        #endregion

        #region Allors
        [Id("7fdad417-99be-4e64-850b-da0f7cd64534")]
        #endregion
        [Workspace(Default)]
        public string Location { get; set; }

        #region Allors
        [Id("d54f8b8f-8e91-4e84-972f-18641701d88d")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public FixedAsset FixedAsset { get; set; }

        #region Allors
        [Id("ecc706e6-9ad3-4d16-8989-a380c3936f6a")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace(Default)]
        public string FixedAssetName { get; set; }

        #region Allors
        [Id("51e8d0b1-630c-4471-bc90-c5bccbfb261b")]
        [Indexed]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool UnOperable{ get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        public void Reopen() { }

        public void Cancel() { }

        public void Delete() { }

        #endregion

        #region Allors
        [Id("59056183-a6c4-4ae0-81a6-bb6a236758e5")]
        #endregion
        [Workspace(Default)]
        public void CreateWorkTask() { }
    }
}
