// <copyright file="PullTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//
// </summary>

namespace Tests.Workspace
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using System.Threading;
    using System.Threading.Tasks;
    using Allors;
    using Allors.Workspace.Data;
    using Allors.Workspace.Meta;
    using Xunit;
    using static Names;
    using C1 = Allors.Workspace.Domain.C1;
    using C2 = Allors.Workspace.Domain.C2;
    using I12 = Allors.Workspace.Domain.I12;
    using I2 = Allors.Workspace.Domain.I2;
    using UnitSample = Allors.Workspace.Domain.UnitSample;

    public abstract class PullTests : Test
    {
        protected PullTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public async Task PullUsesFinalSelectionAndCompletesExactCacheHits()
        {
            var requests = new List<string>();
            var ids = new[] { 1L, 2L };
            var binary = new byte[] { 1, 2 };
            var dependencies = new[] { "original" };
            var arguments = new Dictionary<string, object> { ["value"] = "original", ["ids"] = ids, ["binary"] = binary };
            JsonNode originalRequest = null;
            var workspace = this.Profile.CreateWorkspace(new PullHandler((path, body) =>
            {
                requests.Add(path);
                if (path == "pull")
                {
                    if (requests.Count == 1)
                    {
                        originalRequest = body.DeepClone();
                        Assert.Equal("original", body["l"][0]["a"]["value"].GetValue<string>());
                        Assert.Single(body["d"].AsArray());
                        Assert.Equal(this.M.C1.C1C1Many2One.RelationType.Tag, body["d"][0]["r"].GetValue<string>());
                        arguments["value"] = "changed";
                        dependencies[0] = "changed";
                        ids[0] = 99;
                        binary[0] = 99;
                    }

                    return Task.FromResult(requests.Count == 1
                        ? this.PullSnapshot(new[] { 1L }, "initial", 2)
                        : this.PullSnapshot(new[] { 3L, 2L, 3L }, "final", 3));
                }

                Assert.Equal("sync", path);
                Assert.NotNull(body["pull"]);
                Assert.Single(body["pull"]["l"].AsArray());
                Assert.True(JsonNode.DeepEquals(originalRequest, body["pull"]));
                return Task.FromResult(this.SyncSnapshot(new[] { 3L, 2L, 3L }, "final", 3));
            }));
            var session = workspace.CreateSession();
            session.OnChange += (_, _) =>
            {
                Assert.NotNull(session.Instantiate<C1>(2));
                Assert.NotNull(session.Instantiate<C1>(3));
            };
            session.Activate(new[] { new PullDependencyRule(this.M) });
            var pull = new Pull
            {
                Extent = new Filter(this.M.C1)
                {
                    Predicate = new Equals(this.M.C1.C1AllorsString) { Parameter = "value", Dependencies = dependencies },
                },
                Arguments = arguments,
            };
            var result = await session.PullAsync(pull);

            Assert.False(result.HasErrors);
            Assert.Equal(new[] { 3L, 2L, 3L }, result.GetCollection<C1>("selection").Select(v => v.Id));
            Assert.Equal(3, result.GetObject<C1>("selected").Id);
            Assert.Equal("final", result.GetValue<string>("value"));
            Assert.Throws<InvalidOperationException>(() => session.Instantiate<C1>(1));

            result = await session.PullAsync(pull);
            Assert.False(result.HasErrors);
            Assert.Equal(new[] { 3L, 2L, 3L }, result.GetCollection<C1>("selection").Select(v => v.Id));
            Assert.Equal(new[] { "pull", "sync", "pull" }, requests);
        }

        [Theory]
        [InlineData("object")]
        [InlineData("grant")]
        [InlineData("revocation")]
        [InlineData("permission")]
        [InlineData("result")]
        [InlineData("role")]
        [InlineData("create")]
        [InlineData("transport")]
        [InlineData("unit")]
        [InlineData("to-one")]
        [InlineData("duplicate-grant")]
        [InlineData("duplicate-permission")]
        [InlineData("boolean-string")]
        [InlineData("integer-string")]
        [InlineData("float-string")]
        [InlineData("decimal-number")]
        [InlineData("decimal-overflow")]
        [InlineData("decimal-precision")]
        [InlineData("binary")]
        [InlineData("unique")]
        [InlineData("date")]
        [InlineData("date-only")]
        [InlineData("date-hour")]
        [InlineData("object-refresh")]
        [InlineData("transport-refresh")]
        [InlineData("class-after-push")]
        public async Task PullRejectsIncompleteResponsesAtomically(string missing)
        {
            var refresh = missing.EndsWith("-refresh", StringComparison.Ordinal);
            var pushed = missing == "class-after-push";
            var failure = missing.Replace("-refresh", string.Empty);
            var warmed = false;
            var workspace = this.Profile.CreateWorkspace(new PullHandler((path, body) =>
            {
                if (path == "push")
                {
                    return Task.FromResult(new JsonObject
                    {
                        ["n"] = new JsonArray(new JsonObject { ["w"] = body["n"][0]["w"].GetValue<long>(), ["d"] = 1 }),
                    });
                }

                if (path == "pull")
                {
                    return Task.FromResult(this.PullSnapshot(new[] { 1L, 2L }, "initial", warmed ? 3 : 2));
                }

                Assert.Equal("sync", path);
                if (refresh && !warmed)
                {
                    warmed = true;
                    return Task.FromResult(this.SyncSnapshot(new[] { 1L, 2L }, "cached", 2));
                }

                if (failure == "transport")
                {
                    throw new OperationCanceledException("Simulated interrupted Sync");
                }

                var response = this.SyncSnapshot(new[] { 1L, 2L }, "final", warmed ? 3 : 2);
                void SetInvalidUnit(IRoleType role, object value) => response["o"][1]["ro"][0] =
                    new JsonObject { ["t"] = role.RelationType.Tag, ["v"] = JsonSerializer.SerializeToNode(value) };
                switch (failure)
                {
                    case "object": response["o"][1]["ro"] = null; break;
                    case "grant": response["g"][0]["p"] = null; break;
                    case "revocation": response["r"][0]["p"] = null; break;
                    case "permission": response["p"] = new JsonArray(); break;
                    case "result": response["result"]["c"]["selection"] = new JsonArray(1, 99); break;
                    case "role": response["o"][1]["ro"][0]["t"] = "unknown-role"; break;
                    case "create": response["p"][1]["t"] = this.M.C1.C1AllorsString.RelationType.Tag; break;
                    case "unit": response["o"][1]["ro"][0]["v"] = true; break;
                    case "to-one":
                        response["o"][1]["ro"][0] = new JsonObject { ["t"] = this.M.C1.C1C1Many2One.RelationType.Tag, ["o"] = -1 };
                        break;
                    case "duplicate-grant": response["g"].AsArray().Add(response["g"][0].DeepClone()); break;
                    case "duplicate-permission": response["p"].AsArray().Add(response["p"][0].DeepClone()); break;
                    case "boolean-string": SetInvalidUnit(this.M.C1.C1AllorsBoolean, "true"); break;
                    case "integer-string": SetInvalidUnit(this.M.C1.C1AllorsInteger, "12"); break;
                    case "float-string": SetInvalidUnit(this.M.C1.C1AllorsDouble, "12.5"); break;
                    case "decimal-number": SetInvalidUnit(this.M.C1.C1AllorsDecimal, 12.5); break;
                    case "decimal-overflow": SetInvalidUnit(this.M.C1.C1AllorsDecimal, "79228162514264337593543950335.1"); break;
                    case "decimal-precision": SetInvalidUnit(this.M.C1.C1AllorsDecimal, "0.12345678901234567890123456789"); break;
                    case "binary": SetInvalidUnit(this.M.C1.C1AllorsBinary, "not-base64"); break;
                    case "unique": SetInvalidUnit(this.M.C1.C1AllorsUnique, "2946CF3771BE46818FE6D0024D59BEFF"); break;
                    case "date": SetInvalidUnit(this.M.C1.C1AllorsDateTime, "2024-02-31T00:00:00Z"); break;
                    case "date-only": SetInvalidUnit(this.M.C1.C1AllorsDateTime, "2024-01-01"); break;
                    case "date-hour": SetInvalidUnit(this.M.C1.C1AllorsDateTime, "2024-01-01T24:00:00Z"); break;
                    case "class-after-push":
                        response["o"][0]["c"] = this.M.C2.Tag;
                        response["o"][0]["ro"] = new JsonArray();
                        break;
                }

                return Task.FromResult(response);
            }));
            var session = workspace.CreateSession();
            C1 existing = null;
            if (pushed)
            {
                existing = session.Create<C1>();
                existing.C1AllorsString = "local";
                Assert.False((await session.PushAsync()).HasErrors);
                Assert.Equal(1, existing.Id);
                Assert.Empty(this.Profile.CachedVersions(workspace, 1));
            }
            else if (refresh)
            {
                Assert.False((await session.PullAsync(new Pull { ObjectId = 1 })).HasErrors);
                existing = session.Instantiate<C1>(1);
            }

            await Assert.ThrowsAnyAsync<Exception>(() => session.PullAsync(new Pull { ObjectId = 1 }));
            if (pushed)
            {
                Assert.Same(existing, session.Instantiate<C1>(1));
                Assert.Equal("local", existing.C1AllorsString);
                Assert.Equal(Allors.Version.WorkspaceInitial.Value, existing.Strategy.Version);
                Assert.Throws<InvalidOperationException>(() => session.Instantiate<C1>(2));
                Assert.Empty(this.Profile.CachedVersions(workspace, 1));
                Assert.Empty(this.Profile.CachedVersions(workspace, 2));
            }
            else if (refresh)
            {
                Assert.Same(existing, session.Instantiate<C1>(1));
                Assert.Equal("cached", existing.C1AllorsString);
                Assert.Equal("cached", session.Instantiate<C1>(2).C1AllorsString);
                Assert.Equal(2, existing.Strategy.Version);
                Assert.Equal(new[] { 2L }, this.Profile.CachedVersions(workspace, 1));
                Assert.Equal(new[] { 2L }, this.Profile.CachedVersions(workspace, 2));
            }
            else
            {
                Assert.Throws<InvalidOperationException>(() => session.Instantiate<C1>(1));
                Assert.Throws<InvalidOperationException>(() => session.Instantiate<C1>(2));
                Assert.Empty(this.Profile.CachedVersions(workspace, 1));
                Assert.Empty(this.Profile.CachedVersions(workspace, 2));
            }
        }

        [Fact]
        public async Task PullRetainsAdvertisedRecordsAcrossOverlappingSessions()
        {
            var delayed = new TaskCompletionSource<JsonObject>(TaskCreationOptions.RunContinuationsAsynchronously);
            var advertised = new TaskCompletionSource<JsonObject>(TaskCreationOptions.RunContinuationsAsynchronously);
            var pulls = 0;
            var workspace = this.Profile.CreateWorkspace(new PullHandler((path, body) =>
            {
                if (path == "pull")
                {
                    ++pulls;
                    return Task.FromResult(this.PullSnapshot(pulls == 2 ? new[] { 1L, 2L } : new[] { 1L }, "initial", pulls == 3 ? 3 : 2));
                }

                Assert.Equal("sync", path);
                if (pulls == 2)
                {
                    advertised.SetResult(body);
                    return delayed.Task;
                }

                return Task.FromResult(this.SyncSnapshot(new[] { 1L }, pulls == 3 ? "newer" : "cached", pulls == 3 ? 3 : 2));
            }));
            var first = workspace.CreateSession();
            var current = workspace.CreateSession();
            await first.PullAsync(new Pull { ObjectId = 1 });
            var pending = first.PullAsync(new Pull { ObjectId = 1 });
            var request = await advertised.Task;
            Assert.Single(request["o"].AsArray());
            Assert.Equal(1, request["o"][0]["i"].GetValue<long>());
            Assert.Single(request["g"].AsArray());
            await current.PullAsync(new Pull { ObjectId = 1 });

            var response = this.SyncSnapshot(new[] { 1L, 2L }, "retained", 2);
            response["o"][0]["ro"] = null;
            response["o"][1]["ro"] = new JsonArray();
            response["g"][0]["p"] = null;
            response["r"][0]["p"] = null;
            response["p"] = new JsonArray();
            delayed.SetResult(response);
            Assert.False((await pending).HasErrors);
            Assert.Equal("cached", first.Instantiate<C1>(1).C1AllorsString);
            Assert.Null(first.Instantiate<C1>(2).C1AllorsString);
            Assert.Equal("newer", current.Instantiate<C1>(1).C1AllorsString);
            Assert.Equal(new[] { 2L, 3L }, this.Profile.CachedVersions(workspace, 1));

            await first.PullAsync(new Pull { ObjectId = 1 });
            Assert.Equal("cached", first.Instantiate<C1>(1).C1AllorsString);
            Assert.Equal(new[] { 2L, 3L }, this.Profile.CachedVersions(workspace, 1));
        }

        [Fact]
        public async Task PullFinalAbsencePreservesExistingSessionObjects()
        {
            var pulls = 0;
            var workspace = this.Profile.CreateWorkspace(new PullHandler((path, body) =>
            {
                if (path == "pull")
                {
                    ++pulls;
                    return Task.FromResult(this.PullSnapshot(pulls == 1 ? new[] { 1L } : new[] { 1L, 2L }, "initial", 2));
                }

                Assert.Equal("sync", path);
                return Task.FromResult(this.SyncSnapshot(pulls == 1 ? new[] { 1L } : Array.Empty<long>(), "final", 2));
            }));
            var session = workspace.CreateSession();
            await session.PullAsync(new Pull { ObjectId = 1 });
            var existing = session.Instantiate<C1>(1);
            var result = await session.PullAsync(new Pull { ObjectId = 1 });
            Assert.False(result.HasErrors);
            Assert.Null(result.GetObject<C1>("selected"));
            Assert.Empty(result.GetCollection<C1>("selection"));
            Assert.Same(existing, session.Instantiate<C1>(1));
            Assert.Equal("final", existing.C1AllorsString);
            Assert.Throws<InvalidOperationException>(() => session.Instantiate<C1>(2));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task PullRequiresContentsWhenOnlySecurityChanges(bool omitObject)
        {
            var pulls = 0;
            var synchronizations = 0;
            var workspace = this.Profile.CreateWorkspace(new PullHandler((path, body) =>
            {
                if (path == "pull")
                {
                    ++pulls;
                    var response = this.PullSnapshot(new[] { 1L }, "initial", 2);
                    response["g"][0][1] = pulls;
                    return Task.FromResult(response);
                }

                Assert.Equal("sync", path);
                ++synchronizations;
                var synced = this.SyncSnapshot(new[] { 1L }, "cached", 2);
                if (pulls == 2)
                {
                    Assert.Empty(body["o"].AsArray());
                    synced["g"][0]["v"] = 2;
                    synced["g"][0]["p"] = new JsonArray();
                    synced["r"][0]["p"] = null;
                    synced["o"][0]["ro"] = omitObject ? null : new JsonArray();
                    synced["p"] = new JsonArray();
                }

                return Task.FromResult(synced);
            }));
            var first = workspace.CreateSession();
            var second = workspace.CreateSession();
            await first.PullAsync(new Pull { ObjectId = 1 });
            if (omitObject)
            {
                await Assert.ThrowsAsync<InvalidOperationException>(() => second.PullAsync(new Pull { ObjectId = 1 }));
                Assert.Throws<InvalidOperationException>(() => second.Instantiate<C1>(1));
            }
            else
            {
                Assert.False((await second.PullAsync(new Pull { ObjectId = 1 })).HasErrors);
                Assert.False(second.Instantiate<C1>(1).CanReadC1AllorsString);
                Assert.Null(second.Instantiate<C1>(1).C1AllorsString);
            }

            Assert.Equal(2, synchronizations);
            Assert.True(first.Instantiate<C1>(1).CanReadC1AllorsString);
            Assert.Equal("cached", first.Instantiate<C1>(1).C1AllorsString);
        }

        private JsonObject PullSnapshot(long[] ids, string value, long version) => new JsonObject
        {
            ["o"] = new JsonObject { ["selected"] = ids.FirstOrDefault() },
            ["c"] = new JsonObject { ["selection"] = JsonSerializer.SerializeToNode(ids) },
            ["v"] = new JsonObject { ["value"] = value },
            ["p"] = JsonSerializer.SerializeToNode(ids.Distinct().Select(id => new { i = id, v = version, g = new[] { 10L }, r = new[] { 20L } })),
            ["g"] = new JsonArray(new JsonArray(10, 1)),
            ["r"] = new JsonArray(new JsonArray(20, 1)),
        };

        private JsonObject SyncSnapshot(long[] ids, string value, long version) => new JsonObject
        {
            ["result"] = new JsonObject
            {
                ["o"] = new JsonObject { ["selected"] = ids.FirstOrDefault() },
                ["c"] = new JsonObject { ["selection"] = JsonSerializer.SerializeToNode(ids) },
                ["v"] = new JsonObject { ["value"] = value },
            },
            ["o"] = JsonSerializer.SerializeToNode(ids.Distinct().Select(id => new
            {
                i = id, v = version, c = this.M.C1.Tag, g = new[] { 10L }, r = new[] { 20L },
                ro = new[] { new { t = this.M.C1.C1AllorsString.RelationType.Tag, v = value } },
            })),
            ["g"] = JsonSerializer.SerializeToNode(new[] { new { i = 10L, v = 1L, p = new[] { 100L, 101L } } }),
            ["r"] = JsonSerializer.SerializeToNode(new[] { new { i = 20L, v = 1L, p = new[] { 102L } } }),
            ["p"] = JsonSerializer.SerializeToNode(new[]
            {
                new { i = 100L, c = this.M.C1.Tag, t = this.M.C1.C1AllorsString.RelationType.Tag, o = (int)Operations.Read },
                new { i = 101L, c = this.M.C1.Tag, t = (string)null, o = (int)Operations.Create },
                new { i = 102L, c = this.M.C1.Tag, t = this.M.C1.C1AllorsString.RelationType.Tag, o = (int)Operations.Write },
            }),
        };

        private sealed class PullHandler : HttpMessageHandler
        {
            private readonly Func<string, JsonObject, Task<JsonObject>> respond;

            internal PullHandler(Func<string, JsonObject, Task<JsonObject>> respond) => this.respond = respond;

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var body = JsonNode.Parse(await request.Content.ReadAsStringAsync(cancellationToken)).AsObject();
                var response = await this.respond(request.RequestUri.Segments.Last(), body);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(response.ToJsonString(), Encoding.UTF8, "application/json"),
                };
            }
        }

        private sealed class PullDependencyRule : Allors.Workspace.Domain.PersonSessionFullNameRule
        {
            internal PullDependencyRule(M m) : base(m) => this.Dependencies = new IDependency[]
            {
                new Dependency(m.C1, m.C1.C1C1Many2One),
                new Dependency(m.C1, m.C1.C1AllorsString),
            };
        }

        [Fact]
        public async Task UnitSamplesWithNulls()
        {
            await this.Login("administrator");
            var session = this.Workspace.CreateSession();

            var id = await this.Profile.SetupUnitSample(0);
            var result = await session.PullAsync(new Pull { ObjectId = id });

            Assert.False(result.HasErrors);

            var unitSample = result.GetObject<UnitSample>();

            Assert.False(unitSample.ExistAllorsBinary);
            Assert.False(unitSample.ExistAllorsBoolean);
            Assert.False(unitSample.ExistAllorsDateTime);
            Assert.False(unitSample.ExistAllorsDecimal);
            Assert.False(unitSample.ExistAllorsDouble);
            Assert.False(unitSample.ExistAllorsInteger);
            Assert.False(unitSample.ExistAllorsString);
            Assert.False(unitSample.ExistAllorsUnique);
        }

        [Fact]
        public async Task UnitSamplesWithValues()
        {
            await this.Login("administrator");
            var session = this.Workspace.CreateSession();

            var id = await this.Profile.SetupUnitSample(1);
            var result = await session.PullAsync(new Pull { ObjectId = id });

            Assert.False(result.HasErrors);

            var unitSample = result.GetObject<UnitSample>();

            Assert.True(unitSample.ExistAllorsBinary);
            Assert.True(unitSample.ExistAllorsBoolean);
            Assert.True(unitSample.ExistAllorsDateTime);
            Assert.True(unitSample.ExistAllorsDecimal);
            Assert.True(unitSample.ExistAllorsDouble);
            Assert.True(unitSample.ExistAllorsInteger);
            Assert.True(unitSample.ExistAllorsString);
            Assert.True(unitSample.ExistAllorsUnique);

            Assert.Equal(new byte[] { 1, 2, 3 }, unitSample.AllorsBinary);
            Assert.True(unitSample.AllorsBoolean);
            Assert.Equal(new System.DateTime(1973, 3, 27, 0, 0, 0, DateTimeKind.Utc), unitSample.AllorsDateTime);
            Assert.Equal(12.34m, unitSample.AllorsDecimal);
            Assert.Equal(123d, unitSample.AllorsDouble);
            Assert.Equal(1000, unitSample.AllorsInteger);
            Assert.Equal("a string", unitSample.AllorsString);
            Assert.Equal(new Guid("2946CF37-71BE-4681-8FE6-D0024D59BEFF"), unitSample.AllorsUnique);
        }

        [Fact]
        public async void AndGreaterThanLessThan()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            // Class
            var pull = new Pull
            {
                Extent = new Filter(this.M.C1)
                {
                    Predicate = new And
                    {
                        Operands = new IPredicate[]
                        {
                            new GreaterThan(m.C1.C1AllorsInteger){Value = 0},
                            new LessThan(m.C1.C1AllorsInteger){Value = 2}
                        }
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B);

            // Interface
            pull = new Pull
            {
                Extent = new Filter(this.M.I12)
                {
                    Predicate = new And
                    {
                        Operands = new IPredicate[]
                        {
                            new GreaterThan(m.I12.I12AllorsInteger){Value = 0},
                            new LessThan(m.I12.I12AllorsInteger){Value = 2}
                        }
                    }
                }
            };

            result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<I12>().Equal(c1B, c2B);
        }

        [Fact]
        public async void AssociationMany2ManyContainedIn()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            // Empty
            var pull = new Pull
            {
                Extent = new Filter(this.M.C2)
                {
                    Predicate = new ContainedIn(m.C2.C1sWhereC1C2Many2Many)
                    {
                        Extent = new Filter(this.M.C1)
                        {
                            Predicate = new Equals(m.C1.C1AllorsString) { Value = "Nothing here!" }
                        }
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Empty(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            // Full
            pull = new Pull
            {
                Extent = new Filter(this.M.C2)
                {
                    Predicate = new ContainedIn(m.C2.C1sWhereC1C2Many2Many)
                    {
                        Extent = new Filter(this.M.C1)
                    }
                }
            };

            result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C2>().Equal(c2B, c2C, c2D);

            // Filtered
            pull = new Pull
            {
                Extent = new Filter(this.M.C2)
                {
                    Predicate = new ContainedIn(m.C2.C1sWhereC1C2Many2Many)
                    {
                        Extent = new Filter(this.M.C1)
                        {
                            Predicate = new Equals(m.C1.C1AllorsString) { Value = "ᴀbra" }
                        }
                    }
                }
            };

            result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C2>().Equal(c2B);
        }

        [Fact]
        public async void AssociationMany2ManyContainedInObjects()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            // The "ᴀbra" C1 objects, passed explicitly (the by-Objects form of ContainedIn,
            // as opposed to the by-Extent form exercised by AssociationMany2ManyContainedIn).
            var c1Result = await session.PullAsync(new Pull
            {
                Extent = new Filter(this.M.C1)
                {
                    Predicate = new Equals(m.C1.C1AllorsString) { Value = "ᴀbra" }
                }
            });
            var abraC1s = c1Result.GetCollection<C1>();

            var pull = new Pull
            {
                Extent = new Filter(this.M.C2)
                {
                    Predicate = new ContainedIn(m.C2.C1sWhereC1C2Many2Many) { Objects = abraC1s }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C2>().Equal(c2B);
        }

        [Fact]
        public async void AssociationMany2ManyContains()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var c1c = await session.PullObject<C1>(c1C);

            // Full
            var pull = new Pull
            {
                Extent = new Filter(this.M.C2)
                {
                    Predicate = new Contains(m.C2.C1sWhereC1C2Many2Many)
                    {
                        Object = c1c
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C2>().Equal(c2B, c2C);
        }

        [Fact]
        public async void AssociationMany2ManyExist()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            // Full
            var pull = new Pull
            {
                Extent = new Filter(this.M.C2)
                {
                    Predicate = new Exists(m.C2.C1sWhereC1C2Many2Many)
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C2>().Equal(c2B, c2C, c2D);
        }

        [Fact]
        public async void AssociationMany2OneContainedIn()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(this.M.C2)
                {
                    Predicate = new ContainedIn(m.C2.C1sWhereC1C2Many2One)
                    {
                        Extent = new Filter(this.M.C1)
                        {
                            Predicate = new Equals(m.C1.C1AllorsString) { Value = "ᴀbra" }
                        }
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C2>().Equal(c2B);
        }

        [Fact]
        public async void AssociationMany2OneContains()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var c1c = await session.PullObject<C1>(c1C);

            // Full
            var pull = new Pull
            {
                Extent = new Filter(this.M.C2)
                {
                    Predicate = new Contains(m.C2.C1sWhereC1C2Many2One)
                    {
                        Object = c1c
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C2>().Equal(c2C);
        }

        [Fact]
        public async void AssociationOne2ManyContainedIn()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(this.M.C2)
                {
                    Predicate = new ContainedIn(m.C2.C1WhereC1C2One2Many)
                    {
                        Extent = new Filter(this.M.C1)
                        {
                            Predicate = new Equals(m.C1.C1AllorsString) { Value = "ᴀbra" }
                        }
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C2>().Equal(c2B);
        }

        [Fact]
        public async void AssociationOne2ManyEquals()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var c1b = await session.PullObject<C1>(c1B);
            var c1c = await session.PullObject<C1>(c1C);

            var pull = new Pull
            {
                Extent = new Filter(this.M.C2)
                {
                    Predicate = new Equals(m.C2.C1WhereC1C2One2Many)
                    {
                        Object = c1b
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C2>().Equal(c2B);

            pull = new Pull
            {
                Extent = new Filter(this.M.C2)
                {
                    Predicate = new Equals(m.C2.C1WhereC1C2One2Many)
                    {
                        Object = c1c
                    }
                }
            };

            result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C2>().Equal(c2C, c2D);
        }

        [Fact]
        public async void AssociationOne2ManyExists()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            // Class
            var pull = new Pull
            {
                Extent = new Filter(this.M.C2)
                {
                    Predicate = new Exists(m.C2.C1WhereC1C2One2Many)
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C2>().Equal(c2B, c2C, c2D);

            // Interface
            pull = new Pull
            {
                Extent = new Filter(this.M.I2)
                {
                    Predicate = new Exists(m.I2.I1WhereI1I2One2Many)
                }
            };

            result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<I2>().Equal(c2B, c2C, c2D);
        }

        [Fact]
        public async void AssociationOne2ManyInstanceOf()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(this.M.C2)
                {
                    Predicate = new Instanceof(m.C2.C1WhereC1C2One2Many) { ObjectType = m.C1 }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C2>().Equal(c2B, c2C, c2D);
        }

        [Fact]
        public async void AssociationOne2OneContainedIn()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(this.M.C2)
                {
                    Predicate = new ContainedIn(m.C2.C1WhereC1C2One2One)
                    {
                        Extent = new Filter(this.M.C1)
                        {
                            Predicate = new Equals(m.C1.C1AllorsString) { Value = "ᴀbra" }
                        }
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C2>().Equal(c2B);
        }

        [Fact]
        public async void AssociationOne2OneEquals()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var c1b = await session.PullObject<C1>(c1B);
            var c1c = await session.PullObject<C1>(c1C);

            var pull = new Pull
            {
                Extent = new Filter(this.M.C2)
                {
                    Predicate = new Equals(m.C2.C1WhereC1C2One2One)
                    {
                        Object = c1b
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C2>().Equal(c2B);

            pull = new Pull
            {
                Extent = new Filter(this.M.C2)
                {
                    Predicate = new Equals(m.C2.C1WhereC1C2One2One)
                    {
                        Object = c1c
                    }
                }
            };

            result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C2>().Equal(c2C);
        }

        [Fact]
        public async void AssociationOne2OneExists()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;


            var pull = new Pull
            {
                Extent = new Filter(this.M.C1)
                {
                    Predicate = new Exists(m.C1.C1WhereC1C1One2One)
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B, c1C, c1D);

            pull = new Pull
            {
                Extent = new Filter(this.M.C2)
                {
                    Predicate = new Exists(m.C2.C1WhereC1C2One2One)
                }
            };

            result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C2>().Equal(c2B, c2C, c2D);
        }

        [Fact]
        public async void AssociationOne2OneInstanceOf()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(this.M.I12)
                {
                    Predicate = new Instanceof(m.I12.I12WhereI12I12One2One) { ObjectType = m.C1 }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<I12>().Equal(c1D, c2B, c2C);

            pull = new Pull
            {
                Extent = new Filter(this.M.I12)
                {
                    Predicate = new Instanceof(m.I12.I12WhereI12I12One2One) { ObjectType = m.I2 }
                }
            };

            result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<I12>().Equal(c1B, c1C, c2D);
        }

        [Fact]
        public async void ObjectEquals()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var c1c = await session.PullObject<C1>(c1C);

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new Equals { Object = c1c }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1C);
        }

        [Fact]
        public async void ExtentInterface()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.I12)
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<I12>().Equal(c1A, c1B, c1C, c1D, c2A, c2B, c2C, c2D);
        }

        [Fact]
        public async void InstanceOf()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.I12)
                {
                    Predicate = new Instanceof
                    {
                        ObjectType = m.C1
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<I12>().Equal(c1A, c1B, c1C, c1D);
        }

        [Fact]
        public async void NotEquals()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var c1c = await session.PullObject<C1>(c1C);

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new Not
                    {
                        Operand = new Equals { Object = c1c }
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1A, c1B, c1D);
        }

        [Fact]
        public async void OrEquals()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var c1b = await session.PullObject<C1>(c1B);
            var c1c = await session.PullObject<C1>(c1C);

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new Or
                    {
                        Operands = new[]
                        {
                            new Equals { Object = c1b },
                            new Equals { Object = c1c }
                        }
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B, c1C);
        }

        [Fact]
        public async void OperatorExcept()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Except
                {
                    Operands = new Extent[]
                    {
                        new Filter(m.I12),
                        new Filter(m.I12)
                        {
                            Predicate = new Instanceof{ObjectType = m.C2}
                        }
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<I12>().Equal(c1A, c1B, c1C, c1D);
        }

        [Fact]
        public async void OperatorIntersect()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Intersect()
                {
                    Operands = new Extent[]
                    {
                        new Filter(m.I12),
                        new Filter(m.I12)
                        {
                            Predicate = new Instanceof{ObjectType = m.C2}
                        }
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<I12>().Equal(c2A, c2B, c2C, c2D);
        }

        [Fact]
        public async void OperatorUnion()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Union
                {
                    Operands = new Extent[]
                    {
                        new Filter(m.C1){Predicate = new Equals(m.C1.Name) {Value = "c1A"}},
                        new Filter(m.C1){Predicate = new Equals(m.C1.Name) {Value = "c1B"}}
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1A, c1B);
        }

        [Fact]
        public async void RoleDateTimeBetweenPath()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new Between(m.C1.C1AllorsDateTime)
                    {
                        Paths = new IRoleType[] { m.C1.C1DateTimeBetweenA, m.C1.C1DateTimeBetweenB }
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1C, c1D);
        }

        [Fact]
        public async void RoleDateTimeBetweenValue()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new Between(m.C1.C1AllorsDateTime)
                    {
                        Values = new object[]
                        {
                            new System.DateTime(2000, 1, 1, 0, 0, 4, DateTimeKind.Utc),
                            new System.DateTime(2000, 1, 1, 0, 0, 6, DateTimeKind.Utc)
                        }
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B, c1C, c1D);
        }

        [Fact]
        public async void RoleDateTimeGreaterThanPath()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new GreaterThan(m.C1.C1AllorsDateTime)
                    {
                        Path = m.C1.C1DateTimeGreaterThan
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B);
        }

        [Fact]
        public async void RoleDateTimeGreaterThanValue()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new GreaterThan(m.C1.C1AllorsDateTime)
                    {
                        Value = new System.DateTime(2000, 1, 1, 0, 0, 4, DateTimeKind.Utc)
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1C, c1D);
        }

        [Fact]
        public async void RoleDateTimeLessThanPath()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new LessThan(m.C1.C1AllorsDateTime)
                    {
                        Path = m.C1.C1DateTimeLessThan
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1D);
        }

        [Fact]
        public async void RoleDateTimeLessThanValue()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new LessThan(m.C1.C1AllorsDateTime)
                    {
                        Value = new System.DateTime(2000, 1, 1, 0, 0, 5, DateTimeKind.Utc)
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B);
        }

        [Fact]
        public async void RoleDateTimeEquals()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new Equals(m.C1.C1AllorsDateTime)
                    {
                        Value = new System.DateTime(2000, 1, 1, 0, 0, 4, DateTimeKind.Utc)
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B);
        }

        [Fact]
        public async void RoleDecimalBetweenPath()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new Between(m.C1.C1AllorsDecimal)
                    {
                        Paths = new IRoleType[] { m.C1.C1DecimalBetweenA, m.C1.C1DecimalBetweenB }
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1D);
        }

        [Fact]
        public async void RoleDecimalBetweenValue()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new Between(m.C1.C1AllorsDecimal)
                    {
                        Values = new object[] { 2.1m, 2.3m }
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1C, c1D);
        }

        [Fact]
        public async void RoleDecimalGreaterThanPath()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new GreaterThan(m.C1.C1AllorsDecimal)
                    {
                        Path = m.C1.C1DecimalGreaterThan
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B, c1C);
        }

        [Fact]
        public async void RoleDecimalGreaterThanValue()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new GreaterThan(m.C1.C1AllorsDecimal)
                    {
                        Value = 1.5m
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1C, c1D);
        }

        [Fact]
        public async void RoleDecimalLessThanPath()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new LessThan(m.C1.C1AllorsDecimal)
                    {
                        Path = m.C1.C1DecimalLessThan
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1D);
        }

        [Fact]
        public async void RoleDecimalLessThanValue()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new LessThan(m.C1.C1AllorsDecimal)
                    {
                        Value = 1.9m
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B);
        }

        [Fact]
        public async void RoleDecimalEquals()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new Equals(m.C1.C1AllorsDecimal)
                    {
                        Value = 2.2m
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1C, c1D);
        }

        [Fact]
        public async void RoleDoubleBetweenPath()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new Between(m.C1.C1AllorsDouble)
                    {
                        Paths = new IRoleType[] { m.C1.C1DoubleBetweenA, m.C1.C1DoubleBetweenB }
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1D);
        }

        [Fact]
        public async void RoleDoubleBetweenValue()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new Between(m.C1.C1AllorsDouble)
                    {
                        Values = new object[] { 2.1d, 2.3d }
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1C, c1D);
        }

        [Fact]
        public async void RoleDoubleGreaterThanPath()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new GreaterThan(m.C1.C1AllorsDouble)
                    {
                        Path = m.C1.C1DoubleGreaterThan
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B, c1C);
        }

        [Fact]
        public async void RoleDoubleGreaterThanValue()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new GreaterThan(m.C1.C1AllorsDouble)
                    {
                        Value = 1.5d
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1C, c1D);
        }

        [Fact]
        public async void RoleDoubleLessThanPath()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new LessThan(m.C1.C1AllorsDouble)
                    {
                        Path = m.C1.C1DoubleLessThan
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1D);
        }

        [Fact]
        public async void RoleDoubleLessThanValue()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new LessThan(m.C1.C1AllorsDouble)
                    {
                        Value = 1.9d
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B);
        }

        [Fact]
        public async void RoleDoubleEquals()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new Equals(m.C1.C1AllorsDouble)
                    {
                        Value = 2.2d
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1C, c1D);
        }

        [Fact]
        public async void RoleIntegerBetweenPath()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new Between(m.C1.C1AllorsInteger)
                    {
                        Paths = new IRoleType[] { m.C1.C1IntegerBetweenA, m.C1.C1IntegerBetweenB }
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1C, c1D);
        }

        [Fact]
        public async void RoleIntegerBetweenValue()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new Between(m.C1.C1AllorsInteger)
                    {
                        Values = new object[] { 1, 2 }
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B, c1C, c1D);
        }

        [Fact]
        public async void RoleIntegerGreaterThanPath()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new GreaterThan(m.C1.C1AllorsInteger)
                    {
                        Path = m.C1.C1IntegerGreaterThan
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B);
        }

        [Fact]
        public async void RoleIntegerGreaterThanValue()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new GreaterThan(m.C1.C1AllorsInteger)
                    {
                        Value = 1
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1C, c1D);
        }

        [Fact]
        public async void RoleIntegerLessThanPath()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new LessThan(m.C1.C1AllorsInteger)
                    {
                        Path = m.C1.C1IntegerLessThan
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1D);
        }

        [Fact]
        public async void RoleIntegerLessThanValue()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new LessThan(m.C1.C1AllorsInteger)
                    {
                        Value = 2
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B);
        }

        [Fact]
        public async void RoleIntegerEquals()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new Equals(m.C1.C1AllorsInteger)
                    {
                        Value = 2
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1C, c1D);
        }

        [Fact]
        public async void RoleIntegerExist()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new Exists(m.C1.C1AllorsInteger)
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B, c1C, c1D);
        }

        [Fact]
        public async void RoleStringEqualsPath()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new Equals(m.C1.C1AllorsString)
                    {
                        Path = m.C1.C1AllorsStringEquals
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1C);
        }

        [Fact]
        public async void RoleStringEqualsValue()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new Equals(m.C1.C1AllorsString)
                    {
                        Value = "ᴀbra"
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B);
        }

        [Fact]
        public async void RoleStringLike()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new Like(m.C1.C1AllorsString)
                    {
                        Value = "ᴀ%"
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B, c1C, c1D);
        }

        [Fact]
        public async void RoleUniqueEquals()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new Equals(m.C1.C1AllorsUnique)
                    {
                        Value = new Guid("8B3C4978-72D3-40BA-B302-114EB331FE04")
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B);
        }

        [Fact]
        public async void RoleMany2ManyContainedIn()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            // Empty
            var pull = new Pull
            {
                Extent = new Filter(this.M.C1)
                {
                    Predicate = new ContainedIn(m.C1.C1I12Many2Manies)
                    {
                        Extent = new Filter(this.M.I12)
                        {
                            Predicate = new Equals(m.I12.I12AllorsString) { Value = "Nothing here!" }
                        }
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Empty(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            // Full
            pull = new Pull
            {
                Extent = new Filter(this.M.C1)
                {
                    Predicate = new ContainedIn(m.C1.C1I12Many2Manies)
                    {
                        Extent = new Filter(this.M.I12)
                    }
                }
            };

            result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B, c1C, c1D);

            // Filtered
            pull = new Pull
            {
                Extent = new Filter(this.M.C1)
                {
                    Predicate = new ContainedIn(m.C1.C1I12Many2Manies)
                    {
                        Extent = new Filter(this.M.I12)
                        {
                            Predicate = new Equals(m.I12.I12AllorsString) { Value = "ᴀbra" }
                        }
                    }
                }
            };

            result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B, c1C, c1D);
        }

        [Fact]
        public async void RoleMany2ManyContains()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var c2c = await session.PullObject<C2>(c2C);

            var pull = new Pull
            {
                Extent = new Filter(this.M.C1)
                {
                    Predicate = new Contains(m.C1.C1C2Many2Manies)
                    {
                        Object = c2c
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1C, c1D);
        }

        [Fact]
        public async void RoleOne2ManyContainedIn()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(this.M.C1)
                {
                    Predicate = new ContainedIn(m.C1.C1I12One2Manies)
                    {
                        Extent = new Filter(this.M.I12)
                        {
                            Predicate = new Equals(m.I12.I12AllorsString) { Value = "ᴀbra" }
                        }
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B);
        }

        [Fact]
        public async void RoleOne2ManyContains()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var c2d = await session.PullObject<C2>(c2D);

            var pull = new Pull
            {
                Extent = new Filter(this.M.C1)
                {
                    Predicate = new Contains(m.C1.C1C2One2Manies)
                    {
                        Object = c2d
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1C);
        }

        [Fact]
        public async void RoleMany2OneContainedIn()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(this.M.C1)
                {
                    Predicate = new ContainedIn(m.C1.C1I12Many2One)
                    {
                        Extent = new Filter(this.M.I12)
                        {
                            Predicate = new Equals(m.I12.I12AllorsString) { Value = "ᴀbra" }
                        }
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B);
        }

        [Fact]
        public async void RoleOne2OneContainedIn()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(this.M.C1)
                {
                    Predicate = new ContainedIn(m.C1.C1I12One2One)
                    {
                        Extent = new Filter(this.M.I12)
                        {
                            Predicate = new Equals(m.I12.I12AllorsString) { Value = "ᴀbra" }
                        }
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>().Equal(c1B, c1C);
        }

        [Fact]
        public async void WithResultName()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var m = this.M;

            var pull = new Pull
            {
                Extent = new Filter(m.C1)
                {
                    Predicate = new Equals(m.C1.C1AllorsInteger)
                    {
                        Value = 2
                    }
                },
                Results = new[]{
                    new Result
                    {
                        Name = "IetsAnders",
                    }
                }
            };

            var result = await session.PullAsync(pull);

            Assert.Single(result.Collections);
            Assert.Empty(result.Objects);
            Assert.Empty(result.Values);

            result.Assert().Collection<C1>("IetsAnders").Equal(c1C, c1D);
        }

        [Fact]
        public async void PullWithObjectId()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();
            var pull1 = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.PullAsync(pull1);
            var c1a = result.GetCollection<C1>()[0];

            var pull2 = new Pull
            {
                ObjectId = c1a.Id
            };

            result = await session.PullAsync(pull2);

            Assert.Single(result.Objects);
            Assert.Empty(result.Collections);
            Assert.Empty(result.Values);
        }

        [Fact]
        public async void PullWithInclude()
        {
            await this.Login("administrator");
            var session = this.Workspace.CreateSession();

            var pull = new Pull
            {
                Extent = new Filter(this.M.C1),
                Results = new[]
                {
                    new Result
                    {
                        Select = new Select
                        {
                            Include = this.M.C1.Nodes(v=>v.C1C2One2One.Node())
                        }
                    }
                }
            };

            var result = await session.PullAsync(pull);

            var c1s = result.GetCollection<C1>();
            var c1b = c1s.Single(v => v.Name == "c1B");
            var c1c = c1s.Single(v => v.Name == "c1C");
            var c1d = c1s.Single(v => v.Name == "c1D");

            var c2ByC1 = c1s.ToDictionary(v => v, v => v.C1C2One2One);

            Assert.Equal("c2B", c2ByC1[c1b].Name);
            Assert.Equal("c2C", c2ByC1[c1c].Name);
            Assert.Equal("c2D", c2ByC1[c1d].Name);
        }

        [Fact]
        public async void SortDirectionDefault()
        {
            await this.Login("administrator");
            var session = this.Workspace.CreateSession();

            var pull = new Pull
            {
                Extent = new Filter(this.M.I12) { Sorting = new[] { new Sort(this.M.I12.Order) } },
            };

            var result = await session.PullAsync(pull);

            var i12s = result.GetCollection<I12>();

            Assert.Equal("c2D", i12s[0].Name);
            Assert.Equal("c2C", i12s[1].Name);
            Assert.Equal("c1B", i12s[2].Name);
            Assert.Equal("c1A", i12s[3].Name);
            Assert.Equal("c2A", i12s[4].Name);
            Assert.Equal("c2B", i12s[5].Name);
            Assert.Equal("c1D", i12s[6].Name);
            Assert.Equal("c1C", i12s[7].Name);
        }

        [Fact]
        public async void SortDirectionAscending()
        {
            await this.Login("administrator");
            var session = this.Workspace.CreateSession();

            var pull = new Pull
            {
                Extent = new Filter(this.M.I12) { Sorting = new[] { new Sort(this.M.I12.Order) { SortDirection = SortDirection.Ascending } } },
            };

            var result = await session.PullAsync(pull);

            var i12s = result.GetCollection<I12>();

            Assert.Equal("c2D", i12s[0].Name);
            Assert.Equal("c2C", i12s[1].Name);
            Assert.Equal("c1B", i12s[2].Name);
            Assert.Equal("c1A", i12s[3].Name);
            Assert.Equal("c2A", i12s[4].Name);
            Assert.Equal("c2B", i12s[5].Name);
            Assert.Equal("c1D", i12s[6].Name);
            Assert.Equal("c1C", i12s[7].Name);
        }


        [Fact]
        public async void SortDirectionDescending()
        {
            await this.Login("administrator");
            var session = this.Workspace.CreateSession();

            var pull = new Pull
            {
                Extent = new Filter(this.M.I12) { Sorting = new[] { new Sort(this.M.I12.Order) { SortDirection = SortDirection.Descending } } },
            };

            var result = await session.PullAsync(pull);

            var i12s = result.GetCollection<I12>();

            Assert.Equal("c2D", i12s[7].Name);
            Assert.Equal("c2C", i12s[6].Name);
            Assert.Equal("c1B", i12s[5].Name);
            Assert.Equal("c1A", i12s[4].Name);
            Assert.Equal("c2A", i12s[3].Name);
            Assert.Equal("c2B", i12s[2].Name);
            Assert.Equal("c1D", i12s[1].Name);
            Assert.Equal("c1C", i12s[0].Name);
        }
    }
}
