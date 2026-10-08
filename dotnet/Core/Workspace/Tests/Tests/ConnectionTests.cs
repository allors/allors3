// <copyright file="ConnectionTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors;
    using Allors.Ranges;
    using Allors.Workspace.Connection;
    using Allors.Workspace.Data;
    using Allors.Workspace.Meta.Lazy;
    using Xunit;

    /// <summary>
    /// The contract of the connection, exercised without a session: a pull by the query model
    /// answers ids and leaves records behind, a record answers its roles as values and its
    /// permissions against the grants and revocations of the user, and a push and an invoke take
    /// ids and versions.
    /// </summary>
    public abstract class ConnectionTests : Test
    {
        protected ConnectionTests(Fixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async void PullByExtentAnswersIdsAndLeavesRecords()
        {
            await this.Login("administrator");
            var connection = this.DatabaseConnection;

            var result = await connection.PullAsync(new[] { new Pull { Extent = new Filter(this.M.C1) } });

            Assert.False(result.HasErrors);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            var ids = result.Collections[this.M.C1.PluralName];
            Assert.Equal(4, ids.Length);
            Assert.Equal(ids.OrderBy(v => v), result.Pool.OrderBy(v => v));

            foreach (var id in ids)
            {
                Assert.True(id > 0);

                var record = connection.GetRecord(id);
                Assert.NotNull(record);
                Assert.Equal(id, record.Id);
                Assert.Same(this.M.C1, record.Class);
                Assert.True(record.Version >= Allors.Version.DatabaseInitial.Value);
            }
        }

        [Fact]
        public async void RecordAnswersRolesAsValues()
        {
            await this.Login("administrator");
            var connection = this.DatabaseConnection;

            var result = await connection.PullAsync(new[] { new Pull { Extent = new Filter(this.M.C1) } });
            var idByName = result.Collections[this.M.C1.PluralName].ToDictionary(id => (string)connection.GetRecord(id).GetRole(this.M.C1.Name));

            var c1A = connection.GetRecord(idByName[Names.c1A]);
            Assert.Null(c1A.GetRole(this.M.C1.C1AllorsString));
            Assert.Equal(idByName[Names.c1B], Assert.IsType<long>(c1A.GetRole(this.M.C1.C1C1One2One)));

            var c1B = connection.GetRecord(idByName[Names.c1B]);
            Assert.Equal("ᴀbra", c1B.GetRole(this.M.C1.C1AllorsString));
            Assert.Equal(true, c1B.GetRole(this.M.C1.C1AllorsBoolean));

            var c1C = connection.GetRecord(idByName[Names.c1C]);
            var many2Many = Assert.IsAssignableFrom<IRange<long>>(c1C.GetRole(this.M.C1.C1C1Many2Manies));
            Assert.Equal(new[] { idByName[Names.c1B], idByName[Names.c1C] }.OrderBy(v => v), many2Many);
        }

        [Fact]
        public async void PermissionsAreAnsweredPerRecord()
        {
            await this.Login("administrator");
            var connection = this.DatabaseConnection;

            var result = await connection.PullAsync(new[] { new Pull { Extent = new Filter(this.M.C1) } });

            var read = connection.GetPermission(this.M.C1, this.M.C1.C1AllorsString, Operations.Read);
            var write = connection.GetPermission(this.M.C1, this.M.C1.C1AllorsString, Operations.Write);
            Assert.NotEqual(0, read);
            Assert.NotEqual(0, write);

            foreach (var id in result.Collections[this.M.C1.PluralName])
            {
                var record = connection.GetRecord(id);
                Assert.True(record.IsPermitted(read));
                Assert.True(record.IsPermitted(write));
            }
        }

        [Fact]
        public async void WithoutAccessControlNothingIsPermitted()
        {
            await this.Login("noacl");
            var connection = this.DatabaseConnection;

            var result = await connection.PullAsync(new[] { new Pull { Extent = new Filter(this.M.C1) } });

            var read = connection.GetPermission(this.M.C1, this.M.C1.C1AllorsString, Operations.Read);
            Assert.Equal(0, read);

            foreach (var id in result.Collections[this.M.C1.PluralName])
            {
                Assert.False(connection.GetRecord(id).IsPermitted(read));
            }
        }

        [Fact]
        public async void RevocationDeniesTheWrite()
        {
            await this.Login("administrator");
            var connection = this.DatabaseConnection;

            var result = await connection.PullAsync(new[] { new Pull { Extent = new Filter(this.M.Denied) } });

            var read = connection.GetPermission(this.M.Denied, this.M.Denied.DefaultWorkspaceProperty, Operations.Read);
            var write = connection.GetPermission(this.M.Denied, this.M.Denied.DefaultWorkspaceProperty, Operations.Write);
            Assert.NotEqual(0, read);
            Assert.NotEqual(0, write);

            var ids = result.Collections[this.M.Denied.PluralName];
            Assert.NotEmpty(ids);

            foreach (var id in ids)
            {
                var record = connection.GetRecord(id);
                Assert.True(record.IsPermitted(read));
                Assert.False(record.IsPermitted(write));
            }
        }

        [Fact]
        public async void PushNewObjectAnswersItsDatabaseId()
        {
            await this.Login("administrator");
            var connection = this.DatabaseConnection;

            var workspaceId = new IdGenerator().Next();
            var newObject = new PushNewObject(workspaceId, this.M.C1, new[] { new RoleChange(this.M.C1.C1AllorsString, "pushed") });

            var pushed = await connection.PushAsync(new[] { newObject }, null);

            Assert.False(pushed.HasErrors);
            var id = pushed.DatabaseIdByWorkspaceId[workspaceId];
            Assert.True(id > 0);

            var result = await connection.PullAsync(new[] { new Pull { ObjectId = id } });

            Assert.False(result.HasErrors);
            Assert.Contains(id, result.Pool);

            var record = connection.GetRecord(id);
            Assert.Same(this.M.C1, record.Class);
            Assert.Equal("pushed", record.GetRole(this.M.C1.C1AllorsString));
        }

        [Fact]
        public async void PushChangedObjectTakesTheVersionItWasChangedFrom()
        {
            await this.Login("administrator");
            var connection = this.DatabaseConnection;

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = Names.c1A } } };
            var id = (await connection.PullAsync(new[] { pull })).Collections[this.M.C1.PluralName].Single();
            var before = connection.GetRecord(id);

            var pushed = await connection.PushAsync(null, new[] { new PushChangedObject(id, before.Version, new[] { new RoleChange(this.M.C1.C1AllorsString, "X") }) });

            Assert.False(pushed.HasErrors);

            await connection.PullAsync(new[] { pull });
            var after = connection.GetRecord(id);
            Assert.NotSame(before, after);
            Assert.True(after.Version > before.Version);
            Assert.Equal("X", after.GetRole(this.M.C1.C1AllorsString));

            var stale = await connection.PushAsync(null, new[] { new PushChangedObject(id, before.Version, new[] { new RoleChange(this.M.C1.C1AllorsString, "Y") }) });

            Assert.True(stale.HasErrors);
            Assert.Contains(id, stale.VersionErrors);
        }

        [Fact]
        public async void InvokeRunsTheMethod()
        {
            await this.Login("administrator");
            var connection = this.DatabaseConnection;

            var pull = new Pull { Extent = new Filter(this.M.Organisation) };
            var id = (await connection.PullAsync(new[] { pull })).Collections[this.M.Organisation.PluralName].First();
            var before = connection.GetRecord(id);
            Assert.NotEqual(true, before.GetRole(this.M.Organisation.JustDidIt));

            var invoked = await connection.InvokeAsync(new[] { new Invocation(id, before.Version, this.M.Organisation.JustDoIt) });

            Assert.False(invoked.HasErrors);

            await connection.PullAsync(new[] { pull });
            Assert.Equal(true, connection.GetRecord(id).GetRole(this.M.Organisation.JustDidIt));
        }

        [Fact]
        public async void RecordChangedIsRaisedWhenAPullReplacesARecord()
        {
            await this.Login("administrator");
            var connection = this.DatabaseConnection;

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = Names.c1A } } };
            var id = (await connection.PullAsync(new[] { pull })).Collections[this.M.C1.PluralName].Single();
            var version = connection.GetRecord(id).Version;

            var changed = new List<RecordChangedEventArgs>();
            connection.RecordChanged += (sender, e) => changed.Add(e);

            await connection.PullAsync(new[] { pull });
            Assert.Empty(changed);

            var pushed = await connection.PushAsync(null, new[] { new PushChangedObject(id, version, new[] { new RoleChange(this.M.C1.C1AllorsString, "changed") }) });
            Assert.False(pushed.HasErrors);
            Assert.Empty(changed);

            await connection.PullAsync(new[] { pull });

            var e = Assert.Single(changed);
            Assert.Equal(id, e.Id);
            Assert.Same(connection.GetRecord(id), e.Record);
            Assert.True(e.Record.Version > version);
        }

        [Fact]
        public async void TheConnectionsOfOneUserShareACache()
        {
            await this.Login("administrator");
            var template = this.DatabaseConnection;

            var cache = new MemoryCache(template.WorkspaceName, template.MetaPopulation);
            var first = this.CreateConnection("administrator", cache);
            var second = this.CreateConnection("administrator", cache);

            var pull = new Pull { Extent = new Filter(this.M.C1) };
            var result = await first.PullAsync(new[] { pull });

            Assert.Same(cache, first.Cache);
            Assert.Same(cache, second.Cache);
            Assert.NotEmpty(result.Pool);
            foreach (var id in result.Pool)
            {
                Assert.NotNull(second.GetRecord(id));
                Assert.Same(first.GetRecord(id), second.GetRecord(id));
            }

            var changed = new List<RecordChangedEventArgs>();
            second.RecordChanged += (sender, e) => changed.Add(e);

            await second.PullAsync(new[] { pull });

            Assert.Empty(changed);
        }

        [Fact]
        public async void TheCacheTellsEveryConnectionWhenAnotherConnectionReplacesARecord()
        {
            await this.Login("administrator");
            var template = this.DatabaseConnection;

            var cache = new MemoryCache(template.WorkspaceName, template.MetaPopulation);
            var first = this.CreateConnection("administrator", cache);
            var second = this.CreateConnection("administrator", cache);

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = Names.c1A } } };
            var id = (await first.PullAsync(new[] { pull })).Collections[this.M.C1.PluralName].Single();
            var before = first.GetRecord(id);

            var changed = new List<RecordChangedEventArgs>();
            cache.RecordChanged += (sender, e) => changed.Add(e);

            var pushed = await first.PushAsync(null, new[] { new PushChangedObject(id, before.Version, new[] { new RoleChange(this.M.C1.C1AllorsString, "shared") }) });
            Assert.False(pushed.HasErrors);
            Assert.Empty(changed);

            await second.PullAsync(new[] { pull });

            var e = Assert.Single(changed);
            Assert.Equal(id, e.Id);
            Assert.Same(first.GetRecord(id), e.Record);
            Assert.True(e.Record.Version > before.Version);
            Assert.Equal("shared", first.GetRecord(id).GetRole(this.M.C1.C1AllorsString));
        }

        [Fact]
        public async void ACacheOfAnotherWorkspaceNameIsRefused()
        {
            await this.Login("administrator");
            var template = this.DatabaseConnection;

            var cache = new MemoryCache("Other", template.MetaPopulation);

            var exception = Assert.Throws<ArgumentException>(() => this.CreateConnection("administrator", cache));
            Assert.Contains("Other", exception.Message);
            Assert.Contains(template.WorkspaceName, exception.Message);
        }

        [Fact]
        public async void ACacheOfAnotherMetaPopulationIsRefused()
        {
            await this.Login("administrator");
            var template = this.DatabaseConnection;

            var cache = new MemoryCache(template.WorkspaceName, new MetaBuilder().Build());

            var exception = Assert.Throws<ArgumentException>(() => this.CreateConnection("administrator", cache));
            Assert.Contains("meta population", exception.Message);
        }

        [Fact]
        public async void TheConnectionLearnsTheDatabaseAndTheUserFromTheFirstResponse()
        {
            await this.Login("administrator");
            var connection = this.DatabaseConnection;

            Assert.Null(connection.DatabaseId);
            Assert.Null(connection.UserId);
            Assert.Null(connection.Cache.Key);
            Assert.Matches("^[0-9a-f]{16}$", connection.MetaFingerprint);

            await connection.PullAsync(new[] { new Pull { Extent = new Filter(this.M.C1) } });

            Assert.False(string.IsNullOrWhiteSpace(connection.DatabaseId));
            Assert.True(connection.UserId > 0);
            Assert.Equal(new CacheKey(connection.DatabaseId, connection.UserId.Value, connection.WorkspaceName, connection.MetaFingerprint), connection.Cache.Key);

            var other = this.CreateConnection("noacl", null);
            await other.PullAsync(new[] { new Pull { Extent = new Filter(this.M.C1) } });

            Assert.Equal(connection.DatabaseId, other.DatabaseId);
            Assert.NotEqual(connection.UserId, other.UserId);
        }

        [Fact]
        public async void TheCacheRefusesAConnectionOfAnotherUser()
        {
            await this.Login("administrator");
            var template = this.DatabaseConnection;

            var cache = new MemoryCache(template.WorkspaceName, template.MetaPopulation);
            var administrator = this.CreateConnection("administrator", cache);
            var noacl = this.CreateConnection("noacl", cache);

            var pull = new Pull { Extent = new Filter(this.M.C1) };
            var result = await administrator.PullAsync(new[] { pull });

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => noacl.PullAsync(new[] { pull }));

            Assert.Contains($"user {administrator.UserId}", exception.Message);
            Assert.Contains($"user {noacl.UserId}", exception.Message);
            foreach (var id in result.Pool)
            {
                Assert.NotNull(administrator.GetRecord(id));
            }
        }

        [Fact]
        public async void TheServerRefusesAConnectionForAnotherWorkspaceName()
        {
            await this.Login("administrator");
            var template = this.DatabaseConnection;

            var connection = new DatabaseConnection("Other", template.MetaPopulation, this.Profile.CreateTransport("administrator"), template.Ranges);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => connection.PullAsync(new[] { new Pull { Extent = new Filter(this.M.C1) } }));

            Assert.Contains("'Other'", exception.Message);
            Assert.Contains($"'{template.WorkspaceName}'", exception.Message);
            Assert.Null(connection.DatabaseId);
        }

        [Fact]
        public async void AGrantWhoseVersionChangedIsRequestedAgain()
        {
            await this.Login("administrator");
            var connection = this.DatabaseConnection;

            var pull = new Pull { Extent = new Filter(this.M.C1) };
            var result = await connection.PullAsync(new[] { pull });
            var write = connection.GetPermission(this.M.C1, this.M.C1.C1AllorsString, Operations.Write);
            var read = connection.GetPermission(this.M.C1, this.M.C1.C1AllorsString, Operations.Read);
            Assert.All(result.Pool, id => Assert.True(connection.GetRecord(id).IsPermitted(write)));

            await this.Profile.RemoveAdministratorPermission(this.M.C1.C1AllorsString, Operations.Write);

            await connection.PullAsync(new[] { pull });

            Assert.All(result.Pool, id => Assert.False(connection.GetRecord(id).IsPermitted(write)));
            Assert.All(result.Pool, id => Assert.True(connection.GetRecord(id).IsPermitted(read)));
        }

        [Fact]
        public async void ARevocationWhoseVersionChangedIsRequestedAgain()
        {
            await this.Login("administrator");
            var connection = this.DatabaseConnection;

            var pull = new Pull { Extent = new Filter(this.M.Denied) };
            var result = await connection.PullAsync(new[] { pull });
            var read = connection.GetPermission(this.M.Denied, this.M.Denied.DefaultWorkspaceProperty, Operations.Read);
            var write = connection.GetPermission(this.M.Denied, this.M.Denied.DefaultWorkspaceProperty, Operations.Write);
            Assert.NotEmpty(result.Pool);
            Assert.All(result.Pool, id => Assert.True(connection.GetRecord(id).IsPermitted(read)));
            Assert.All(result.Pool, id => Assert.False(connection.GetRecord(id).IsPermitted(write)));

            await this.Profile.DenyPermission(this.M.Denied.DefaultWorkspaceProperty, Operations.Read);

            await connection.PullAsync(new[] { pull });

            Assert.All(result.Pool, id => Assert.False(connection.GetRecord(id).IsPermitted(read)));
        }

        [Fact]
        public async void APullIsPersistedAndANewConnectionRestoresItWithoutAskingTheServer()
        {
            await this.Login("administrator");
            var provider = new MemoryPersistenceProvider();

            var (first, firstTransport) = this.CreatePersistedConnection(provider);
            var pull = new Pull { Extent = new Filter(this.M.C1) };
            var result = await first.PullAsync(new[] { pull });
            var write = first.GetPermission(this.M.C1, this.M.C1.C1AllorsString, Operations.Write);

            Assert.NotEmpty(result.Pool);
            Assert.Equal(1, firstTransport.SyncCount);
            Assert.Equal(result.Pool.OrderBy(v => v), provider.Objects(first.Cache.Key).Keys.OrderBy(v => v));

            var (second, secondTransport) = this.CreatePersistedConnection(provider);
            var restored = await second.PullAsync(new[] { pull });

            Assert.Equal(result.Pool.OrderBy(v => v), restored.Pool.OrderBy(v => v));
            Assert.Equal(0, secondTransport.SyncCount);
            Assert.Equal(0, secondTransport.AccessCount);
            Assert.Equal(0, secondTransport.PermissionCount);
            Assert.Equal(write, second.GetPermission(this.M.C1, this.M.C1.C1AllorsString, Operations.Write));

            foreach (var id in result.Pool)
            {
                var original = first.GetRecord(id);
                var record = second.GetRecord(id);
                Assert.NotNull(record);
                Assert.Same(original.Class, record.Class);
                Assert.Equal(original.Version, record.Version);
                Assert.Equal(original.GetRole(this.M.C1.C1AllorsString), record.GetRole(this.M.C1.C1AllorsString));
                Assert.Equal(original.GetRole(this.M.C1.C1C1Many2Manies), record.GetRole(this.M.C1.C1C1Many2Manies));
                Assert.Equal(original.GrantIds, record.GrantIds);
                Assert.True(record.IsPermitted(write));
            }
        }

        [Fact]
        public async void AChangedObjectIsSyncedAndTheRestRestored()
        {
            await this.Login("administrator");
            var provider = new MemoryPersistenceProvider();

            var (first, _) = this.CreatePersistedConnection(provider);
            var pull = new Pull { Extent = new Filter(this.M.C1) };
            var result = await first.PullAsync(new[] { pull });
            var id = result.Collections[this.M.C1.PluralName].First(v => (string)first.GetRecord(v).GetRole(this.M.C1.Name) == Names.c1A);

            var pushed = await first.PushAsync(null, new[] { new PushChangedObject(id, first.GetRecord(id).Version, new[] { new RoleChange(this.M.C1.C1AllorsString, "persisted") }) });
            Assert.False(pushed.HasErrors);

            var (second, secondTransport) = this.CreatePersistedConnection(provider);
            await second.PullAsync(new[] { pull });

            Assert.Equal(1, secondTransport.SyncCount);
            Assert.Equal(new[] { id }, secondTransport.LastSyncRequest.o);
            Assert.Equal("persisted", second.GetRecord(id).GetRole(this.M.C1.C1AllorsString));
            Assert.Equal(second.GetRecord(id).Version, provider.Objects(second.Cache.Key)[id].v);
        }

        [Fact]
        public async void ClearAsyncForgetsThePersistedView()
        {
            await this.Login("administrator");
            var provider = new MemoryPersistenceProvider();

            var (first, _) = this.CreatePersistedConnection(provider);
            var pull = new Pull { Extent = new Filter(this.M.C1) };
            var result = await first.PullAsync(new[] { pull });
            var key = first.Cache.Key;
            Assert.NotEmpty(provider.Objects(key));

            await first.ClearAsync();

            Assert.Empty(provider.Objects(key));
            Assert.All(result.Pool, id => Assert.Null(first.GetRecord(id)));

            var (second, secondTransport) = this.CreatePersistedConnection(provider);
            await second.PullAsync(new[] { pull });
            Assert.Equal(1, secondTransport.SyncCount);
            Assert.Equal(result.Pool.OrderBy(v => v), secondTransport.LastSyncRequest.o.OrderBy(v => v));
        }

        private (DatabaseConnection Connection, CountingTransport Transport) CreatePersistedConnection(IPersistenceProvider provider)
        {
            var template = this.DatabaseConnection;
            var transport = new CountingTransport(this.Profile.CreateTransport("administrator"));
            var cache = new MemoryCache(template.WorkspaceName, template.MetaPopulation);
            return (new DatabaseConnection(template.WorkspaceName, template.MetaPopulation, transport, template.Ranges, cache, provider), transport);
        }

        private DatabaseConnection CreateConnection(string userName, ICache cache)
        {
            var template = this.DatabaseConnection;
            return new DatabaseConnection(template.WorkspaceName, template.MetaPopulation, this.Profile.CreateTransport(userName), template.Ranges, cache);
        }
    }
}
