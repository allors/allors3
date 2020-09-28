// <copyright file="EmploymentApplication.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("6940c300-47e6-44f2-b93b-d70bed9de602")]
    #endregion
    public partial class EmploymentApplication : Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("528de310-3268-4b17-ab42-49dea27d5aee")]
        #endregion
        [Required]

        public DateTime ApplicationDate { get; set; }

        #region Allors
        [Id("75cc1a7c-6bf7-4798-9ddc-fd1b283aed19")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public Position Position { get; set; }

        #region Allors
        [Id("7d3147e2-9709-42bc-a6cd-5b922bfc143d")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public EmploymentApplicationStatus EmploymentApplicationStatus { get; set; }

        #region Allors
        [Id("a4c14261-14a2-404c-814f-6475368d685a")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public Person Person { get; set; }

        #region Allors
        [Id("b0799b22-bff3-49d7-8f9a-3ea41c540778")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public EmploymentApplicationSource EmploymentApplicationSource { get; set; }

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
