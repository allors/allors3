// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Allors.Meta;
    using Derivations;
    using Resources;

    public class BankAccountDerivation : DomainDerivation
    {
        public BankAccountDerivation(M m) : base(m, new Guid("633f58cd-ca1b-4a2e-8f6e-e1642466a9f7")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.OwnBankAccount.BankAccount) { Steps =  new IPropertyType[] { m.OwnBankAccount.BankAccount} },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<BankAccount>())
            {

                if (@this.ExistOwnBankAccountsWhereBankAccount)
                {
                    validation.AssertExists(@this, @this.Meta.Bank);
                    validation.AssertExists(@this, @this.Meta.Currency);
                    validation.AssertExists(@this, @this.Meta.NameOnAccount);
                }

                DeriveIban(validation, @this);
            }

            void DeriveIban(IDomainValidation derivation, BankAccount bankAccount)
            {
                if (!string.IsNullOrEmpty(bankAccount.Iban))
                {
                    var iban = Regex.Replace(bankAccount.Iban, @"\s", string.Empty).ToUpper(); // remove empty space & convert all uppercase

                    if (Regex.IsMatch(iban, @"\W"))
                    {
                        // contains chars other than (a-zA-Z0-9)
                        validation.AddError($"{bankAccount}, {bankAccount.Meta.Iban}, {ErrorMessages.IbanIllegalCharacters}");
                    }

                    if (!Regex.IsMatch(iban, @"^\D\D\d\d.+"))
                    {
                        // first chars are letter letter digit digit
                        validation.AddError($"{bankAccount}, {bankAccount.Meta.Iban}, {ErrorMessages.IbanStructuralFailure}");
                    }

                    if (Regex.IsMatch(iban, @"^\D\D00.+|^\D\D01.+|^\D\D99.+"))
                    {
                        // check digit are 00 or 01 or 99
                        validation.AddError($"{bankAccount}, {bankAccount.Meta.Iban}, {ErrorMessages.IbanCheckDigitsError}");
                    }

                    var country = new Countries(bankAccount.Strategy.Session).FindBy(this.M.Country.IsoCode, bankAccount.Iban.Substring(0, 2));

                    if (country == null || !country.ExistIbanRegex || !country.ExistIbanLength)
                    {
                        validation.AddError($"{bankAccount}, {bankAccount.Meta.Iban}, {ErrorMessages.IbanValidationUnavailable}");
                    }

                    if (country != null && country.ExistIbanRegex && country.ExistIbanLength)
                    {
                        if (iban.Length != country.IbanLength)
                        {
                            // fits length to country
                            validation.AddError($"{bankAccount}, {bankAccount.Meta.Iban}, {ErrorMessages.IbanLengthFailure}");
                        }

                        if (!Regex.IsMatch(iban.Remove(0, 4), country.IbanRegex))
                        {
                            // check country specific structure
                            validation.AddError($"{bankAccount}, {bankAccount.Meta.Iban}, {ErrorMessages.IbanStructuralFailure}");
                        }
                    }

                    //if (!validation.HasErrors)
                    //{
                    //    // ******* from wikipedia.org
                    //    // The checksum is a basic ISO 7064 mod 97-10 calculation where the remainder must equal 1.
                    //    // To validate the checksum:
                    //    // 1- Check that the total IBAN length is correct as per the country. If not, the IBAN is invalid.
                    //    // 2- Move the four initial characters to the end of the string.
                    //    // 3- Replace each letter in the string with two digits, thereby expanding the string, where A=10, B=11, ..., Z=35.
                    //    // 4- Interpret the string as a decimal integer and compute the remainder of that number on division by 97.
                    //    // The IBAN number can only be valid if the remainder is 1.
                    //    var modifiedIban = iban.ToUpper().Substring(4) + iban.Substring(0, 4);
                    //    modifiedIban = Regex.Replace(modifiedIban, @"\D", m => (m.Value[0] - 55).ToString(CultureInfo.InvariantCulture));

                    //    var remainer = 0;
                    //    while (modifiedIban.Length >= 7)
                    //    {
                    //        remainer = int.Parse(remainer + modifiedIban.Substring(0, 7)) % 97;
                    //        modifiedIban = modifiedIban.Substring(7);
                    //    }

                    //    remainer = int.Parse(remainer + modifiedIban) % 97;

                    //    if (remainer != 1)
                    //    {
                    //        validation.AddError($"{bankAccount}, {bankAccount.Meta.Iban}, {ErrorMessages.IbanIncorrect}");
                    //    }
                    //}
                }
            }
        }
    }
}
