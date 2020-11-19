// <copyright file="S12.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Attributes;

    #region Allors
    [Id("c5747a64-f468-4d0d-80f3-6463bd32b0ca")]
    #endregion
    public partial interface S12 : Object
    {
        #region Allors
        [Id("06fabe71-737a-4cff-ac10-2d15dafce503")]
        [Size(256)]
        #endregion
        string S12AllorsString { get; set; }

        #region Allors
        [Id("2eb9e232-4ed4-4997-a21a-f11bb0fe3b0e")]
        #endregion
        DateTime S12AllorsDateTime { get; set; }

        #region Allors
        [Id("39f50108-df59-455d-8371-fc07f3dbb7ef")]
        [Multiplicity(Multiplicity.ManyToMany)]
        #endregion
        C2[] S12C2many2manies { get; set; }

        #region Allors
        [Id("61e8c425-407e-408b-9f2e-c95548833004")]
        [Multiplicity(Multiplicity.ManyToOne)]
        #endregion
        C2 S12C2many2one { get; set; }

        #region Allors
        [Id("830117d4-fbe1-4944-bacf-54331e8451d7")]
        [Multiplicity(Multiplicity.OneToOne)]
        #endregion
        C2 S12C2one2one { get; set; }

        #region Allors
        [Id("a3aac482-aad0-4b59-9361-51b23867e5a2")]
        [Multiplicity(Multiplicity.OneToMany)]
        #endregion
        C2[] S12C2one2manies { get; set; }

        #region Allors
        [Id("a97eca8e-807b-4a06-9587-6240f6150203")]
        #endregion
        bool S12AllorsBoolean { get; set; }

        #region Allors
        [Id("acc4ae39-2d5c-4485-be22-87b27e84b627")]
        #endregion
        double S12AllorsDouble { get; set; }

        #region Allors
        [Id("d07313ca-fd8d-4c74-928e-41274aa28de9")]
        #endregion
        int S12AllorsInteger { get; set; }

        #region Allors
        [Id("f7ace363-89bd-4ea5-a865-4a6e3de2d723")]
        [Precision(19)]
        [Scale(2)]
        #endregion
        decimal S12AllorsDecimal { get; set; }
    }
}
