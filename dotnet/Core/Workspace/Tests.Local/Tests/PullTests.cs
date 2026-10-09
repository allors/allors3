// <copyright file="PullTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Local
{
    using System;
    using System.Threading.Tasks;
    using Allors.Database;
    using Allors.Database.Domain;
    using Allors.Database.Protocol.Json;
    using Allors.Database.Services;
    using Allors.Database.Tracing;
    using Allors.Workspace.Data;
    using Xunit;

    public class PullTests : Workspace.PullTests, IClassFixture<Fixture>
    {
        public PullTests(Fixture fixture) : base(fixture) => this.Profile = new Profile(fixture);

        public override IProfile Profile { get; }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task JsonApiPullDisposesItsTransaction(bool throwAfterPull)
        {
            await this.Login("administrator");
            var profile = (Profile)this.Profile;
            var sink = new PullSink(throwAfterPull);
            profile.Database.Sink = sink;
            var session = this.Workspace.CreateSession();
            var pull = new Pull { Extent = new Filter(this.M.C1) };

            try
            {
                if (throwAfterPull)
                {
                    var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => session.PullAsync(pull));
                    Assert.Equal("Test pull failure", exception.Message);
                }
                else
                {
                    var result = await session.PullAsync(pull);
                    Assert.False(result.HasErrors);
                    Assert.NotEmpty(result.GetCollection<Allors.Workspace.Domain.C1>());
                }

                Assert.NotNull(sink.PullEvent);
                Assert.NotNull(sink.PullEvent.PullResponse);
                Assert.Equal(profile.DatabaseConnection.UserId, sink.UserId);
                // The sink leaves a new object in the request transaction. Disposal must roll it
                // back on both success and failure, rather than committing it with the response.
                Assert.Null(sink.PullEvent.Transaction.Instantiate(sink.UncommittedId));
                using var transaction = profile.Database.CreateTransaction();
                Assert.Null(transaction.Instantiate(sink.UncommittedId));
            }
            finally
            {
                profile.Database.Sink = null;
            }
        }

        private sealed class PullSink(bool throwAfterPull) : ISink
        {
            public PullEvent PullEvent { get; private set; }

            public long UserId { get; private set; }

            public long UncommittedId { get; private set; }

            public void OnBefore(IEvent @event)
            {
                if (@event is PullEvent pullEvent)
                {
                    this.PullEvent = pullEvent;
                    this.UserId = pullEvent.Transaction.Services.Get<IUserService>().User.Id;
                }
            }

            public void OnAfter(IEvent @event)
            {
                if (@event is PullEvent pullEvent)
                {
                    this.UncommittedId = pullEvent.Transaction.Create<C1>().Id;
                    if (throwAfterPull)
                    {
                        throw new InvalidOperationException("Test pull failure");
                    }
                }
            }
        }
    }
}
