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
        public async void ConnectionsOfOneUserKeepIndependentRecordsAndPermissions()
        {
            await this.Login("administrator");
            var first = this.CreateConnection("administrator");
            var second = this.CreateConnection("administrator");
            var pull = new Pull { Extent = new Filter(this.M.C1) };
            var result = await first.PullAsync(new[] { pull });
            var write = first.GetPermission(this.M.C1, this.M.C1.C1AllorsString, Operations.Write);

            Assert.NotEmpty(result.Pool);
            Assert.NotEqual(0, write);
            Assert.Equal(0, second.GetPermission(this.M.C1, this.M.C1.C1AllorsString, Operations.Write));
            Assert.All(result.Pool, id => Assert.Null(second.GetRecord(id)));

            await second.PullAsync(new[] { pull });

            foreach (var id in result.Pool)
            {
                Assert.NotSame(first.GetRecord(id), second.GetRecord(id));
                Assert.True(first.GetRecord(id).IsPermitted(write));
                Assert.True(second.GetRecord(id).IsPermitted(write));
            }

            await this.Profile.RemoveAdministratorPermission(this.M.C1.C1AllorsString, Operations.Write);
            await second.PullAsync(new[] { pull });

            Assert.All(result.Pool, id => Assert.True(first.GetRecord(id).IsPermitted(write)));
            Assert.All(result.Pool, id => Assert.False(second.GetRecord(id).IsPermitted(write)));

            await first.PullAsync(new[] { pull });

            Assert.All(result.Pool, id => Assert.False(first.GetRecord(id).IsPermitted(write)));
        }

        [Fact]
        public async void APullReplacesRecordsAndRaisesEventsOnlyOnItsConnection()
        {
            await this.Login("administrator");
            var first = this.CreateConnection("administrator");
            var second = this.CreateConnection("administrator");
            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = Names.c1A } } };
            var id = (await first.PullAsync(new[] { pull })).Collections[this.M.C1.PluralName].Single();
            await second.PullAsync(new[] { pull });
            var before = first.GetRecord(id);
            var firstChanged = new List<RecordChangedEventArgs>();
            var secondChanged = new List<RecordChangedEventArgs>();
            first.RecordChanged += (sender, e) => firstChanged.Add(e);
            second.RecordChanged += (sender, e) => secondChanged.Add(e);

            var pushed = await first.PushAsync(null, new[] { new PushChangedObject(id, before.Version, new[] { new RoleChange(this.M.C1.C1AllorsString, "changed") }) });
            Assert.False(pushed.HasErrors);
            Assert.Empty(firstChanged);
            Assert.Empty(secondChanged);

            await second.PullAsync(new[] { pull });

            Assert.Empty(firstChanged);
            Assert.Same(before, first.GetRecord(id));
            var secondEvent = Assert.Single(secondChanged);
            Assert.Equal(id, secondEvent.Id);
            Assert.Same(second.GetRecord(id), secondEvent.Record);
            Assert.True(secondEvent.Record.Version > before.Version);
            Assert.Equal("changed", secondEvent.Record.GetRole(this.M.C1.C1AllorsString));

            await first.PullAsync(new[] { pull });

            var firstEvent = Assert.Single(firstChanged);
            Assert.Equal(id, firstEvent.Id);
            Assert.Same(first.GetRecord(id), firstEvent.Record);
            Assert.Equal(secondEvent.Record.Version, firstEvent.Record.Version);
            Assert.NotSame(secondEvent.Record, firstEvent.Record);
            Assert.Single(secondChanged);
        }

        [Fact]
        public async void TheConnectionLearnsTheDatabaseAndTheUserFromTheFirstResponse()
        {
            await this.Login("administrator");
            var connection = this.DatabaseConnection;

            Assert.Null(connection.DatabaseId);
            Assert.Null(connection.UserId);
            Assert.Matches("^[0-9a-f]{16}$", connection.MetaFingerprint);

            await connection.PullAsync(new[] { new Pull { Extent = new Filter(this.M.C1) } });

            Assert.False(string.IsNullOrWhiteSpace(connection.DatabaseId));
            Assert.True(connection.UserId > 0);

            var other = this.CreateConnection("noacl");
            await other.PullAsync(new[] { new Pull { Extent = new Filter(this.M.C1) } });

            Assert.Equal(connection.DatabaseId, other.DatabaseId);
            Assert.NotEqual(connection.UserId, other.UserId);
        }

        [Fact]
        public async void ConnectionsOfDifferentUsersKeepTheirOwnView()
        {
            await this.Login("administrator");
            var administrator = this.CreateConnection("administrator");
            var noacl = this.CreateConnection("noacl");
            var pull = new Pull { Extent = new Filter(this.M.C1) };
            var result = await administrator.PullAsync(new[] { pull });
            var read = administrator.GetPermission(this.M.C1, this.M.C1.C1AllorsString, Operations.Read);
            var originals = result.Pool.ToDictionary(id => id, administrator.GetRecord);

            var otherResult = await noacl.PullAsync(new[] { pull });

            Assert.False(otherResult.HasErrors);
            Assert.NotEmpty(result.Pool);
            Assert.Equal(result.Pool.OrderBy(id => id), otherResult.Pool.OrderBy(id => id));
            Assert.NotEqual(administrator.UserId, noacl.UserId);
            Assert.NotEqual(0, read);
            Assert.Equal(0, noacl.GetPermission(this.M.C1, this.M.C1.C1AllorsString, Operations.Read));
            foreach (var id in result.Pool)
            {
                var record = noacl.GetRecord(id);
                Assert.NotNull(record);
                Assert.NotSame(originals[id], record);
                Assert.Null(record.GetRole(this.M.C1.C1AllorsString));
                Assert.False(record.IsPermitted(read));
                Assert.Same(originals[id], administrator.GetRecord(id));
                Assert.True(originals[id].IsPermitted(read));
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

        private DatabaseConnection CreateConnection(string userName)
        {
            var template = this.DatabaseConnection;
            return new DatabaseConnection(template.WorkspaceName, template.MetaPopulation, this.Profile.CreateTransport(userName), template.Ranges);
        }
    }
}
