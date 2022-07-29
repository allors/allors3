// <copyright file="UnitSample.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("4e501cd6-807c-4f10-b60b-acd1d80042cd")]
    #endregion
    [Workspace(Default)]
    public partial class UnitSample : Object
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("24771d5b-f920-4820-aff7-ea6391b4a45c")]
        #endregion
        [Workspace(Default)]
        public byte[] AllorsBinary { get; set; }

        #region Allors
        [Id("4d6a80f5-0fa7-4867-91f8-37aa92b6707b")]
        #endregion
        [Workspace(Default)]
        public DateTime AllorsDateTime { get; set; }

        #region Allors
        [Id("5a788ebe-65e9-4d5e-853a-91bb4addabb5")]
        #endregion
        [Workspace(Default)]
        public bool AllorsBoolean { get; set; }

        #region Allors
        [Id("74a35820-ef8c-4373-9447-6215ee8279c0")]
        #endregion
        [Workspace(Default)]
        public double AllorsDouble { get; set; }

        #region Allors
        [Id("b817ba76-876e-44ea-8e5a-51d552d4045e")]
        #endregion
        [Workspace(Default)]
        public int AllorsInteger { get; set; }

        #region Allors
        [Id("c724c733-972a-411c-aecb-e865c2628a90")]
        #endregion
        [Workspace(Default)]
        [Size(256)]
        public string AllorsString { get; set; }

        #region Allors
        [Id("ed58ae4c-24e0-4dd1-8b1c-0909df1e0fcd")]
        #endregion
        [Workspace(Default)]
        public Guid AllorsUnique { get; set; }

        #region Allors
        [Id("f746da51-ea2d-4e22-9ecb-46d4dbc1b084")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal AllorsDecimal { get; set; }

        #region Allors
        [Id("6E05C521-B90A-459E-931A-940B4D769C6A")]
        #endregion
        [Required]
        public byte[] RequiredBinary { get; set; }

        #region Allors
        [Id("0A17B766-9A60-4061-8FCB-AADFC6C13FAF")]
        #endregion
        [Required]
        public DateTime RequiredDateTime { get; set; }

        #region Allors
        [Id("22BEF3E8-1178-4717-9BD1-D6F34569B63C")]
        #endregion
        [Required]
        public bool RequiredBoolean { get; set; }

        #region Allors
        [Id("FAC655F6-6D89-4CE5-B8E9-388F35294DD0")]
        #endregion
        [Required]
        public double RequiredDouble { get; set; }

        #region Allors
        [Id("3257637E-CE68-49B8-879C-E428810DD316")]
        #endregion
        [Required]
        public int RequiredInteger { get; set; }

        #region Allors
        [Id("38405B1B-8469-47D9-BDDF-66B753F52A52")]
        #endregion
        [Required]
        public string RequiredString { get; set; }

        #region Allors
        [Id("336175A6-29FE-4A6A-A21E-5F3B97BFF99D")]
        #endregion
        [Required]
        public Guid RequiredUnique { get; set; }

        #region Allors
        [Id("A5905304-6BB6-4B15-85F7-8C4225D6E6B9")]
        #endregion
        [Required]
        public decimal RequiredDecimal { get; set; }

        #region inherited methods
        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        #endregion
    }
}
