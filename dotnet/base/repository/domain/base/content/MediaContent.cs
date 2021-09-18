// <copyright file="MediaContent.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;


    #region Allors
    [Id("6c20422e-cb3e-4402-bb40-dacaf584405e")]
    #endregion
    [Workspace(Default)]
    public partial class MediaContent : Deletable, Object
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }
        #endregion

        #region Allors
        [Id("890598a9-0be4-49ee-8dd8-3581ee9355e6")]
        #endregion
        [Required]
        [Indexed]
        [Size(1024)]
        [Workspace(Default)]
        public string Type { get; set; }

        #region Allors
        [Id("0756d508-44b7-405e-bf92-bc09e5702e63")]
        #endregion
        [Required]
        [Size(-1)]
        [Workspace(Default)]
        public byte[] Data { get; set; }

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
