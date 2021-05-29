// <copyright file="RgsFilter.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;

    #region Allors

    [Id("e9fc4ce6-0a35-4683-a55e-232e05f7fbfa")]

    #endregion
    public partial class RgsFilter : Object
    {
        #region inherited properties

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region inherited methods

        public void OnBuild()
        {
        }

        public void OnPostBuild()
        {
        }

        public void OnInit()
        {
        }

        public void OnPostDerive()
        {
        }

        #endregion

        #region Allors
        [Id("673e428f-4e94-44a5-81a6-ac07362dce6b")]
        #endregion
        [Workspace]
        public string Name { get; set; }

        #region Allors
        [Id("5602f25b-56ba-472e-a16a-43eeeb55cceb")]
        #endregion
        [Workspace]
        public string Description { get; set; }
    }
}
