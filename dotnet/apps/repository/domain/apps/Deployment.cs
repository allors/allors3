// <copyright file="Deployment.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("ee23df25-f7d7-4974-b62e-ee3cba56b709")]
    #endregion
    public partial class Deployment : Period, Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ThroughDate { get; set; }

        #endregion

        #region Allors
        [Id("212653db-1677-47bd-944c-b5468673ec63")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public Good ProductOffering { get; set; }

        #region Allors
        [Id("c322fbbd-3406-4e73-83ed-033282ab0cfb")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public DeploymentUsage DeploymentUsage { get; set; }

        #region Allors
        [Id("d588ba7f-7b67-43fd-bb67-b9ff82fcffaf")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public SerialisedInventoryItem SerialisedInventoryItem { get; set; }

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
