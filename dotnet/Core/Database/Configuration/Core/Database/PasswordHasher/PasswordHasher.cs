// <copyright file="PasswordHasher.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using System.Linq;
    using Domain;
    using Microsoft.AspNetCore.Identity;

    public class PasswordHasher : IPasswordHasher
    {
        // The same bar as the customer portal's Identity policy (RequiredLength 12,
        // RequiredUniqueChars 4, no composition rules): length over composition, per
        // NIST 800-63B / OWASP ASVS. Replaces the 8-character regex set whose special-character
        // class [!@#$%^&*?_~-£() ] had an unescaped ~-£ range and so accepted ¡¢£¥©®° and a
        // plain space as "special".
        private const int MinimumLength = 12;
        private const int MinimumUniqueCharacters = 4;

        private readonly PasswordHasher<string> passwordHasher;

        public PasswordHasher() => this.passwordHasher = new PasswordHasher<string>();

        public string HashPassword(string user, string password) => this.passwordHasher.HashPassword(user, password);

        public bool VerifyHashedPassword(string user, string hashedPassword, string providedPassword)
        {
            var result = this.passwordHasher.VerifyHashedPassword(user, hashedPassword, providedPassword);
            return result != PasswordVerificationResult.Failed;
        }

        public bool CheckStrength(string password) =>
            password != null
            && password.Length >= MinimumLength
            && password.Distinct().Count() >= MinimumUniqueCharacters;

        public void Dispose()
        {
        }
    }
}
