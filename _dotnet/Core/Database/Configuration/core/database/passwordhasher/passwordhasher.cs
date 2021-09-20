// <copyright file="PasswordHasher.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using Domain;
    using Microsoft.AspNetCore.Identity;

    public class PasswordHasher : IPasswordHasher
    {
        private readonly PasswordHasher<string> passwordHasher;

        public PasswordHasher() => this.passwordHasher = new PasswordHasher<string>();

        public string HashPassword(string user, string password) => this.passwordHasher.HashPassword(user, password);

        public bool VerifyHashedPassword(string user, string hashedPassword, string providedPassword)
        {
            var result = this.passwordHasher.VerifyHashedPassword(user, hashedPassword, providedPassword);
            return result != PasswordVerificationResult.Failed;
        }

        public void Dispose()
        {
        }
    }
}
