// <copyright file="EventRegistration.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("2b5efcb9-54ba-4d59-833b-716d321cc7cb")]
    #endregion
    public partial class EventRegistration : Object
    {
        #region inherited properties
        #endregion

        #region Allors
        [Id("af4b8828-bea1-43e5-b109-9934311cc2df")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public Person Person { get; set; }

        #region Allors
        [Id("ed542026-7020-43e3-ab72-c3f4dd991a4b")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public Event Event { get; set; }

        #region Allors
        [Id("f9805362-2bd2-46d4-b9b2-d38cd0a76f78")]
        #endregion

        public DateTime AllorsDateTime { get; set; }

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

        #endregion

    }
}
