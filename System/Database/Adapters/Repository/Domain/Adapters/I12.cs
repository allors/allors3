// <copyright file="I12.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Attributes;

    #region Allors
    [Id("97755724-b934-4cc5-beb4-3d49a7a4b27e")]
    #endregion
    public partial interface I12 : Object, S12
    {
        #region Allors
        [Id("1a0eb6ea-d877-42c9-a35a-889fb347f883")]
        #endregion
        bool I12AllorsBoolean { get; set; }

        #region Allors
        [Id("249ff221-9261-4219-b0a8-0dc2a8dac8db")]
        #endregion
        int I12AllorsInteger { get; set; }

        #region Allors
        [Id("2c05b90e-a036-450a-8b4e-ee70c8146fed")]
        [Multiplicity(Multiplicity.OneToMany)]
        #endregion
        I34[] I12I34one2manies { get; set; }

        #region Allors
        [Id("3327e14d-5601-4806-b6c5-b740a2c3aa38")]
        [Multiplicity(Multiplicity.ManyToOne)]
        #endregion
        C3 C3many2one { get; set; }

        #region Allors
        [Id("3589d5bc-3338-449a-bd14-34a19d92251e")]
        [Multiplicity(Multiplicity.ManyToOne)]
        #endregion
        C2 I12C2many2one { get; set; }

        #region Allors
        [Id("4c7dd6a2-db16-4477-9b21-34dcb8f50738")]
        #endregion
        double I12AllorsDouble { get; set; }

        #region Allors
        [Id("61fc731f-d769-4eb9-bf87-983e73e403e4")]
        [Multiplicity(Multiplicity.ManyToOne)]
        #endregion
        I34 I12I34many2one { get; set; }

        #region Allors
        [Id("716d13fc-f608-41a8-ac9e-824890c585b5")]
        [Multiplicity(Multiplicity.ManyToMany)]
        #endregion
        I34[] I12I34many2manies { get; set; }

        #region Allors
        [Id("74a22498-ec2c-441b-a42c-0c248ace685d")]
        [Multiplicity(Multiplicity.OneToOne)]
        #endregion
        C3 I12C3one2one { get; set; }

        #region Allors
        [Id("7f373030-657a-4c6b-a086-ac4de33e4648")]
        [Multiplicity(Multiplicity.ManyToMany)]
        #endregion
        C2[] I12C2many2manies { get; set; }

        #region Allors
        [Id("9fbca845-1f98-4ac8-8117-fa66bbe287eb")]
        [Precision(19)]
        [Scale(2)]
        #endregion
        decimal I12AllorsDecimal { get; set; }

        #region Allors
        [Id("afabb84c-f1b3-423b-9028-2ec5bb58e994")]
        [Multiplicity(Multiplicity.OneToOne)]
        #endregion
        C2 I12C2one2one { get; set; }

        #region Allors
        [Id("b0fc73fb-fa74-4e8c-b9e1-17c01698f342")]
        [Multiplicity(Multiplicity.OneToMany)]
        #endregion
        C3[] I12C3one2manies { get; set; }

        #region Allors
        [Id("b889bc75-3d93-4577-a4d7-752393284220")]
        [Multiplicity(Multiplicity.ManyToMany)]
        #endregion
        C3[] I12C3many2manies { get; set; }

        #region Allors
        [Id("c2d1f044-b996-4b16-8fe3-1786f86973b1")]
        [Size(256)]
        #endregion
        string PrefetchTest { get; set; }

        #region Allors
        [Id("c3a2e1da-307c-4fad-ab34-6e9d07eea74f")]
        #endregion
        DateTime I12AllorsDateTime { get; set; }

        #region Allors
        [Id("e227ff6c-a4df-49cf-a02f-04e94af6eb4b")]
        [Size(256)]
        #endregion
        string I12AllorsString { get; set; }

        #region Allors
        [Id("f31ace17-76b1-46db-9fc0-099b94fbada5")]
        [Multiplicity(Multiplicity.OneToOne)]
        #endregion
        I34 I12I34one2one { get; set; }

        #region Allors
        [Id("f37b107e-74e5-401f-a7e8-8ac54ceb6c73")]
        [Multiplicity(Multiplicity.OneToMany)]
        #endregion
        C2[] I12C2one2manies { get; set; }
    }
}
