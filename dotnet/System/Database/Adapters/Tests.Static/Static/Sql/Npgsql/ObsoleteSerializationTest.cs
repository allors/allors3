// <copyright file="SerializationTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Npgsql
{
    using System;
    using System.IO;
    using System.Xml;
    using Domain;
    using global::Npgsql;
    using Moq;
    using Xunit;
    using Adapters;

    public class ObsoleteSerializationTest : Adapters.ObsoleteSerializationTest, IClassFixture<Fixture<ObsoleteSerializationTest>>
    {
        private readonly Profile profile;

        public ObsoleteSerializationTest() => this.profile = new Profile(this.GetType().Name);

        protected override IProfile Profile => this.profile;

        public override void Dispose() => this.profile.Dispose();

        protected override IDatabase CreatePopulation() => this.profile.CreateMemoryDatabase();

        [Fact]
        public void PopulationLifecycleLoadRenewsIdentityAndOmitsItFromXml()
        {
            var database = (Database)this.profile.CreateDatabase();
            database.Init();
            using (var setup = database.CreateTransaction())
            {
                setup.Create<C1>().C1AllorsString = "Saved";
                setup.Commit();
            }

            database.PopulationLifecycle.Renew();
            var original = database.PopulationLifecycle.RequireReady().Incarnation;
            var xml = SavePopulation(database);
            Assert.DoesNotContain(original.ToString(), xml);
            Assert.DoesNotContain("incarnation", xml, StringComparison.OrdinalIgnoreCase);
            for (var attempt = 0; attempt < 2; attempt++)
            {
                var before = database.PopulationLifecycle.Read().Incarnation;
                using var owner = database.PopulationLifecycle.BeginMaintenance();
                using var reader = XmlReader.Create(new StringReader(xml));
                database.Load(reader);
                Assert.Equal(PopulationState.Maintenance, database.PopulationLifecycle.Read().State);
                owner.Complete();
                Assert.NotEqual(before, database.PopulationLifecycle.RequireReady().Incarnation);
                Assert.Equal(xml, SavePopulation(database));
            }
        }

        [Fact]
        public void PopulationLifecycleLoadCallbacksObserveMaintenanceAndFailureStaysUnavailable()
        {
            var database = (Database)this.profile.CreateDatabase();
            database.Init();
            database.PopulationLifecycle.Renew();
            var before = database.PopulationLifecycle.RequireReady().Incarnation;
            var callback = new InvalidOperationException("Expected import callback failure");
            database.ObjectNotLoaded += (_, _) =>
            {
                Assert.NotEqual(before, database.PopulationLifecycle.Read().Incarnation);
                Assert.Equal(PopulationState.Maintenance, database.PopulationLifecycle.Read().State);
                throw callback;
            };

            using var owner = database.PopulationLifecycle.BeginMaintenance();
            using var reader = XmlReader.Create(new StringReader("<allors><population version=\"1\"><objects><database><ot i=\"71000000000000000000000000000000\">1:0</ot></database></objects></population></allors>"));
            Assert.Same(callback, Assert.Throws<InvalidOperationException>(() => database.Load(reader)));
            Assert.Throws<InvalidOperationException>(() => owner.Complete());
            Assert.Equal(PopulationState.Maintenance, database.PopulationLifecycle.Read().State);
            using var verification = database.CreateTransaction();
            Assert.Empty(verification.Extent<C1>());
        }

        [Theory]
        [InlineData("<allors />")]
        [InlineData("<unrelated />")]
        public void PopulationLifecycleLoadRequiresPopulationRoot(string xml)
        {
            var database = (Database)this.profile.CreateDatabase();
            database.Init();
            using var owner = database.PopulationLifecycle.BeginMaintenance();
            using var reader = XmlReader.Create(new StringReader(xml));
            Assert.Throws<ArgumentException>(() => database.Load(reader));
            Assert.Throws<InvalidOperationException>(() => owner.Complete());
            Assert.Equal(PopulationState.Maintenance, database.PopulationLifecycle.Read().State);
        }

        [Fact]
        public void PopulationLifecycleDirectLoaderCannotPublishReady()
        {
            var database = (Database)this.profile.CreateDatabase();
            database.Init();
            database.PopulationLifecycle.Renew();
            var before = database.PopulationLifecycle.RequireReady().Incarnation;
            using var connection = new NpgsqlConnection(database.ConnectionString);
            connection.Open();
            using var reader = XmlReader.Create(new StringReader("<allors><population version=\"1\" /></allors>"));
            new Load(database, connection, null, null).Execute(reader);
            Assert.NotEqual(before, database.PopulationLifecycle.Read().Incarnation);
            Assert.Equal(PopulationState.Maintenance, database.PopulationLifecycle.Read().State);
            Assert.Throws<InvalidOperationException>(() => database.PopulationLifecycle.RequireReady());
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void PopulationLifecycleRelationFailureCannotComplete(bool composite)
        {
            var database = (Database)this.profile.CreateDatabase();
            database.Init();
            string xml;
            using (var setup = database.CreateTransaction())
            {
                var value = setup.Create<C1>();
                if (composite)
                {
                    value.C1C2many2one = setup.Create<C2>();
                }
                else
                {
                    value.C1AllorsString = "Relation failure";
                }

                setup.Commit();
            }

            xml = SavePopulation(database);
            var connection = new Mock<IConnection>();
            connection.Setup(v => v.CreateCommand()).Throws(new InvalidOperationException("Injected relation command failure"));
            var factory = new Mock<IConnectionFactory>();
            factory.Setup(v => v.Create()).Returns(connection.Object);
            database.ConnectionFactory = factory.Object;
            using var owner = database.PopulationLifecycle.BeginMaintenance();
            using var reader = XmlReader.Create(new StringReader(xml));
            database.Load(reader);
            connection.Verify(v => v.Rollback(), Times.AtLeastOnce());
            Assert.Throws<InvalidOperationException>(() => owner.Complete());
            Assert.Equal(PopulationState.Maintenance, database.PopulationLifecycle.Read().State);
        }

        private static string SavePopulation(Database database)
        {
            using var output = new StringWriter();
            using (var writer = XmlWriter.Create(output))
            {
                database.Save(writer);
            }

            return output.ToString();
        }

        [Fact]
        public void PopulationLifecycleEmptyPopulationRequiresValidTrailingXml()
        {
            var database = (Database)this.profile.CreateDatabase();
            database.Init();
            using var owner = database.PopulationLifecycle.BeginMaintenance();
            using var reader = XmlReader.Create(new StringReader("<allors><population version=\"1\" /></different>"));
            Assert.Throws<XmlException>(() => database.Load(reader));
            Assert.Throws<InvalidOperationException>(() => owner.Complete());
            Assert.Equal(PopulationState.Maintenance, database.PopulationLifecycle.Read().State);
        }
    }
}
