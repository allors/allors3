// <copyright file="BankAccount.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("22bc5b67-8015-49c5-bc47-6f9e7e678943")]
    #endregion
    public partial class BankAccount : FinancialAccount
    {
        #region inherited properties
        public FinancialAccountTransaction[] FinancialAccountTransactions { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("52677328-d903-4e97-83c1-b55668ced66d")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        public Bank Bank { get; set; }

        #region Allors
        [Id("53bb9d62-a8e5-417c-9392-c54cf99bc24b")]
        #endregion
        [Size(256)]
        public string NameOnAccount { get; set; }

        #region Allors
        [Id("a7d242b4-4d39-4254-beb2-914eb556f7b7")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        public Currency Currency { get; set; }

        #region Allors
        [Id("ac2d58e5-ad74-4afe-b9f0-aeb9dfdcd4b3")]
        #endregion
        [Required]
        [Unique]
        [Size(256)]
        public string Iban { get; set; }

        #region Allors
        [Id("b06a858d-a8ee-41b8-a747-7fd46336ae4f")]
        #endregion
        [Required]
        [Unique]
        [Size(256)]
        public string Description { get; set; }

        #region Allors
        [Id("ecaedf71-98a2-425d-8046-cc8865fdbe73")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        public Person[] ContactPersons { get; set; }

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
