// <copyright file="AccessClass.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("97758A8A-CA98-4119-B354-386643075B19")]
    #endregion
    [Workspace(Default)]
    [Origin(Origin.Workspace)]
    public partial class WorkspacePerson
    {
        #region Allors
        [Id("137E321E-537E-44B7-AEEB-3D1F772AB8D9")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public string FirstName { get; set; }

        #region Allors
        [Id("05B2117E-B714-4A2A-A793-8BC7D2A2EF33")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Workspace)]
        public string LastName { get; set; }

        #region Allors
        [Id("95429c41-f951-4f38-a15f-ca0e8777fae9")]
        #endregion
        [Derived]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public string FullName { get; set; }
    }
}
