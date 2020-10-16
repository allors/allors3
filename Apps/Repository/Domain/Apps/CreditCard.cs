// <copyright file="CreditCard.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;

    #region Allors
    [Id("9492bd39-0f07-4978-a987-0393ca34b504")]
    #endregion
    public partial class CreditCard : FinancialAccount, Object
    {
        #region inherited properties
        public FinancialAccountTransaction[] FinancialAccountTransactions { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("07d663c5-4716-4e76-a280-ec635216791f")]
        #endregion
        [Required]
        [Size(256)]

        public string NameOnCard { get; set; }

        #region Allors
        [Id("0916d4d2-5f82-46da-967e-7b48012e4019")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public CreditCardCompany CreditCardCompany { get; set; }

        #region Allors
        [Id("4dfa0fda-0001-4635-b8d1-4fd4ce723ed2")]
        #endregion
        [Required]

        public int ExpirationYear { get; set; }

        #region Allors
        [Id("7fa0d04e-b2df-49f8-8aa2-2d546ca843d6")]
        #endregion
        [Required]

        public int ExpirationMonth { get; set; }

        #region Allors
        [Id("b5484c11-52d4-45f7-b25a-bf4c05e2c9a0")]
        #endregion
        [Required]
        [Unique]
        [Size(256)]

        public string CardNumber { get; set; }

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
