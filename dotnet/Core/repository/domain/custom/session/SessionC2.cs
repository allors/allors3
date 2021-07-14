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
    [Id("1467598c-6e4c-4d34-9831-ee36026495b6")]
    #endregion
    [Workspace(Default)]
    [Origin(Origin.Session)]
    public partial class SessionC2 : Object, DerivationCounted, SessionI2
    {
        #region inherited properties

        public int DerivationCount { get; set; }

        #region Session

        public SessionI2 SessionI2SessionI2Many2One { get; set; }

        public SessionC1 SessionI2SessionC1Many2One { get; set; }

        public SessionI12 SessionI2SessionI12Many2One { get; set; }

        public bool SessionI2AllorsBoolean { get; set; }

        public SessionC1[] SessionI2SessionC1One2Manies { get; set; }

        public SessionC1 SessionI2SessionC1One2One { get; set; }

        public decimal SessionI2AllorsDecimal { get; set; }

        public SessionI2[] SessionI2SessionI2Many2Manies { get; set; }

        public byte[] SessionI2AllorsBinary { get; set; }

        public Guid SessionI2AllorsUnique { get; set; }

        public SessionI1 SessionI2SessionI1Many2One { get; set; }

        public DateTime SessionI2AllorsDateTime { get; set; }

        public SessionI12[] SessionI2SessionI12One2Manies { get; set; }

        public SessionI12 SessionI2SessionI12One2One { get; set; }

        public SessionC2[] SessionI2SessionC2Many2Manies { get; set; }

        public SessionI1[] SessionI2SessionI1Many2Manies { get; set; }

        public SessionC2 SessionI2SessionC2Many2One { get; set; }

        public string SessionI2AllorsString { get; set; }

        public SessionC2[] SessionI2SessionC2One2Manies { get; set; }

        public SessionI1 SessionI2SessionI1One2One { get; set; }

        public SessionI1[] SessionI2SessionI1One2Manies { get; set; }

        public SessionI12[] SessionI2SessionI12Many2Manies { get; set; }

        public SessionI2 SessionI2SessionI2One2One { get; set; }

        public int SessionI2AllorsInteger { get; set; }

        public SessionI2[] SessionI2SessionI2One2Manies { get; set; }

        public SessionC1[] SessionI2SessionC1Many2Manies { get; set; }

        public SessionC2 SessionI2SessionC2One2One { get; set; }

        public double SessionI2AllorsDouble { get; set; }

        public byte[] SessionI12AllorsBinary { get; set; }

        public SessionC2 SessionI12SessionC2One2One { get; set; }

        public double SessionI12AllorsDouble { get; set; }

        public SessionI1 SessionI12SessionI1Many2One { get; set; }

        public string SessionI12AllorsString { get; set; }

        public SessionI12[] SessionI12SessionI12Many2Manies { get; set; }

        public decimal SessionI12AllorsDecimal { get; set; }

        public SessionI2[] SessionI12SessionI2Many2Manies { get; set; }

        public SessionC2[] SessionI12SessionC2Many2Manies { get; set; }

        public SessionI1[] SessionI12SessionI1Many2Manies { get; set; }

        public SessionI12[] SessionI12SessionI12One2Manies { get; set; }

        public string SessionName { get; set; }

        public SessionC1[] SessionI12SessionC1Many2Manies { get; set; }

        public SessionI2 SessionI12SessionI2Many2One { get; set; }

        public Guid SessionI12AllorsUnique { get; set; }

        public int SessionI12AllorsInteger { get; set; }

        public SessionI1[] SessionI12SessionI1One2Manies { get; set; }

        public SessionC1 SessionI12SessionC1One2One { get; set; }

        public SessionI12 SessionI12SessionI12One2One { get; set; }

        public SessionI2 SessionI12SessionI2One2One { get; set; }

        public SessionI12[] SessionDependencies { get; set; }

        public SessionI2[] SessionI12SessionI2One2Manies { get; set; }

        public SessionC2 SessionI12SessionC2Many2One { get; set; }

        public SessionI12 SessionI12SessionI12Many2One { get; set; }

        public bool SessionI12AllorsBoolean { get; set; }

        public SessionI1 SessionI12SessionI1One2One { get; set; }

        public SessionC1[] SessionI12SessionC1One2Manies { get; set; }

        public SessionC1 SessionI12SessionC1Many2One { get; set; }

        public DateTime SessionI12AllorsDateTime { get; set; }

        #endregion

        #region Workspace

        public WorkspaceI2 SessionI2WorkspaceI2Many2One { get; set; }

        public WorkspaceC1 SessionI2WorkspaceC1Many2One { get; set; }

        public WorkspaceI12 SessionI2WorkspaceI12Many2One { get; set; }

        public bool WorkspaceI2AllorsBoolean { get; set; }

        public WorkspaceC1[] SessionI2WorkspaceC1One2Manies { get; set; }

        public WorkspaceC1 SessionI2WorkspaceC1One2One { get; set; }

        public decimal WorkspaceI2AllorsDecimal { get; set; }

        public WorkspaceI2[] SessionI2WorkspaceI2Many2Manies { get; set; }

        public byte[] WorkspaceI2AllorsBinary { get; set; }

        public Guid WorkspaceI2AllorsUnique { get; set; }

        public WorkspaceI1 SessionI2WorkspaceI1Many2One { get; set; }

        public DateTime WorkspaceI2AllorsDateTime { get; set; }

        public WorkspaceI12[] SessionI2WorkspaceI12One2Manies { get; set; }

        public WorkspaceI12 SessionI2WorkspaceI12One2One { get; set; }

        public WorkspaceC2[] SessionI2WorkspaceC2Many2Manies { get; set; }

        public WorkspaceI1[] SessionI2WorkspaceI1Many2Manies { get; set; }

        public WorkspaceC2 SessionI2WorkspaceC2Many2One { get; set; }

        public string WorkspaceI2AllorsString { get; set; }

        public WorkspaceC2[] SessionI2WorkspaceC2One2Manies { get; set; }

        public WorkspaceI1 SessionI2WorkspaceI1One2One { get; set; }

        public WorkspaceI1[] SessionI2WorkspaceI1One2Manies { get; set; }

        public WorkspaceI12[] SessionI2WorkspaceI12Many2Manies { get; set; }

        public WorkspaceI2 SessionI2WorkspaceI2One2One { get; set; }

        public int WorkspaceI2AllorsInteger { get; set; }

        public WorkspaceI2[] SessionI2WorkspaceI2One2Manies { get; set; }

        public WorkspaceC1[] SessionI2WorkspaceC1Many2Manies { get; set; }

        public WorkspaceC2 SessionI2WorkspaceC2One2One { get; set; }

        public double WorkspaceI2AllorsDouble { get; set; }

        public byte[] WorkspaceI12AllorsBinary { get; set; }

        public WorkspaceC2 SessionI12WorkspaceC2One2One { get; set; }

        public double WorkspaceI12AllorsDouble { get; set; }

        public WorkspaceI1 SessionI12WorkspaceI1Many2One { get; set; }

        public string WorkspaceI12AllorsString { get; set; }

        public WorkspaceI12[] SessionI12WorkspaceI12Many2Manies { get; set; }

        public decimal WorkspaceI12AllorsDecimal { get; set; }

        public WorkspaceI2[] SessionI12WorkspaceI2Many2Manies { get; set; }

        public WorkspaceC2[] SessionI12WorkspaceC2Many2Manies { get; set; }

        public WorkspaceI1[] SessionI12WorkspaceI1Many2Manies { get; set; }

        public WorkspaceI12[] SessionI12WorkspaceI12One2Manies { get; set; }

        public string WorkspaceName { get; set; }

        public WorkspaceC1[] SessionI12WorkspaceC1Many2Manies { get; set; }

        public WorkspaceI2 SessionI12WorkspaceI2Many2One { get; set; }

        public Guid WorkspaceI12AllorsUnique { get; set; }

        public int WorkspaceI12AllorsInteger { get; set; }

        public WorkspaceI1[] SessionI12WorkspaceI1One2Manies { get; set; }

        public WorkspaceC1 SessionI12WorkspaceC1One2One { get; set; }

        public WorkspaceI12 SessionI12WorkspaceI12One2One { get; set; }

        public WorkspaceI2 SessionI12WorkspaceI2One2One { get; set; }

        public WorkspaceI12[] WorkspaceDependencies { get; set; }

        public WorkspaceI2[] SessionI12WorkspaceI2One2Manies { get; set; }

        public WorkspaceC2 SessionI12WorkspaceC2Many2One { get; set; }

        public WorkspaceI12 SessionI12WorkspaceI12Many2One { get; set; }

        public bool WorkspaceI12AllorsBoolean { get; set; }

        public WorkspaceI1 SessionI12WorkspaceI1One2One { get; set; }

        public WorkspaceC1[] SessionI12WorkspaceC1One2Manies { get; set; }

        public WorkspaceC1 SessionI12WorkspaceC1Many2One { get; set; }

        public DateTime WorkspaceI12AllorsDateTime { get; set; }

        #endregion

        #region Database

        public I2 SessionI2DatabaseI2Many2One { get; set; }

        public C1 SessionI2DatabaseC1Many2One { get; set; }

        public I12 SessionI2DatabaseI12Many2One { get; set; }

        public bool DatabaseI2AllorsBoolean { get; set; }

        public C1[] SessionI2DatabaseC1One2Manies { get; set; }

        public C1 SessionI2DatabaseC1One2One { get; set; }

        public decimal DatabaseI2AllorsDecimal { get; set; }

        public I2[] SessionI2DatabaseI2Many2Manies { get; set; }

        public byte[] DatabaseI2AllorsBinary { get; set; }

        public Guid DatabaseI2AllorsUnique { get; set; }

        public I1 SessionI2DatabaseI1Many2One { get; set; }

        public DateTime DatabaseI2AllorsDateTime { get; set; }

        public I12[] SessionI2DatabaseI12One2Manies { get; set; }

        public I12 SessionI2DatabaseI12One2One { get; set; }

        public C2[] SessionI2DatabaseC2Many2Manies { get; set; }

        public I1[] SessionI2DatabaseI1Many2Manies { get; set; }

        public C2 SessionI2DatabaseC2Many2One { get; set; }

        public string DatabaseI2AllorsString { get; set; }

        public C2[] SessionI2DatabaseC2One2Manies { get; set; }

        public I1 SessionI2DatabaseI1One2One { get; set; }

        public I1[] SessionI2DatabaseI1One2Manies { get; set; }

        public I12[] SessionI2DatabaseI12Many2Manies { get; set; }

        public I2 SessionI2DatabaseI2One2One { get; set; }

        public int DatabaseI2AllorsInteger { get; set; }

        public I2[] SessionI2DatabaseI2One2Manies { get; set; }

        public C1[] SessionI2DatabaseC1Many2Manies { get; set; }

        public C2 SessionI2DatabaseC2One2One { get; set; }

        public double DatabaseI2AllorsDouble { get; set; }

        public byte[] DatabaseI12AllorsBinary { get; set; }

        public C2 SessionI12DatabaseC2One2One { get; set; }

        public double DatabaseI12AllorsDouble { get; set; }

        public I1 SessionI12DatabaseI1Many2One { get; set; }

        public string DatabaseI12AllorsString { get; set; }

        public I12[] SessionI12DatabaseI12Many2Manies { get; set; }

        public decimal DatabaseI12AllorsDecimal { get; set; }

        public I2[] SessionI12DatabaseI2Many2Manies { get; set; }

        public C2[] SessionI12DatabaseC2Many2Manies { get; set; }

        public I1[] SessionI12DatabaseI1Many2Manies { get; set; }

        public I12[] SessionI12DatabaseI12One2Manies { get; set; }

        public string DatabaseName { get; set; }

        public C1[] SessionI12DatabaseC1Many2Manies { get; set; }

        public I2 SessionI12DatabaseI2Many2One { get; set; }

        public Guid DatabaseI12AllorsUnique { get; set; }

        public int DatabaseI12AllorsInteger { get; set; }

        public I1[] SessionI12DatabaseI1One2Manies { get; set; }

        public C1 SessionI12DatabaseC1One2One { get; set; }

        public I12 SessionI12DatabaseI12One2One { get; set; }

        public I2 SessionI12DatabaseI2One2One { get; set; }

        public I12[] DatabaseDependencies { get; set; }

        public I2[] SessionI12DatabaseI2One2Manies { get; set; }

        public C2 SessionI12DatabaseC2Many2One { get; set; }

        public I12 SessionI12DatabaseI12Many2One { get; set; }

        public bool DatabaseI12AllorsBoolean { get; set; }

        public I1 SessionI12DatabaseI1One2One { get; set; }

        public C1[] SessionI12DatabaseC1One2Manies { get; set; }

        public C1 SessionI12DatabaseC1Many2One { get; set; }

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
        [Id("1aafc02f-c0f8-4496-99d4-515957fe8867")]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        public decimal SessionC2AllorsDecimal { get; set; }

        #region Allors
        [Id("bc11237f-5db4-4638-b73d-10bc15d6c189")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        public C1 SessionC2C1One2One { get; set; }

        #region Allors
        [Id("6d9bca0b-1406-4fdc-84db-120852f3fdfd")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        public C2 SessionC2C2Many2One { get; set; }

        #region Allors
        [Id("372163ab-a95f-47d9-ac1c-542e9873b677")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        public Guid SessionC2AllorsUnique { get; set; }

        #region Allors
        [Id("9f254ae0-6c9c-488e-8441-d40dcefcd02f")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        public I12 SessionC2I12Many2One { get; set; }

        #region Allors
        [Id("3e51ca10-f92f-4db5-a77d-88a9f49009c9")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        public I12 SessionC2I12One2One { get; set; }

        #region Allors
        [Id("9b60035d-36e7-4628-8336-3088511b1634")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        public I1[] SessionC2I1Many2Manies { get; set; }

        #region Allors
        [Id("f3b342fa-8f98-468a-8de2-ee948216a7f4")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        public double SessionC2AllorsDouble { get; set; }

        #region Allors
        [Id("a8c6f019-233e-4043-ba4e-37e6fd795ed4")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        public I1[] SessionC2I1One2Manies { get; set; }

        #region Allors
        [Id("b24a7ba3-6323-4782-9894-96975721b759")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        public I2 SessionC2I2One2One { get; set; }

        #region Allors
        [Id("e2f9d0a7-b944-4e44-a9b3-d642b3b7f378")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        public int SessionC2AllorsInteger { get; set; }

        #region Allors
        [Id("d36c3c27-4f1f-42b5-9b0e-d377c936023b")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        public I2[] SessionC2I2Many2Manies { get; set; }

        #region Allors
        [Id("2d255363-67cf-4e57-9fce-d100c00320e2")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        public I12[] SessionC2I12Many2Manies { get; set; }

        #region Allors
        [Id("ca868fa6-5f89-4489-899a-5a29025164e3")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        public C2[] SessionC2C2One2Manies { get; set; }

        #region Allors
        [Id("8a789d1d-211b-49cc-9dc7-4d55dd836884")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        public bool SessionC2AllorsBoolean { get; set; }

        #region Allors
        [Id("29f6f6c1-1add-4bc8-812c-f4e9b119d68c")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        public I1 SessionC2I1Many2One { get; set; }

        #region Allors
        [Id("81dadabd-a71e-4abf-85db-061174e2bbe1")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        public I1 SessionC2I1One2One { get; set; }

        #region Allors
        [Id("42e655eb-50f7-499e-9a25-8de1e837e931")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        public C1[] SessionC2C1Many2Manies { get; set; }

        #region Allors
        [Id("63e91cd7-82a6-4a71-9162-9bd9e6f3425e")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        public I12[] SessionC2I12One2Manies { get; set; }

        #region Allors
        [Id("a30939c9-e66a-4948-a7c7-d33a6e4278cb")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        public I2[] SessionC2I2One2Manies { get; set; }

        #region Allors
        [Id("e2183953-0afb-44e9-ad1c-2ad5736ea34a")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        public C2 SessionC2C2One2One { get; set; }

        #region Allors
        [Id("08db411c-f3d7-4e46-a09f-b1bead102cc7")]
        [Size(256)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        public string SessionC2AllorsString { get; set; }

        #region Allors
        [Id("f6aba7b9-9420-4e84-8971-880b0e68e431")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        public C1 SessionC2C1Many2One { get; set; }

        #region Allors
        [Id("8d63a633-4b89-4816-b15a-35fd2b49c244")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        public C2[] SessionC2C2Many2Manies { get; set; }

        #region Allors
        [Id("d9455a35-b387-4387-b8cf-520f3960e348")]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        public DateTime SessionC2AllorsDateTime { get; set; }

        #region Allors
        [Id("01987d00-ba3a-4ba4-85e1-f69d6c25217b")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        public I2 SessionC2I2Many2One { get; set; }

        #region Allors
        [Id("6d488788-384b-4927-954a-64247c7fbde6")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        public C1[] SessionC2C1One2Manies { get; set; }

        #region Allors
        [Id("4db835ab-8519-4a13-b0b7-c14d799c90fb")]
        [Size(-1)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        public byte[] SessionC2AllorsBinary { get; set; }

        #region Allors
        [Id("c7dca082-698a-4a73-97ea-540dc93a5b29")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        #endregion
        public S1 SessionS1One2One { get; set; }

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
