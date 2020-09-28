// <copyright file="OrderRequirementCommitment.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("2fcdaf95-c3ec-4da2-8e7e-09c55741082f")]
    #endregion
    public partial class OrderRequirementCommitment : Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("a03b08be-82d9-4678-803a-0463c658d4c4")]
        #endregion
        [Required]

        public int Quantity { get; set; }

        #region Allors
        [Id("a9020377-d721-4329-868d-33ab63aed074")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public OrderItem OrderItem { get; set; }

        #region Allors
        [Id("e36224d2-cc6f-43b0-82e1-e300710f6407")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public Requirement Requirement { get; set; }

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
