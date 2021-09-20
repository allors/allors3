// <copyright file="AccessClass.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("dcdc800a-e918-4e44-b00a-405bd855333c")]
    #endregion
    [Workspace(Default)]
    [Origin(Origin.Session)]
    public partial class SessionPerson
    {
        #region Allors
        [Id("d1482ad8-cdc0-4017-a435-274b8767bb7e")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public string FirstName { get; set; }

        #region Allors
        [Id("d5f8e87b-1f30-4501-8706-e85693d4d660")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public string LastName { get; set; }

        #region Allors
        [Id("b3cbf945-cd44-4889-92b5-b759b1538eb2")]
        #endregion
        [Derived]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        public string FullName { get; set; }
    }
}
