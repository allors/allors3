// <copyright file="Users.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;

    public partial class Users
    {
        public static readonly Guid AdministratorId = new Guid("880AFBDD-E1D3-4382-A54B-1E008DA58CB7");
        public static readonly Guid JaneId = new Guid("52396749-5CFF-4D1D-889F-DFD12753945F");
        public static readonly Guid AgentId = new Guid("E0310978-B016-40D6-926B-1155A4B9BC82");
        public static readonly Guid NoAclId = new Guid("5CF05A59-C6E2-44DD-87F8-ADB6A670ED0A");
        public static readonly Guid NoPermId = new Guid("171125FF-E79A-447C-86D8-018BB88C09A7");

        // The test clients sign these users in by UniqueId, so no test depends on how an
        // authentication plug-in names a user; the tests keep naming the users as before.
        private static readonly Dictionary<string, Guid> TestUserIdByAlias = new(StringComparer.OrdinalIgnoreCase)
        {
            ["administrator"] = AdministratorId,
            ["jane@example.com"] = JaneId,
            ["agent"] = AgentId,
            ["noacl"] = NoAclId,
            ["noperm"] = NoPermId,
        };

        public static bool TryGetTestUserId(string alias, out Guid uniqueId) => TestUserIdByAlias.TryGetValue(alias, out uniqueId);

        public static Guid TestUserId(string alias) =>
            TryGetTestUserId(alias, out var uniqueId)
                ? uniqueId
                : throw new ArgumentException(
                    $"'{alias}' is not a user of the test population. Sign in as one of: {string.Join(", ", TestUserIdByAlias.Keys)}.",
                    nameof(alias));
    }
}
