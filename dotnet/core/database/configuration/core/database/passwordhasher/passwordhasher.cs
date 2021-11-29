// <copyright file="PasswordHasher.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Domain;
    using Microsoft.AspNetCore.Identity;

    public class PasswordHasher : IPasswordHasher
    {
        private readonly PasswordHasher<string> passwordHasher;
        private readonly IEnumerable<Regex> rules;

        public PasswordHasher()
        {
            this.passwordHasher = new PasswordHasher<string>();
            this.rules = new[]
              {
                    ".{8,}",                        // eight or more characters long
                    "\\d",                          // contains numbers
                    "[a-z].*?[A-Z]|[A-Z].*?[a-z]",  // mixed case
                    "[!@#$%^&*?_~-£() ]"            // special characters
                }
              .Select(rule => new Regex(rule));
        }

        public string HashPassword(string user, string password) => this.passwordHasher.HashPassword(user, password);

        public bool VerifyHashedPassword(string user, string hashedPassword, string providedPassword)
        {
            var result = this.passwordHasher.VerifyHashedPassword(user, hashedPassword, providedPassword);
            return result != PasswordVerificationResult.Failed;
        }

        public bool CheckStrength(string password)
        {
            var matches = this.rules.Select(regex => regex.Match(password));
            return matches.All(m => m.Success);
        }

        public void Dispose()
        {
        }
    }
}
