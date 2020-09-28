// <copyright file="ElectronicAddress.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;
    using static Workspaces;
    using static Workspaces;

    #region Allors
    [Id("5cd86f69-e09b-4150-a2a6-2eed4c72b426")]
    #endregion
    public partial interface ElectronicAddress : ContactMechanism
    {
        #region Allors
        [Id("90288ea6-cb3b-47ad-9bb1-aa71d7c65926")]
        #endregion
        [Required]
        [Size(256)]
        [Workspace(Default)]
        string ElectronicAddressString { get; set; }
    }
}
