// <copyright file="PersistentPreparedExtent.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("645A4F92-F1F1-41C7-BA76-53A1CC4D2A61")]
    #endregion
    public partial class PersistentPreparedExtent : UniquelyIdentifiable, Deletable
    {
        #region inherited properties

        public Guid UniqueId { get; set; }

        #endregion

        #region Allors
        [Id("CEADE44E-AA67-4E77-83FC-2C6E141A89F6")]
        #endregion
        [Size(256)]
        public string Name { get; set; }

        #region Allors
        [Id("03B7FB15-970F-453D-B6AC-A50654775E5E")]
        #endregion
        [Size(-1)]
        public string Description { get; set; }

        #region Allors
        [Id("712367B5-85ED-4623-9AC9-C082A32D8889")]
        #endregion
        [Size(-1)]
        public string Content { get; set; }

        #region inherited methods

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

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

        public void Delete()
        {
        }

        #endregion
    }
}
