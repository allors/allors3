// <copyright file="I1.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;


    #region Allors
    [Id("fefcf1b6-ac8f-47b0-bed5-939207a2833e")]
    #endregion
    public partial interface I1 : Object, I12, S1
    {
        #region Allors
        [Id("06b72534-49a8-4f6d-a991-bc4aaf6f939f")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        I1 I1I1Many2One { get; set; }

        #region Allors
        [Id("0a2895ec-0102-493d-9b94-e12e94b4a403")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        I12[] I1I12Many2Manies { get; set; }

        #region Allors
        [Id("0acbea28-f8aa-477c-b296-b8976d9b10a5")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        I2[] I1I2Many2Manies { get; set; }

        #region Allors
        [Id("194580f4-e0e3-4b52-b9ba-6020171be4e9")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        I2 I1I2Many2One { get; set; }

        #region Allors
        [Id("28ceffc2-c776-4a0a-9825-a6d1bcb265dc")]
        [Size(256)]
        [Workspace(Default)]
        #endregion
        string I1AllorsString { get; set; }

        #region Allors
        [Id("2e85d74a-8d13-4bc0-ae4f-42b305e79373")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        I12 I1I12Many2One { get; set; }

        #region Allors
        [Id("32fc21cc-4be7-4a0e-ac71-df135be95e68")]
        [Workspace(Default)]
        #endregion
        DateTime I1AllorsDateTime { get; set; }

        #region Allors
        [Id("39e28141-fd6b-4f49-8884-d5400f6c57ff")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        I2[] I1I2One2Manies { get; set; }

        #region Allors
        [Id("4506a14b-22f1-41fe-972b-40fab7c6dd31")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        C2[] I1C2One2Manies { get; set; }

        #region Allors
        [Id("593914b1-af95-4992-9703-2b60f4ea0926")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        C1 I1C1One2One { get; set; }

        #region Allors
        [Id("5cb44331-fd8c-4f73-8994-161f702849b6")]
        [Workspace(Default)]
        #endregion
        int I1AllorsInteger { get; set; }

        #region Allors
        [Id("6199e5b4-133d-4d0e-9941-207316164ec8")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        C2[] I1C2Many2Manies { get; set; }

        #region Allors
        [Id("670c753e-8ea0-40b1-bfc9-7388074191d3")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        I1[] I1I1One2Manies { get; set; }

        #region Allors
        [Id("6bb3ba6d-ffc7-4700-9723-c323b9b2d233")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        I1[] I1I1Many2Manies { get; set; }

        #region Allors
        [Id("6c3d04be-6f95-44b8-863a-245e150e3110")]
        [Workspace(Default)]
        #endregion
        bool I1AllorsBoolean { get; set; }

        #region Allors
        [Id("818b4013-5ef1-4455-9f0d-9a39fa3425bb")]
        [Precision(10)]
        [Scale(2)]
        [Workspace(Default)]
        #endregion
        decimal I1AllorsDecimal { get; set; }

        #region Allors
        [Id("a51d9d21-40ec-44b9-853d-8c18f54d659d")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        I12 I1I12One2One { get; set; }

        #region Allors
        [Id("a5761a0e-5c10-407a-bd68-0c4f69d78968")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        I2 I1I2One2One { get; set; }

        #region Allors
        [Id("b6e0fce0-14fc-46e3-995d-1b6e3699ed96")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        C2 I1C2One2One { get; set; }

        #region Allors
        [Id("b89092f1-8775-4b6a-99ef-f8626bc770bd")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        C1[] I1C1One2Manies { get; set; }

        #region Allors
        [Id("b9c67658-4abc-41f3-9434-c8512a482179")]
        [Size(-1)]
        [Workspace(Default)]
        #endregion
        byte[] I1AllorsBinary { get; set; }

        #region Allors
        [Id("bcc9eee6-fa07-4d37-be84-b691bfce24be")]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        C1[] I1C1Many2Manies { get; set; }

        #region Allors
        [Id("cdb758bf-ecaf-4d99-88fb-58df9258c13c")]
        [Workspace(Default)]
        #endregion
        double I1AllorsDouble { get; set; }

        #region Allors
        [Id("e1b13216-7210-4c24-a668-83b40162a21b")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        I1 I1I1One2One { get; set; }

        #region Allors
        [Id("e3126228-342a-4415-a2e8-d52eceaeaf89")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        C1 I1C1Many2One { get; set; }

        #region Allors
        [Id("e386cca6-e738-4c37-8bfc-b23057d7a0be")]
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        I12[] I1I12One2Manies { get; set; }

        #region Allors
        [Id("ef1a0a5e-1794-4478-9d0a-517182355206")]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        #endregion
        C2 I1C2Many2One { get; set; }

        #region Allors
        [Id("f9d7411e-7993-4e43-a7e2-726f1e44e29c")]
        [Workspace(Default)]
        #endregion
        Guid I1AllorsUnique { get; set; }

        #region Allors
        [Id("A360CF09-7B55-421B-A45D-D100BAF3D0D6")]
        #endregion
        void InterfaceMethod();
    }
}
