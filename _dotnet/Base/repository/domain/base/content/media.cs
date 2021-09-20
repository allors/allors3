// <copyright file="Media.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;


    #region Allors
    [Id("da5b86a3-4f33-4c0d-965d-f4fbc1179374")]
    #endregion
    [Workspace(Default)]
    public partial class Media : UniquelyIdentifiable, Deletable, Object
    {
        #region inherited properties
        public Guid UniqueId { get; set; }

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("B74C2159-739A-4F1C-ADA7-C2DCC3CDCF83")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace(Default)]
        public Guid Revision { get; set; }

        #region Allors
        [Id("67082a51-1502-490b-b8db-537799e550bd")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public MediaContent MediaContent { get; set; }

        #region Allors
        [Id("DCBF8D02-84A5-4C1B-B2C1-16D6E97EEA06")]
        #endregion
        [Indexed]
        [Size(1024)]
        [Workspace(Default)]
        public string InType { get; set; }

        #region Allors
        [Id("18236718-1835-430C-A936-7EC461EEE2CF")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        public byte[] InData { get; set; }

        #region Allors
        [Id("79B04065-F13B-43B3-B86E-F3ADBBAAF0C4")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        public string InDataUri { get; set; }

        #region Allors
        [Id("E03239E9-2039-49DC-9615-36CEA3C971D3")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string InFileName { get; set; }

        #region Allors
        [Id("DDD6C005-0104-44CA-A19C-1150B8BEB4A3")]
        #endregion
        [Indexed]
        [Size(256)]
        [Workspace(Default)]
        public string Name { get; set; }

        #region Allors
        [Id("29541613-0B16-49AD-8F40-3309A7C7D7B8")]
        #endregion
        [Indexed]
        [Size(1024)]
        [Workspace(Default)]
        [Derived]
        public string Type { get; set; }

        #region Allors
        [Id("AC462C32-3945-4C39-BAEF-9D228EEA80A6")]
        #endregion
        [Indexed]
        [Size(256)]
        [Derived]
        [Workspace(Default)]
        public string FileName { get; set; }

        #region inherited methods
        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        public void Delete() { }
        #endregion
    }
}
