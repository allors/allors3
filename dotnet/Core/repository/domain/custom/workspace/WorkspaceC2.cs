// <copyright file="C2.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;


    #region Allors
    [Id("e31f4e5c-646e-4648-9b3e-ef9bf71e04ea")]
    #endregion
    [Workspace(Default)]
    [Origin(Origin.Workspace)]
    public partial class WorkspaceC2 : Object, DerivationCounted, WorkspaceI2
    {
        #region inherited properties

        public int DerivationCount { get; set; }

        #region Workspace

        public WorkspaceI2 WorkspaceI2WorkspaceI2Many2One { get; set; }

        public WorkspaceC1 WorkspaceI2WorkspaceC1Many2One { get; set; }

        public WorkspaceI12 WorkspaceI2WorkspaceI12Many2One { get; set; }

        public bool WorkspaceI2AllorsBoolean { get; set; }

        public WorkspaceC1[] WorkspaceI2WorkspaceC1One2Manies { get; set; }

        public WorkspaceC1 WorkspaceI2WorkspaceC1One2One { get; set; }

        public decimal WorkspaceI2AllorsDecimal { get; set; }

        public WorkspaceI2[] WorkspaceI2WorkspaceI2Many2Manies { get; set; }

        public byte[] WorkspaceI2AllorsBinary { get; set; }

        public Guid WorkspaceI2AllorsUnique { get; set; }

        public WorkspaceI1 WorkspaceI2WorkspaceI1Many2One { get; set; }

        public DateTime WorkspaceI2AllorsDateTime { get; set; }

        public WorkspaceI12[] WorkspaceI2WorkspaceI12One2Manies { get; set; }

        public WorkspaceI12 WorkspaceI2WorkspaceI12One2One { get; set; }

        public WorkspaceC2[] WorkspaceI2WorkspaceC2Many2Manies { get; set; }

        public WorkspaceI1[] WorkspaceI2WorkspaceI1Many2Manies { get; set; }

        public WorkspaceC2 WorkspaceI2WorkspaceC2Many2One { get; set; }

        public string WorkspaceI2AllorsString { get; set; }

        public WorkspaceC2[] WorkspaceI2WorkspaceC2One2Manies { get; set; }

        public WorkspaceI1 WorkspaceI2WorkspaceI1One2One { get; set; }

        public WorkspaceI1[] WorkspaceI2WorkspaceI1One2Manies { get; set; }

        public WorkspaceI12[] WorkspaceI2WorkspaceI12Many2Manies { get; set; }

        public WorkspaceI2 WorkspaceI2WorkspaceI2One2One { get; set; }

        public int WorkspaceI2AllorsInteger { get; set; }

        public WorkspaceI2[] WorkspaceI2WorkspaceI2One2Manies { get; set; }

        public WorkspaceC1[] WorkspaceI2WorkspaceC1Many2Manies { get; set; }

        public WorkspaceC2 WorkspaceI2WorkspaceC2One2One { get; set; }

        public double WorkspaceI2AllorsDouble { get; set; }

        public byte[] WorkspaceI12AllorsBinary { get; set; }

        public WorkspaceC2 WorkspaceI12WorkspaceC2One2One { get; set; }

        public double WorkspaceI12AllorsDouble { get; set; }

        public WorkspaceI1 WorkspaceI12WorkspaceI1Many2One { get; set; }

        public string WorkspaceI12AllorsString { get; set; }

        public WorkspaceI12[] WorkspaceI12WorkspaceI12Many2Manies { get; set; }

        public decimal WorkspaceI12AllorsDecimal { get; set; }

        public WorkspaceI2[] WorkspaceI12WorkspaceI2Many2Manies { get; set; }

        public WorkspaceC2[] WorkspaceI12WorkspaceC2Many2Manies { get; set; }

        public WorkspaceI1[] WorkspaceI12WorkspaceI1Many2Manies { get; set; }

        public WorkspaceI12[] WorkspaceI12WorkspaceI12One2Manies { get; set; }

        public string WorkspaceName { get; set; }

        public WorkspaceC1[] WorkspaceI12WorkspaceC1Many2Manies { get; set; }

        public WorkspaceI2 WorkspaceI12WorkspaceI2Many2One { get; set; }

        public Guid WorkspaceI12AllorsUnique { get; set; }

        public int WorkspaceI12AllorsInteger { get; set; }

        public WorkspaceI1[] WorkspaceI12WorkspaceI1One2Manies { get; set; }

        public WorkspaceC1 WorkspaceI12WorkspaceC1One2One { get; set; }

        public WorkspaceI12 WorkspaceI12WorkspaceI12One2One { get; set; }

        public WorkspaceI2 WorkspaceI12WorkspaceI2One2One { get; set; }

        public WorkspaceI12[] WorkspaceDependencies { get; set; }

        public WorkspaceI2[] WorkspaceI12WorkspaceI2One2Manies { get; set; }

        public WorkspaceC2 WorkspaceI12WorkspaceC2Many2One { get; set; }

        public WorkspaceI12 WorkspaceI12WorkspaceI12Many2One { get; set; }

        public bool WorkspaceI12AllorsBoolean { get; set; }

        public WorkspaceI1 WorkspaceI12WorkspaceI1One2One { get; set; }

        public WorkspaceC1[] WorkspaceI12WorkspaceC1One2Manies { get; set; }

        public WorkspaceC1 WorkspaceI12WorkspaceC1Many2One { get; set; }

        public DateTime WorkspaceI12AllorsDateTime { get; set; }

        #endregion

        #region Database

        public I2 WorkspaceI2DatabaseI2Many2One { get; set; }

        public C1 WorkspaceI2DatabaseC1Many2One { get; set; }

        public I12 WorkspaceI2DatabaseI12Many2One { get; set; }

        public bool DatabaseI2AllorsBoolean { get; set; }

        public C1[] WorkspaceI2DatabaseC1One2Manies { get; set; }

        public C1 WorkspaceI2DatabaseC1One2One { get; set; }

        public decimal DatabaseI2AllorsDecimal { get; set; }

        public I2[] WorkspaceI2DatabaseI2Many2Manies { get; set; }

        public byte[] DatabaseI2AllorsBinary { get; set; }

        public Guid DatabaseI2AllorsUnique { get; set; }

        public I1 WorkspaceI2DatabaseI1Many2One { get; set; }

        public DateTime DatabaseI2AllorsDateTime { get; set; }

        public I12[] WorkspaceI2DatabaseI12One2Manies { get; set; }

        public I12 WorkspaceI2DatabaseI12One2One { get; set; }

        public C2[] WorkspaceI2DatabaseC2Many2Manies { get; set; }

        public I1[] WorkspaceI2DatabaseI1Many2Manies { get; set; }

        public C2 WorkspaceI2DatabaseC2Many2One { get; set; }

        public string DatabaseI2AllorsString { get; set; }

        public C2[] WorkspaceI2DatabaseC2One2Manies { get; set; }

        public I1 WorkspaceI2DatabaseI1One2One { get; set; }

        public I1[] WorkspaceI2DatabaseI1One2Manies { get; set; }

        public I12[] WorkspaceI2DatabaseI12Many2Manies { get; set; }

        public I2 WorkspaceI2DatabaseI2One2One { get; set; }

        public int DatabaseI2AllorsInteger { get; set; }

        public I2[] WorkspaceI2DatabaseI2One2Manies { get; set; }

        public C1[] WorkspaceI2DatabaseC1Many2Manies { get; set; }

        public C2 WorkspaceI2DatabaseC2One2One { get; set; }

        public double DatabaseI2AllorsDouble { get; set; }

        public byte[] DatabaseI12AllorsBinary { get; set; }

        public C2 WorkspaceI12DatabaseC2One2One { get; set; }

        public double DatabaseI12AllorsDouble { get; set; }

        public I1 WorkspaceI12DatabaseI1Many2One { get; set; }

        public string DatabaseI12AllorsString { get; set; }

        public I12[] WorkspaceI12DatabaseI12Many2Manies { get; set; }

        public decimal DatabaseI12AllorsDecimal { get; set; }

        public I2[] WorkspaceI12DatabaseI2Many2Manies { get; set; }

        public C2[] WorkspaceI12DatabaseC2Many2Manies { get; set; }

        public I1[] WorkspaceI12DatabaseI1Many2Manies { get; set; }

        public I12[] WorkspaceI12DatabaseI12One2Manies { get; set; }

        public string DatabaseName { get; set; }

        public C1[] WorkspaceI12DatabaseC1Many2Manies { get; set; }

        public I2 WorkspaceI12DatabaseI2Many2One { get; set; }

        public Guid DatabaseI12AllorsUnique { get; set; }

        public int DatabaseI12AllorsInteger { get; set; }

        public I1[] WorkspaceI12DatabaseI1One2Manies { get; set; }

        public C1 WorkspaceI12DatabaseC1One2One { get; set; }

        public I12 WorkspaceI12DatabaseI12One2One { get; set; }

        public I2 WorkspaceI12DatabaseI2One2One { get; set; }

        public I12[] DatabaseDependencies { get; set; }

        public I2[] WorkspaceI12DatabaseI2One2Manies { get; set; }

        public C2 WorkspaceI12DatabaseC2Many2One { get; set; }

        public I12 WorkspaceI12DatabaseI12Many2One { get; set; }

        public bool DatabaseI12AllorsBoolean { get; set; }

        public I1 WorkspaceI12DatabaseI1One2One { get; set; }

        public C1[] WorkspaceI12DatabaseC1One2Manies { get; set; }

        public C1 WorkspaceI12DatabaseC1Many2One { get; set; }

        public DateTime DatabaseI12AllorsDateTime { get; set; }

        #endregion

        public bool ChangedRolePingS12 { get; set; }
        public bool ChangedRolePongS12 { get; set; }
        public bool ChangedRolePingI12 { get; set; }
        public bool ChangedRolePongI12 { get; set; }
        public bool ChangedRolePingI1 { get; set; }
        public bool ChangedRolePongI1 { get; set; }
        public bool ChangedRolePingC1 { get; set; }
        public bool ChangedRolePongC1 { get; set; }

        #endregion

        #region Allors
        [Id("762ea90c-ae3b-4c1e-8878-8fa4f2055670")]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        public decimal WorkspaceC2AllorsDecimal { get; set; }

        #region Allors
        [Id("36206a2c-ab38-4528-a62e-27f368159160")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        public WorkspaceC1 WorkspaceC2WorkspaceC1One2One { get; set; }

        #region Allors
        [Id("bca2af47-f3b5-4585-9c0c-d8cd91a31160")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        public WorkspaceC2 WorkspaceC2WorkspaceC2Many2One { get; set; }

        #region Allors
        [Id("7cfc96b5-85e8-4509-b4e6-e1916c23a20b")]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        public Guid WorkspaceC2AllorsUnique { get; set; }

        #region Allors
        [Id("b08aa308-acdb-4dae-a4c6-920824864c67")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        public WorkspaceI12 WorkspaceC2WorkspaceI12Many2One { get; set; }

        #region Allors
        [Id("0f1c6b4d-70fe-4787-b5e6-c39634fc82f1")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        public WorkspaceI12 WorkspaceC2WorkspaceI12One2One { get; set; }

        #region Allors
        [Id("8208a723-5d9d-44f6-8262-17416301dd23")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        public WorkspaceI1[] WorkspaceC2WorkspaceI1Many2Manies { get; set; }

        #region Allors
        [Id("2cd80e78-790f-43dc-a96d-ed9c0919bdd2")]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        public double WorkspaceC2AllorsDouble { get; set; }

        #region Allors
        [Id("27b49e78-4d1f-4a23-83ab-be99b5749ef4")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        public WorkspaceI1[] WorkspaceC2WorkspaceI1One2Manies { get; set; }

        #region Allors
        [Id("89715dad-701d-46fa-9e3e-98e778998811")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        public WorkspaceI2 WorkspaceC2WorkspaceI2One2One { get; set; }

        #region Allors
        [Id("55559ee8-f313-446e-962c-b432d12899be")]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        public int WorkspaceC2AllorsInteger { get; set; }

        #region Allors
        [Id("b6b54c67-e95b-4a38-9998-2c878f9216c1")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        public WorkspaceI2[] WorkspaceC2WorkspaceI2Many2Manies { get; set; }

        #region Allors
        [Id("d100c05c-fba9-4326-82fe-08d29f99fff8")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        public WorkspaceI12[] WorkspaceC2WorkspaceI12Many2Manies { get; set; }

        #region Allors
        [Id("91dcbef5-5a43-4e51-a5ee-5d58359689db")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        public WorkspaceC2[] WorkspaceC2WorkspaceC2One2Manies { get; set; }

        #region Allors
        [Id("7ef08563-7351-47ad-bd2b-2f2d343400f9")]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        public bool WorkspaceC2AllorsBoolean { get; set; }

        #region Allors
        [Id("fcb1b25f-ea08-4021-94c2-0dc48482dcf0")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        public WorkspaceI1 WorkspaceC2WorkspaceI1Many2One { get; set; }

        #region Allors
        [Id("c318163e-72cf-4a4d-be78-6a9bfd2b51a6")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        public WorkspaceI1 WorkspaceC2WorkspaceI1One2One { get; set; }

        #region Allors
        [Id("c83690f4-2b4d-4439-aabf-9ff4ab7590e4")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        public WorkspaceC1[] WorkspaceC2WorkspaceC1Many2Manies { get; set; }

        #region Allors
        [Id("d373f606-411b-4424-8b95-5ba63e5c0927")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        public WorkspaceI12[] WorkspaceC2WorkspaceI12One2Manies { get; set; }

        #region Allors
        [Id("98b776b1-f901-48f9-8d30-61bedc644dd6")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        public WorkspaceI2[] WorkspaceC2WorkspaceI2One2Manies { get; set; }

        #region Allors
        [Id("ab6b657c-ebbd-4e90-960f-0f0b83a8dc08")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        public WorkspaceC2 WorkspaceC2WorkspaceC2One2One { get; set; }

        #region Allors
        [Id("9dcd6b76-df40-4037-9a68-d124113af05e")]
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        public string WorkspaceC2AllorsString { get; set; }

        #region Allors
        [Id("fac081a2-1623-4239-8862-818c9e794cde")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        public WorkspaceC1 WorkspaceC2WorkspaceC1Many2One { get; set; }

        #region Allors
        [Id("4d213dd6-28cb-4ae1-b9c8-24d28e4b7903")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        public WorkspaceC2[] WorkspaceC2WorkspaceC2Many2Manies { get; set; }

        #region Allors
        [Id("b6aa18ce-d309-4af0-a0a9-8051080e3d76")]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        public DateTime WorkspaceC2AllorsDateTime { get; set; }

        #region Allors
        [Id("bb77b01e-6fb7-4cf7-a369-9c61376eeff8")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        public WorkspaceI2 WorkspaceC2WorkspaceI2Many2One { get; set; }

        #region Allors
        [Id("84b6f42d-f5e1-4244-ae86-062cce549d05")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        public WorkspaceC1[] WorkspaceC2WorkspaceC1One2Manies { get; set; }

        #region Allors
        [Id("af1c0a93-348f-49a2-a5f9-bcec1c087219")]
        [Size(-1)]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        public byte[] WorkspaceC2AllorsBinary { get; set; }

        #region Allors
        [Id("b55903be-d957-4151-a8fb-de4fff81c086")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        #endregion
        public S1 WorkspaceS1One2One { get; set; }

        #region inherited methods

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        #endregion
    }
}
