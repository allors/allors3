// <copyright file="EngineeringChange.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("c6c4537a-21f8-4d62-b584-3c609fb2210f")]
    #endregion
    public partial class EngineeringChange : Transitional
    {
        #region inherited properties

        public ObjectState[] PreviousObjectStates { get; set; }

        public ObjectState[] LastObjectStates { get; set; }

        public ObjectState[] ObjectStates { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public Permission[] TransitionalDeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region ObjectStates
        #region EngineeringChangeState
        #region Allors
        [Id("f3e5f6b4-ae65-4ee7-b9bd-735ceb630d92")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public EngineeringChangeState PreviousEngineeringChangeState { get; set; }

        #region Allors
        [Id("423bb0aa-c703-46c3-a9f1-ed40992feedb")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public EngineeringChangeState LastEngineeringChangeState { get; set; }

        #region Allors
        [Id("41225806-6444-45ac-87b9-d8e44d105f4c")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public EngineeringChangeState EngineeringChangeState { get; set; }
        #endregion
        #endregion

        #region Allors
        [Id("1a5edba2-6fda-4eb1-9e37-7a0e368ccff0")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public Person Requestor { get; set; }

        #region Allors
        [Id("1b65b18b-c930-49b4-85e4-bb4b07dfdca2")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public Person Authorizer { get; set; }

        #region Allors
        [Id("4487e364-4c5e-4b84-8847-a6ec1f1a0e6f")]
        #endregion
        [Required]
        [Size(-1)]

        public string Description { get; set; }

        #region Allors
        [Id("8d123834-364e-47d7-9d1e-63f4ef19f8c0")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public Person Designer { get; set; }

        #region Allors
        [Id("9caba64b-4959-43f9-a6a6-c76dff62dc02")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]

        public PartSpecification[] PartSpecifications { get; set; }

        #region Allors
        [Id("b076cdcc-7e3f-46c8-b127-98d29a4c9e4e")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]

        public PartBillOfMaterial[] PartBillOfMaterials { get; set; }

        #region Allors
        [Id("c360a1d9-5d8c-4295-aaae-2d50410dd293")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Indexed]

        public EngineeringChangeObjectState CurrentObjectState { get; set; }

        #region Allors
        [Id("caf244e2-f61d-436e-978c-1d0af118949f")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]

        public EngineeringChangeStatus[] EngineeringChangeStatuses { get; set; }

        #region Allors
        [Id("d18955d3-1fce-46c9-bb44-5830bfdc09fd")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public Person Tester { get; set; }

        #region Allors
        [Id("f56d7ad0-430d-482d-a298-5c41ffb082b4")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Derived]
        [Indexed]

        public EngineeringChangeStatus CurrentEngineeringChangeStatus { get; set; }

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
