// <copyright file="Login.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("F4FD1CB2-9E98-4F58-AAD4-2388916C2E56")]
    #endregion
    public partial class IdentityClaim : Deletable
    {
        #region inherited properties
        #endregion

        #region Allors
        [Id("7B1AD6D4-EF6F-40FE-90BD-89DE8A997388")]
        #endregion
        [Indexed]
        [Size(256)]
        public string Type { get; set; }

        #region Allors
        [Id("34793275-CD8F-4A4E-9E79-B82F3869ACCF")]
        #endregion
        [Indexed]
        [Size(256)]
        public string Value { get; set; }

        #region Allors
        [Id("96792928-C286-4F47-B4BE-2E92C5E3E993")]
        #endregion
        [Indexed]
        [Size(256)]
        public string ValueType { get; set; }

        #region Allors
        [Id("505088BD-A9A9-433A-9585-2FB08EBA212B")]
        #endregion
        [Indexed]
        [Size(256)]
        public string Issuer { get; set; }

        #region Allors
        [Id("94E59DDC-62DD-4EBB-97C0-D5D441C28CE0")]
        #endregion
        [Indexed]
        [Size(256)]
        public string OriginalIssuer { get; set; }

        #region inherited methods

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        public void Delete() { }
        #endregion
    }
}
