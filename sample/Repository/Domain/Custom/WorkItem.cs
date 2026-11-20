// <copyright file="WorkItem.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("8e78ae27-8c39-4bb9-89c5-e5b4f27507c1")]
    #endregion
    [Workspace(Default)]
    public partial class WorkItem : Object
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }
        #endregion

        #region Allors
        [Id("6992a861-6b69-4715-973a-366273674bfa")]
        #endregion
        [Required]
        [Size(256)]
        [Workspace(Default)]
        public string Title { get; set; }

        #region Allors
        [Id("e97e247c-e639-4ada-8d6d-05bec4a24b42")]
        #endregion
        [Indexed]
        [Workspace(Default)]
        public Person Assignee { get; set; }

        #region Allors
        [Id("1c104242-3e18-4898-9ce7-1dea2374f2e9")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool Done { get; set; }

        #region inherited methods
        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit() { }

        public void OnPostDerive() { }
        #endregion
    }
}
