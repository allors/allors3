// <copyright file="BudgetItem.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;

    #region Allors
    [Id("b397c075-215a-4d5b-b962-ea48540a64fa")]
    #endregion
    public partial class BudgetItem : DelegatedAccessControlledObject
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("24645d36-9f98-4d08-a7e0-51c1132a110d")]
        #endregion
        [Required]
        [Size(256)]

        public string Purpose { get; set; }

        #region Allors
        [Id("6b313789-9a6d-47ca-adad-def39af1e11f")]
        #endregion
        [Size(256)]

        public string Justification { get; set; }

        #region Allors
        [Id("a4e584cc-7cf6-4590-83e4-a827a7a06624")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]

        public BudgetItem[] Children { get; set; }

        #region Allors
        [Id("cced2368-6a7d-4aea-8112-57dead05f7b4")]
        #endregion
        [Required]
        [Precision(19)]
        [Scale(2)]
        public decimal Amount { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        public void DelegateAccess() { }

        #endregion
    }
}
