// <copyright file="IPasswordHasher.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public interface IPasswordHasher
    {
        string HashPassword(string user, string password);

        bool VerifyHashedPassword(string user, string hashedPassword, string providedPassword);

        bool CheckStrength(string password);
    }
}
