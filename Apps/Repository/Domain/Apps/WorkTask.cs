
// <copyright file="WorkTask.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("76911215-A288-4B0D-BECE-83E7A617B847")]
    #endregion
    public partial class WorkTask : WorkEffort, Versioned
    {
        #region inherited properties

        public Organisation TakenBy { get; set; }

        public Organisation ExecutedBy { get; set; }

        // public Store Store { get; set; }
        public Party Customer { get; set; }

        public ContactMechanism FullfillContactMechanism { get; set; }

        public string WorkEffortNumber { get; set; }

        public Person Owner { get; set; }

        public Person ContactPerson { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string WorkDone { get; set; }

        public Priority Priority { get; set; }

        public WorkEffortPurpose[] WorkEffortPurposes { get; set; }

        public DateTime ActualCompletion { get; set; }

        public DateTime ScheduledStart { get; set; }

        public DateTime ScheduledCompletion { get; set; }

        public decimal ActualHours { get; set; }

        public decimal EstimatedHours { get; set; }

        public decimal TotalLabourCost { get; set; }

        public decimal TotalMaterialCost { get; set; }

        public decimal TotalSubContractedCost { get; set; }

        public decimal TotalCost { get; set; }

        public decimal TotalLabourRevenue { get; set; }

        public decimal TotalMaterialRevenue { get; set; }

        public decimal TotalSubContractedRevenue { get; set; }

        public decimal TotalRevenue { get; set; }

        public decimal GrandTotal { get; set; }

        public WorkEffort[] Precendencies { get; set; }

        public Facility Facility { get; set; }

        public Deliverable[] DeliverablesProduced { get; set; }

        public DateTime ActualStart { get; set; }

        public WorkEffort[] Children { get; set; }

        public OrderItem OrderItemFulfillment { get; set; }

        public WorkEffortType WorkEffortType { get; set; }

        public Requirement[] RequirementFulfillments { get; set; }

        public string SpecialTerms { get; set; }

        public WorkEffort[] Concurrencies { get; set; }

        public bool CanInvoice { get; set; }

        public WorkEffortState PreviousWorkEffortState { get; set; }

        public WorkEffortState LastWorkEffortState { get; set; }

        public WorkEffortState WorkEffortState { get; set; }

        public ObjectState[] PreviousObjectStates { get; set; }

        public ObjectState[] LastObjectStates { get; set; }

        public ObjectState[] ObjectStates { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid UniqueId { get; set; }

        public User CreatedBy { get; set; }

        public User LastModifiedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public PrintDocument PrintDocument { get; set; }

        public string Comment { get; set; }

        public LocalisedText[] LocalisedComments { get; set; }

        public Media[] ElectronicDocuments { get; set; }

        public int SortableWorkEffortNumber { get; set; }

        public Guid DerivationTrigger { get; set; }

        #endregion

        #region Allors
        [Id("A1070CB5-3492-408C-959A-1C0785C774A0")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public bool SendNotification { get; set; }

        #region Allors
        [Id("55229180-203E-4743-B41B-DA4B4FC1B079")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public bool SendReminder { get; set; }

        #region Allors
        [Id("413541ED-963E-4036-9347-047456F211E6")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public DateTime RemindAt { get; set; }

        #region Versioning
        #region Allors
        [Id("D5808960-5987-435A-B7B6-4D75B8DE8FE2")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public WorkTaskVersion CurrentVersion { get; set; }

        #region Allors
        [Id("78A2865E-C4FF-4EDD-AA05-AC35EF3C59AE")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public WorkTaskVersion[] AllVersions { get; set; }
        #endregion

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        public void Complete() { }

        public void Invoice() { }

        public void Cancel() { }

        public void Reopen() { }

        public void Delete() { }

        public void Print() { }

        public void Revise() { }

        public void CalculateTotalRevenue() { }

        #endregion
    }
}
