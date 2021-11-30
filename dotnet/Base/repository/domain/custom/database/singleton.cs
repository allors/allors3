// <copyright file="Singleton.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    public partial class Singleton : Object
    {
        #region Allors
        [Id("AD981D2E-32E8-4DC6-91A2-F8A2F44086F3")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        [Indexed]
        public Person AutocompleteDefault { get; set; }
    }
}
