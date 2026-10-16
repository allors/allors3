// <copyright file="ContentTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the ContentTests type.</summary>

namespace Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Allors.Database.Configuration;
    using Allors.Database.Data;
    using Allors.Database.Domain;
    using Allors.Database.Protocol.Json;
    using Allors.Protocol.Json;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Protocol.Json.SystemTextJson;
    using Xunit;
    using Extent = Allors.Database.Data.Extent;
    using Pull = Allors.Protocol.Json.Data.Pull;
    using Result = Allors.Database.Data.Result;

    public class PullExtentTests : ApiTest, IClassFixture<Fixture>
    {
        public PullExtentTests(Fixture fixture) : base(fixture) => this.UnitConvert = new UnitConvert();

        public IUnitConvert UnitConvert { get; }

        [Theory]
        [InlineData("unknown-type")]
        [InlineData("unit-type")]
        [InlineData("filter-operands")]
        [InlineData("operator-predicate")]
        [InlineData("operator-arity")]
        [InlineData("unknown-role")]
        [InlineData("wrong-owner")]
        [InlineData("conflicting-value")]
        [InlineData("wrong-unit")]
        [InlineData("unsupported-exists-parameter")]
        [InlineData("operator-extra-operand")]
        [InlineData("unit-select")]
        [InlineData("empty-next-select")]
        public void CompleteReadRejectsInvalidInlineDeclarations(string invalid)
        {
            var extent = new Allors.Protocol.Json.Data.Extent { k = Allors.Protocol.Json.Data.ExtentKind.Filter, t = this.M.C1.Tag };
            Allors.Protocol.Json.Data.Select select = null;
            switch (invalid)
            {
                case "unknown-type": extent.t = "unknown"; break;
                case "unit-type": extent.t = this.M.C1.Name.ObjectType.Tag; break;
                case "filter-operands": extent.o = new[] { new Allors.Protocol.Json.Data.Extent { k = Allors.Protocol.Json.Data.ExtentKind.Filter, t = this.M.C1.Tag } }; break;
                case "operator-predicate":
                    extent.k = Allors.Protocol.Json.Data.ExtentKind.Union;
                    extent.p = new Allors.Protocol.Json.Data.Predicate { k = Allors.Protocol.Json.Data.PredicateKind.Exists, r = this.M.C1.Name.RelationType.Tag };
                    break;
                case "operator-arity": extent.k = Allors.Protocol.Json.Data.ExtentKind.Union; extent.t = null; extent.o = new Allors.Protocol.Json.Data.Extent[0]; break;
                case "unknown-role": extent.p = new Allors.Protocol.Json.Data.Predicate { k = Allors.Protocol.Json.Data.PredicateKind.Exists, r = "unknown" }; break;
                case "wrong-owner": extent.p = new Allors.Protocol.Json.Data.Predicate { k = Allors.Protocol.Json.Data.PredicateKind.Exists, r = this.M.User.UserName.RelationType.Tag }; break;
                case "conflicting-value": extent.p = new Allors.Protocol.Json.Data.Predicate { k = Allors.Protocol.Json.Data.PredicateKind.Equals, r = this.M.C1.Name.RelationType.Tag, v = "x", p = "name" }; break;
                case "wrong-unit": extent.p = new Allors.Protocol.Json.Data.Predicate { k = Allors.Protocol.Json.Data.PredicateKind.Equals, r = this.M.C1.Name.RelationType.Tag, v = true }; break;
                case "unsupported-exists-parameter": extent.p = new Allors.Protocol.Json.Data.Predicate { k = Allors.Protocol.Json.Data.PredicateKind.Exists, r = this.M.C1.Name.RelationType.Tag, p = "property" }; break;
                case "operator-extra-operand":
                    extent = new Allors.Protocol.Json.Data.Extent { k = Allors.Protocol.Json.Data.ExtentKind.Union, o = new[] { extent, extent, extent } };
                    break;
                case "unit-select": select = new Allors.Protocol.Json.Data.Select { r = this.M.C1.Name.RelationType.Tag }; break;
                case "empty-next-select": select = new Allors.Protocol.Json.Data.Select { r = this.M.C1.C1C2One2One.RelationType.Tag, n = new Allors.Protocol.Json.Data.Select() }; break;
            }

            this.Transaction.Rollback();
            var population = ((Allors.Database.Adapters.Sql.Database)this.Transaction.Database).PopulationLifecycle.Renew();
            var user = this.SetUser("jane@example.com");
            var request = new Allors.Protocol.Json.Api.Read.ReadRequest
            {
                v = 1, i = "inline-grammar", k = "read",
                p = new Allors.Protocol.Json.Api.Read.ReadPull
                {
                    l = new[] { new Pull { e = extent, r = select == null ? null : new[] { new Allors.Protocol.Json.Data.Result { s = select } } } },
                    d = System.Array.Empty<PullDependency>(),
                },
            };
            var executor = new ReadExecutor(this.Transaction.Database,
                new ReadConfiguration(new Dictionary<string, string> { ["Default"] = "revision-1" }));
            var raw = executor.Execute(System.Text.Json.JsonSerializer.Serialize(request), "Default", new LocalReadIdentity(population.Incarnation, user.Id));
            var response = Assert.IsType<Allors.Protocol.Json.Api.Read.ReadFailure>(ReadJson.DeserializeResponse(raw, request));
            Assert.NotNull(response.sc);
            Assert.Equal("revision-1", response.sc.r);
            Assert.Equal(user.Id, response.sc.u);
            Assert.StartsWith("$pull", Assert.Single(response.e).m);
            using var json = System.Text.Json.JsonDocument.Parse(raw);
            foreach (var field in new[] { "l", "c", "o", "v", "p", "s", "pm" })
            {
                Assert.False(json.RootElement.TryGetProperty(field, out _));
            }
        }

        [Fact]
        public void ReadTransportRejectsCyclicInlineDeclarations()
        {
            this.Transaction.Rollback();
            var population = ((Allors.Database.Adapters.Sql.Database)this.Transaction.Database).PopulationLifecycle.Renew();
            var user = this.SetUser("jane@example.com");
            var predicate = new Allors.Protocol.Json.Data.Predicate { k = Allors.Protocol.Json.Data.PredicateKind.Not };
            predicate.op = predicate;
            var request = new Allors.Protocol.Json.Api.Read.ReadRequest
            {
                v = 1, i = "cyclic-inline", k = "read",
                p = new Allors.Protocol.Json.Api.Read.ReadPull
                {
                    l = new[] { new Pull { e = new Allors.Protocol.Json.Data.Extent { k = Allors.Protocol.Json.Data.ExtentKind.Filter, t = this.M.C1.Tag, p = predicate } } },
                    d = System.Array.Empty<PullDependency>(),
                },
            };
            var executor = new ReadExecutor(this.Transaction.Database,
                new ReadConfiguration(new Dictionary<string, string> { ["Default"] = "revision-1" }));
            var transport = new Allors.Workspace.Adapters.Local.ReadTransport(executor, "Default", new LocalReadIdentity(population.Incarnation, user.Id));
            Assert.Throws<System.Text.Json.JsonException>(() => transport.CaptureRequest(request));
        }

        [Fact]
        public void CompleteReadAppliesExtentPaginationBeforePoolLimit()
        {
            this.Transaction.Rollback();
            var population = ((Allors.Database.Adapters.Sql.Database)this.Transaction.Database).PopulationLifecycle.Renew();
            var user = this.SetUser("jane@example.com");
            foreach (var item in this.Transaction.Create(this.M.C1, 10002).Cast<C1>())
            {
                item.DerivationCount = 0;
            }
            this.Transaction.Derive();
            this.Transaction.Commit();
            var request = new Allors.Protocol.Json.Api.Read.ReadRequest
            {
                v = 1, i = "paged-large-extent", k = "read",
                p = new Allors.Protocol.Json.Api.Read.ReadPull
                {
                    l = new[] { new Pull
                    {
                        e = new Allors.Protocol.Json.Data.Extent { k = Allors.Protocol.Json.Data.ExtentKind.Filter, t = this.M.C1.Tag },
                        r = new[] { new Allors.Protocol.Json.Data.Result { n = "page", k = 10001, t = 1 } },
                    } },
                    d = System.Array.Empty<PullDependency>(),
                },
            };
            var executor = new ReadExecutor(this.Transaction.Database,
                new ReadConfiguration(new Dictionary<string, string> { ["Default"] = "revision-1" }));
            var raw = System.Text.Json.JsonSerializer.Serialize(request);
            var response = Assert.IsType<Allors.Protocol.Json.Api.Read.ReadSuccess>(ReadJson.DeserializeResponse(
                executor.Execute(raw, "Default", new LocalReadIdentity(population.Incarnation, user.Id)), request));
            Assert.Single(response.c["page"]);
            Assert.Single(response.p);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CompleteReadPreservesEmptyResultArrayMeaning(bool explicitObject)
        {
            this.Transaction.Rollback();
            var population = ((Allors.Database.Adapters.Sql.Database)this.Transaction.Database).PopulationLifecycle.Renew();
            var user = this.SetUser("jane@example.com");
            var target = new C1s(this.Transaction).Extent().First();
            var selection = new Pull { r = System.Array.Empty<Allors.Protocol.Json.Data.Result>() };
            if (explicitObject) { selection.o = target.Id; }
            else { selection.e = new Allors.Protocol.Json.Data.Extent { k = Allors.Protocol.Json.Data.ExtentKind.Filter, t = this.M.C1.Tag }; }
            var request = new Allors.Protocol.Json.Api.Read.ReadRequest
            {
                v = 1, i = "empty-results", k = "read",
                p = new Allors.Protocol.Json.Api.Read.ReadPull { l = new[] { selection }, d = System.Array.Empty<PullDependency>() },
            };
            var executor = new ReadExecutor(this.Transaction.Database,
                new ReadConfiguration(new Dictionary<string, string> { ["Default"] = "revision-1" }));
            var raw = System.Text.Json.JsonSerializer.Serialize(request);
            var response = Assert.IsType<Allors.Protocol.Json.Api.Read.ReadSuccess>(ReadJson.DeserializeResponse(
                executor.Execute(raw, "Default", new LocalReadIdentity(population.Incarnation, user.Id)), request));
            Assert.Equal("resolved", Assert.Single(response.l).s);
            Assert.Empty(response.o);
            if (explicitObject)
            {
                Assert.Empty(response.c);
                Assert.Empty(response.p);
                Assert.Empty(response.s);
                Assert.Empty(response.pm);
            }
            else
            {
                Assert.NotEmpty(response.c[this.M.C1.PluralName]);
                Assert.NotEmpty(response.p);
            }
        }

        [Fact]
        public async System.Threading.Tasks.Task ReadTransportOwnsInlineExtentSelectIncludeAndArgumentDefinitions()
        {
            this.Transaction.Rollback();
            var population = ((Allors.Database.Adapters.Sql.Database)this.Transaction.Database).PopulationLifecycle.Renew();
            var user = this.SetUser("jane@example.com");
            var identity = new LocalReadIdentity(population.Incarnation, user.Id);
            var extent = new Allors.Protocol.Json.Data.Extent
            {
                k = Allors.Protocol.Json.Data.ExtentKind.Filter,
                t = this.M.C1.Tag,
                p = new Allors.Protocol.Json.Data.Predicate { k = Allors.Protocol.Json.Data.PredicateKind.Equals, r = this.M.C1.Name.RelationType.Tag, p = "name" },
            };
            var select = new Allors.Protocol.Json.Data.Select
            {
                i = new[] { new Allors.Protocol.Json.Data.Node { r = this.M.C1.C1C2One2One.RelationType.Tag } },
            };
            var configuration = new ReadConfiguration(new Dictionary<string, string> { ["Default"] = "revision-1" });
            var executor = new ReadExecutor(this.Transaction.Database, configuration);
            var transport = new Allors.Workspace.Adapters.Local.ReadTransport(executor, "Default", identity);
            var arguments = new Dictionary<string, object> { ["name"] = "c1B" };
            var inline = new Allors.Protocol.Json.Api.Read.ReadRequest
            {
                v = 1, i = "owned-inline", k = "read",
                p = new Allors.Protocol.Json.Api.Read.ReadPull
                {
                    l = new[] { new Pull { e = extent, a = arguments, r = new[] { new Allors.Protocol.Json.Data.Result { s = select, n = "matches" } } } },
                    d = System.Array.Empty<PullDependency>(),
                },
            };
            var captured = transport.CaptureRequest(inline);
            var before = Assert.IsType<Allors.Protocol.Json.Api.Read.ReadSuccess>(await transport.Read(captured));
            Assert.Single(before.c["matches"]);
            Assert.True(before.p.Length > 1);

            extent.t = "changed-after-capture";
            extent.p.r = "changed-after-capture";
            select.i[0].r = "changed-after-capture";
            select.n = new Allors.Protocol.Json.Data.Select { r = "changed-after-capture" };
            arguments["name"] = "changed-after-capture";
            var after = Assert.IsType<Allors.Protocol.Json.Api.Read.ReadSuccess>(await transport.Read(captured));
            Assert.NotSame(before, after);
            Assert.Equal(System.Text.Json.JsonSerializer.Serialize(before), System.Text.Json.JsonSerializer.Serialize(after));
        }

        [Theory]
        [InlineData("contradictory-selection")]
        [InlineData("wrong-select-owner")]
        [InlineData("missing-predicate-object")]
        public void CompleteReadRejectsUnadmittedInlineExecution(string invalid)
        {
            this.Transaction.Rollback();
            var population = ((Allors.Database.Adapters.Sql.Database)this.Transaction.Database).PopulationLifecycle.Renew();
            var user = this.SetUser("jane@example.com");
            var request = new Allors.Protocol.Json.Api.Read.ReadRequest
            {
                v = 1, i = "inline-failure", k = "read",
                p = new Allors.Protocol.Json.Api.Read.ReadPull
                {
                    l = new[] { new Pull { e = new Allors.Protocol.Json.Data.Extent { k = Allors.Protocol.Json.Data.ExtentKind.Filter, t = this.M.C1.Tag } } },
                    d = System.Array.Empty<PullDependency>(),
                },
            };
            switch (invalid)
            {
                case "contradictory-selection": request.p.l[0].o = user.Id; break;
                case "wrong-select-owner": request.p.l[0].r = new[] { new Allors.Protocol.Json.Data.Result { s = new Allors.Protocol.Json.Data.Select { r = this.M.User.UserName.RelationType.Tag } } }; break;
                case "missing-predicate-object": request.p.l[0].e.p = new Allors.Protocol.Json.Data.Predicate { k = Allors.Protocol.Json.Data.PredicateKind.Equals, ob = 9007199254740991 }; break;
            }

            var executor = new ReadExecutor(this.Transaction.Database,
                new ReadConfiguration(new Dictionary<string, string> { ["Default"] = "revision-1" }));
            var raw = System.Text.Json.JsonSerializer.Serialize(request);
            var response = Assert.IsType<Allors.Protocol.Json.Api.Read.ReadFailure>(ReadJson.DeserializeResponse(
                executor.Execute(raw, "Default", new LocalReadIdentity(population.Incarnation, user.Id)), request));
            Assert.NotEmpty(Assert.Single(response.e).m);
        }

        [Fact]
        public async void InlineExtentWithArguments()
        {
            this.SetUser("jane@example.com");

            var pullRequest = new PullRequest
            {
                l = new[]
                {
                    new Pull
                    {
                        e = new Allors.Protocol.Json.Data.Extent
                        {
                            k = Allors.Protocol.Json.Data.ExtentKind.Filter,
                            t = this.M.Organisation.Tag,
                            p = new Allors.Protocol.Json.Data.Predicate { k = Allors.Protocol.Json.Data.PredicateKind.Equals, r = this.M.Organisation.Name.RelationType.Tag, p = "name" },
                        },
                        a = new Dictionary<string, object> { ["name"] = "Acme" },
                    },
                },
            };

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var pullResponse = api.Pull(pullRequest);

            var organisations = pullResponse.c["Organisations"];

            Assert.Single(organisations);
        }

        [Fact]
        public async void NamedResult()
        {
            var user = this.SetUser("jane@example.com");

            var data = new DataBuilder(this.Transaction).WithString("First").Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            var pull = new Allors.Database.Data.Pull
            {
                Extent = new Extent(this.M.Data),
                Results = new[]
                {
                    new  Result { Name = "Datas" },
                }
            };

            var pullRequest = new PullRequest
            {
                l = new[]
                {
                    pull.ToJson(this.UnitConvert)
                },
            };

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var pullResponse = api.Pull(pullRequest);

            var namedCollection = pullResponse.c["Datas"];

            Assert.Single(namedCollection);

            var namedObject = namedCollection.First();

            Assert.Equal(data.Id, namedObject);

            var objects = pullResponse.p;

            Assert.Single(objects);

            var @object = objects[0];

            var acls = new DatabaseAccessControl(this.Security, user);
            var acl = acls[data];

            Assert.NotNull(@object);

            Assert.Equal(data.Strategy.ObjectId, @object.i);
            Assert.Equal(data.Strategy.ObjectVersion, @object.v);
            Assert.Equal(acl.Grants.Select(v => v.Id), @object.g);
        }

        [Fact]
        public async void IncludeRoleOne2One()
        {
            var user = this.SetUser("jane@example.com");

            this.Transaction.Derive();
            this.Transaction.Commit();

            var pull = new Allors.Database.Data.Pull
            {
                Extent = new Extent(this.M.C1)
                {
                    Predicate = new Equals(this.M.C1.Name) { Value = "c1B" }
                },
                Results = new[]
                {
                    new  Result
                    {
                        Include = new []
                        {
                            new Node(this.M.C1.C1C2One2One)
                        }
                    },
                }
            };

            var pullRequest = new PullRequest
            {
                l = new[]
                {
                    pull.ToJson(this.UnitConvert)
                },
            };

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var pullResponse = api.Pull(pullRequest);

            var pool = pullResponse.p;

            Assert.Equal(2, pool.Length);

            var c1b = new C1s(this.Transaction).Extent().First(v => "c1B".Equals(v.Name));

            Assert.Contains(pool, v => v.i == c1b.Id);
            Assert.Contains(pool, v => v.i == c1b.C1C2One2One.Id);
        }

        [Fact]
        public async void IncludeAssociationOne2One()
        {
            var user = this.SetUser("jane@example.com");

            this.Transaction.Derive();
            this.Transaction.Commit();

            var pull = new Allors.Database.Data.Pull
            {
                Extent = new Extent(this.M.C2)
                {
                    Predicate = new Equals(this.M.C2.Name) { Value = "c2B" }
                },
                Results = new[]
                {
                    new  Result
                    {
                        Include = new []
                        {
                            new Node(this.M.C2.C1WhereC1C2One2One)
                        }
                    },
                }
            };

            var pullRequest = new PullRequest
            {
                l = new[]
                {
                    pull.ToJson(this.UnitConvert)
                },
            };

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var pullResponse = api.Pull(pullRequest);

            var pool = pullResponse.p;

            Assert.Equal(2, pool.Length);

            var c2b = new C2s(this.Transaction).Extent().First(v => "c2B".Equals(v.Name));

            Assert.Contains(pool, v => v.i == c2b.Id);
            Assert.Contains(pool, v => v.i == c2b.C1WhereC1C2One2One.Id);
        }

        [Fact]
        public async void SelectRoleOne2OneIncludeRoleOne2One()
        {
            var user = this.SetUser("jane@example.com");

            this.Transaction.Derive();
            this.Transaction.Commit();

            var pull = new Allors.Database.Data.Pull
            {
                Extent = new Extent(this.M.C1)
                {
                    Predicate = new Equals(this.M.C1.Name) { Value = "c1B" }
                },
                Results = new[]
                {
                    new  Result
                    {
                        Select = new Select
                        {
                            PropertyType = this.M.C1.C1C2One2One,
                            Include = new[]
                            {
                                new Node(this.M.C2.C2C2One2One)
                            }
                        }
                    },
                }
            };

            var pullRequest = new PullRequest
            {
                l = new[]
                {
                    pull.ToJson(this.UnitConvert)
                },
            };

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var pullResponse = api.Pull(pullRequest);

            var pool = pullResponse.p;

            Assert.Equal(2, pool.Length);

            var c1b = new C1s(this.Transaction).Extent().First(v => "c1B".Equals(v.Name));

            Assert.Contains(pool, v => v.i == c1b.C1C2One2One.Id);
            Assert.Contains(pool, v => v.i == c1b.C1C2One2One.C2C2One2One.Id);
        }

        [Fact]
        public async void SelectAssociationOne2OneIncludeAssociationOne2One()
        {
            var user = this.SetUser("jane@example.com");

            this.Transaction.Derive();
            this.Transaction.Commit();

            var pull = new Allors.Database.Data.Pull
            {
                Extent = new Extent(this.M.C2)
                {
                    Predicate = new Equals(this.M.C2.Name) { Value = "c2B" }
                },
                Results = new[]
                {
                    new  Result
                    {
                        Select = new Select
                        {
                            PropertyType = this.M.C2.C1WhereC1C2One2One,
                            Include = new[]
                            {
                                new Node(this.M.C1.C1WhereC1C1One2One)
                            }
                        }
                    },
                }
            };

            var pullRequest = new PullRequest
            {
                l = new[]
                {
                    pull.ToJson(this.UnitConvert)
                },
            };

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var pullResponse = api.Pull(pullRequest);

            var pool = pullResponse.p;

            Assert.Equal(2, pool.Length);

            var c2b = new C2s(this.Transaction).Extent().First(v => "c2B".Equals(v.Name));

            Assert.Contains(pool, v => v.i == c2b.C1WhereC1C2One2One.Id);
            Assert.Contains(pool, v => v.i == c2b.C1WhereC1C2One2One.C1WhereC1C1One2One.Id);
        }
    }
}
