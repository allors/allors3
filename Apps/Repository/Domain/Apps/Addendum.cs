// <copyright file="Addendum.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;

    #region Allors
    [Id("7baa7594-6890-4e1e-8c06-fc49b3ea262d")]
    #endregion
    public partial class Addendum : Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("2aaa6623-6f1a-4b40-91f0-4014108549d6")]
        #endregion
        [Size(-1)]

        public string Text { get; set; }

        #region Allors
        [Id("30b99ed6-cb44-4401-b5bd-76c0099153d4")]
        #endregion

        public DateTime EffictiveDate { get; set; }

        #region Allors
        [Id("45a9d28e-f131-44a8-aea5-1a9776be709e")]
        #endregion
        [Required]
        [Size(-1)]

        public string Description { get; set; }

        #region Allors
        [Id("f14af73d-8d7d-4c5b-bc6a-957830fd0a80")]
        #endregion
        [Derived]
        [Required]

        public DateTime CreationDate { get; set; }

        #region inherited methods

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
