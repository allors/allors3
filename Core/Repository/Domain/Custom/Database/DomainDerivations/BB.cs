// <copyright file="C1.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("93CDFDD3-B3CE-424D-96A5-6BF9DCB84CF9")]
    #endregion
    public partial class BB : Object
    {
        #region inherited properties
   
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("023301F0-E80E-4AAD-A3B0-7C0DFDEC688C")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        public CC One2One { get; set; }

        #region Allors
        [Id("C14C0CBA-3046-48A5-AAD8-9867352CD5F3")]
        #endregion
        [Size(256)]
        public string Name { get; set; }

        #region Allors
        [Id("FAF8CCED-1967-4A5D-86AB-61D85312E34A")]
        #endregion
        [Size(256)]
        public string Derived { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit() { }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        #endregion
    }
}
