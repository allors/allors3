// <copyright file="PositionReportingStructure.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("b50d0780-bcbf-4041-8576-164577d40c55")]
    #endregion
    public partial class PositionReportingStructure : Commentable, Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public string Comment { get; set; }

        public LocalisedText[] LocalisedComments { get; set; }

        #endregion

        #region Allors
        [Id("23b91508-508f-4afe-8259-a17f16381833")]
        #endregion

        public bool Primary { get; set; }

        #region Allors
        [Id("5fbc72bf-2153-4b91-83f9-6fd057e4b1d6")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public Position ManagedByPosition { get; set; }

        #region Allors
        [Id("e2e60d09-ebfa-4bf3-94e9-759279b00919")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public Position Position { get; set; }

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
