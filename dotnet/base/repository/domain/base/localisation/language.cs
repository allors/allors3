// <copyright file="Language.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;


    #region Allors
    [Id("4a0eca4b-281f-488d-9c7e-497de882c044")]
    #endregion
    [Workspace(Default)]
    public partial class Language : Object
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("d2a32d9f-21cc-4f9d-b0d3-a9b75da66907")]
        #endregion
        [Required]
        [Size(256)]
        [Workspace(Default)]
        public string IsoCode { get; set; }

        #region Allors
        [Id("be482902-beb5-4a76-8ad0-c1b1c1c0e5c4")]
        #endregion
        [Indexed]
        [Required]
        [Size(256)]
        [Workspace(Default)]
        public string Name { get; set; }

        #region Allors
        [Id("f091b264-e6b1-4a57-bbfb-8225cbe8190c")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        public LocalisedText[] LocalisedNames { get; set; }

        #region Allors
        [Id("842CC899-3F37-455A-AE91-51D29D615E69")]
        #endregion
        [Indexed]
        [Required]
        // [Unique] If Unique is enabled then make sure your database supports the range of unicode characters (e.g. use collation 'Latin1_General_100_CI_AS_SC' in sql server)
        [Size(256)]
        [Workspace(Default)]
        public string NativeName { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        #endregion
    }
}
