// <copyright file="PerformanceNote.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("4629c7ed-e9a4-4f31-bb46-e3f2920bd768")]
    #endregion
    public partial class PerformanceNote : Commentable, Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public string Comment { get; set; }

        public LocalisedText[] LocalisedComments { get; set; }

        #endregion

        #region Allors
        [Id("1b8f0ada-bb5c-4226-8e35-5f1c40b06fc8")]
        #endregion
        [Required]
        [Size(-1)]

        public string Description { get; set; }

        #region Allors
        [Id("2f6ed687-4200-4a27-bfb2-922d9ce2e38f")]
        #endregion

        public DateTime CommunicationDate { get; set; }

        #region Allors
        [Id("5bf234d2-8486-47b2-a770-eca36b44bb67")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public Person GivenByManager { get; set; }

        #region Allors
        [Id("a8cd7bf6-6bea-44ad-9e89-1bd63ffca459")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public Person Employee { get; set; }

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
