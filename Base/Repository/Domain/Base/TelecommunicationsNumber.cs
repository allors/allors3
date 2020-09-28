// <copyright file="TelecommunicationsNumber.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("6c255f71-ce18-4d51-b0d9-e402ec0e570e")]
    #endregion
    public partial class TelecommunicationsNumber : ContactMechanism
    {
        #region inherited properties
        public string Description { get; set; }

        public ContactMechanism[] FollowTo { get; set; }

        public ContactMechanismType ContactMechanismType { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public User CreatedBy { get; set; }

        public User LastModifiedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        #endregion

        #region Allors
        [Id("31ccabaf-1d31-4b35-93a4-8c18c813c3cd")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string CountryCode { get; set; }

        #region Allors
        [Id("2eabf6bb-48f9-431a-b05b-b892c88db821")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string AreaCode { get; set; }

        #region Allors
        [Id("9b587eba-53ee-417c-8324-5c19ec90b745")]
        #endregion
        [Required]
        [Size(40)]
        [Workspace(Default)]
        public string ContactNumber { get; set; }

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
        #endregion

    }
}
