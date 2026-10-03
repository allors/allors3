// <copyright file="FakeEntraAccounts.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    // An account of the fake directory: a member of the tenant, a guest invited from another
    // organization, or a program with a registration of its own. The tests sign in as these.
    public sealed record FakeEntraAccount(
        string Id,
        Guid ObjectId,
        string UserName = null,
        string DisplayName = null,
        string Email = null,
        string Password = null,
        string HomeTenantId = null,
        string ClientId = null,
        string ClientSecret = null,
        string[] Roles = null)
    {
        public bool IsProgram => this.ClientId != null;

        public bool IsGuest => this.HomeTenantId != null;
    }

    // The accounts the fake knows. Fixed ids, so that a test can find the user an account created.
    public static class FakeEntraAccounts
    {
        public const string Password = "Fake-Entra-Passw0rd!";

        // The tenant of the customer whose employee is a guest here.
        public const string CustomerTenantId = "3c3b7a1e-5f2c-4d9b-9d6a-2e8f1b4c7a90";

        public static readonly FakeEntraAccount Tester = new(
            "tester",
            new Guid("0a1e6b3c-9d2f-4e8a-b7c6-5d4e3f2a1b01"),
            UserName: "tester@allors-test.example",
            DisplayName: "Tess Tester",
            Email: "tester@allors-test.example",
            Password: Password);

        // The Test domain's user factory refuses an account whose user name starts with "refused".
        public static readonly FakeEntraAccount Refused = new(
            "refused",
            new Guid("0a1e6b3c-9d2f-4e8a-b7c6-5d4e3f2a1b02"),
            UserName: "refused@allors-test.example",
            DisplayName: "Rex Refused",
            Email: "refused@allors-test.example",
            Password: Password);

        // A B2B guest: an object in this tenant whose home is the customer's tenant.
        public static readonly FakeEntraAccount Guest = new(
            "guest",
            new Guid("0a1e6b3c-9d2f-4e8a-b7c6-5d4e3f2a1b03"),
            UserName: "guest@customer.example",
            DisplayName: "Gus Guest",
            Email: "guest@customer.example",
            Password: Password,
            HomeTenantId: CustomerTenantId);

        // A program with its own registration, calling the API with an app role.
        public static readonly FakeEntraAccount Program = new(
            "program",
            new Guid("0a1e6b3c-9d2f-4e8a-b7c6-5d4e3f2a1b04"),
            DisplayName: "Allors Test Program",
            ClientId: "7f1d2c3b-4a59-4e6f-8b7c-9d0e1f2a3b4c",
            ClientSecret: "fake-program-secret",
            Roles: new[] { "Programs.Access" });

        public static readonly IReadOnlyList<FakeEntraAccount> All = new[] { Tester, Refused, Guest, Program };

        public static FakeEntraAccount ById(string id) => All.FirstOrDefault(v => string.Equals(v.Id, id, StringComparison.OrdinalIgnoreCase));

        public static FakeEntraAccount ByUserName(string userName) => All.FirstOrDefault(v => string.Equals(v.UserName, userName, StringComparison.OrdinalIgnoreCase));

        public static FakeEntraAccount ByClientId(string clientId) => All.FirstOrDefault(v => string.Equals(v.ClientId, clientId, StringComparison.OrdinalIgnoreCase));
    }
}
