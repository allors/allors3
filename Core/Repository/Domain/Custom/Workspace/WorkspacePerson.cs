// <copyright file="AccessClass.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;

    #region Allors
    [Id("97758A8A-CA98-4119-B354-386643075B19")]
    #endregion
    [Workspace("Default")]
    [Origin(Origin.Local)]
    public partial class WorkspacePerson 
    {
        #region Allors
        [Id("137E321E-537E-44B7-AEEB-3D1F772AB8D9")]
        [AssociationId("F9A069CD-F8F9-4A57-926D-7880D42631BE")]
        [RoleId("BE6087FE-F7F8-42E1-A22F-3989F7BBBBE9")]
        #endregion
        [Workspace]
        [Origin(Origin.Local)]
        public string FirstName { get; set; }

        #region Allors
        [Id("05B2117E-B714-4A2A-A793-8BC7D2A2EF33")]
        [AssociationId("5FBF966A-97FD-4739-B2A4-178C1853A207")]
        [RoleId("B22E7AC0-22D1-4B50-8937-28739AB5DEEC")]
        #endregion
        [Workspace]
        [Origin(Origin.Local)]
        public string LastName { get; set; }
    }
}
