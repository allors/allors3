// <copyright file="SerialisedItemCharacteristic.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("A34F6226-7837-4905-9125-61CD00A07BF4")]
    #endregion
    public partial class SerialisedItemCharacteristic : Deletable, Object
    {
        #region inherited properties

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("73A04D99-CD9F-41F7-AA1C-B4CD80AF60AD")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace(Default)]
        public SerialisedItemCharacteristicType SerialisedItemCharacteristicType { get; set; }

        #region Allors
        [Id("E68E6931-F10C-4F04-A23E-B2BC82AC6D5C")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Size(-1)]
        [Workspace(Default)]
        public string Value { get; set; }

        #region Allors
        [Id("EE9688B5-B93C-4911-914D-E76E4E4825B0")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public LocalisedText[] LocalisedValues { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        public void Delete() { }

        #endregion
    }
}
