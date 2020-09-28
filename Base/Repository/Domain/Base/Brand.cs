// <copyright file="Brand.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;
    using static Workspaces;
    using static Workspaces;

    #region Allors
    [Id("0a7ac589-946b-4d49-b7e0-7e0b9bc90111")]
    #endregion
    public partial class Brand : Deletable, Object
    {
        #region inherited properties

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("2A45A70B-ECF0-441E-AD89-52FC123BC79E")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Media LogoImage { get; set; }

        #region Allors
        [Id("C603F7EA-5201-464A-B657-BE23D42EF6DB")]
        #endregion
        [Required]
        [Workspace(Default)]
        public string Name { get; set; }

        #region Allors
        [Id("852D8EF7-8ABD-4125-84E3-84DCF96014AC")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        [MediaType("text/markdown")]
        public string Description { get; set; }

        #region Allors
        [Id("7AB21625-164A-4686-A59E-5D64013EE9CC")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        public LocalisedText[] LocalisedDescriptions { get; set; }

        #region Allors
        [Id("0DA86868-CD0A-4370-BD47-34790A22860F")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public Model[] Models { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        public void Delete() { }

        #endregion
    }
}
