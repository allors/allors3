// <copyright file="RequirementCommunication.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("49cdc4a2-f7af-43c9-b160-4c7da9a0ca42")]
    #endregion
    public partial class RequirementCommunication : Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("5a4d9541-4a8a-4661-bec3-e65db5298857")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public CommunicationEvent CommunicationEvent { get; set; }

        #region Allors
        [Id("b65140b1-8dc4-4836-9ad8-fe01f43dad7a")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public Requirement Requirement { get; set; }

        #region Allors
        [Id("cdb72b3f-9920-4082-83a7-a0211a29cf77")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public Person AssociatedProfessional { get; set; }

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
