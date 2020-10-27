// <copyright file="RequestItem.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("daf83fcc-832e-4d5e-ba71-5a08f42355db")]
    #endregion
    public partial class RequestItem : DelegatedAccessControlledObject, Commentable, Transitional, Versioned, Deletable
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public Permission[] TransitionalDeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public string Comment { get; set; }

        public LocalisedText[] LocalisedComments { get; set; }

        public ObjectState[] PreviousObjectStates { get; set; }

        public ObjectState[] LastObjectStates { get; set; }

        public ObjectState[] ObjectStates { get; set; }

        public User CreatedBy { get; set; }

        public User LastModifiedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastModifiedDate { get; set; }
        #endregion

        #region ObjectStates
        #region RequestItemState
        #region Allors
        [Id("0F6064F5-C92E-44C7-A050-F9599F1BE78D")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public RequestItemState PreviousRequestItemState { get; set; }

        #region Allors
        [Id("B8D32D2C-5E6E-40A5-8C5D-EB5E5A6F32A9")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public RequestItemState LastRequestItemState { get; set; }

        #region Allors
        [Id("D90E2400-498A-436C-B456-5B64E0E6A3E2")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public RequestItemState RequestItemState { get; set; }
        #endregion
        #endregion

        #region Versioning
        #region Allors
        [Id("AE96F162-EE98-47BA-940F-45C81412702D")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public RequestItemVersion CurrentVersion { get; set; }

        #region Allors
        [Id("0E0D556B-BB62-4E88-A127-C6CD1C9CB4AB")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public RequestItemVersion[] AllVersions { get; set; }
        #endregion

        #region Allors
        [Id("1A82EB63-123E-4055-BA4C-8DAA62648565")]
        #endregion
        [Workspace(Default)]
        [Size(-1)]
        public string InternalComment { get; set; }

        #region Allors
        [Id("542f3de9-e808-443b-b6e6-baf2db1ec2b1")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        public string Description { get; set; }

        #region Allors
        [Id("5c0f0069-b7f9-47f1-8346-c30f14afbc0c")]
        #endregion
        [Required]
        [Workspace(Default)]
        public int Quantity { get; set; }

        #region Allors
        [Id("ba4e5861-6e5b-4baf-96fd-350f6a70b663")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Required]
        [Workspace(Default)]
        public decimal AssignedUnitPrice { get; set; }

        #region Allors
        [Id("B48D0207-26CD-4A63-922F-69EC62704200")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace(Default)]
        public UnitOfMeasure UnitOfMeasure { get; set; }

        #region Allors
        [Id("6544faeb-a4cf-447c-a696-b6561c45086e")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        public Requirement[] Requirements { get; set; }

        #region Allors
        [Id("a5d1bef9-3086-4c32-9a6d-ce33c4f09539")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Deliverable Deliverable { get; set; }

        #region Allors
        [Id("bf40cb6b-e561-4df1-9ac4-e5a72933c7db")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public ProductFeature ProductFeature { get; set; }

        #region Allors
        [Id("d02d15ae-2938-4753-95f1-686ea8b02f47")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public NeededSkill NeededSkill { get; set; }

        #region Allors
        [Id("f03c07b5-44f2-4e61-ad23-7c373851dafc")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Product Product { get; set; }

        #region Allors
        [Id("A3286D1A-0064-404E-B1A2-A3C9B52D7D7A")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public SerialisedItem SerialisedItem { get; set; }

        #region Allors
        [Id("fa33c3e6-53c4-428a-bd9c-feba1dd9ed45")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal MaximumAllowedPrice { get; set; }

        #region Allors
        [Id("ff41a43c-997d-4158-984e-e669eb935148")]
        #endregion
        [Workspace(Default)]
        public DateTime RequiredByDate { get; set; }

        #region Allors
        [Id("A6AF9B26-D056-4CD4-BAFA-357EE754AB9A")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Synced]
        public Request SyncedRequest { get; set; }

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

        public void DelegateAccess() { }

        #endregion

        #region Allors
        [Id("67541211-256A-4CC8-BCAB-EEA2CCAEBE5F")]
        #endregion
        [Workspace(Default)]
        public void Cancel() { }

        #region Allors
        [Id("C3342A3D-A82C-4AD7-815C-921E7A19B5E3")]
        #endregion
        [Workspace(Default)]
        public void Submit() { }

        #region Allors
        [Id("{7B95A518-9656-4E18-B10B-CB3C59F2229A}")]
        #endregion
        [Workspace(Default)]
        public void Hold() { }
    }
}
