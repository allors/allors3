// <copyright file="SandboxTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters
{
    using System;
    using Data;
    using Moq;
    using Xunit;
    using C1 = Domain.C1;
    using Extent = Data.Extent;
    using User = Domain.User;

    public abstract class SandboxTest : IDisposable
    {
        protected ITransaction Transaction => this.Profile.Transaction;

        protected Action[] Markers => this.Profile.Markers;

        protected Action[] Inits => this.Profile.Inits;

        protected abstract IProfile Profile { get; }

        protected IDatabase Population => this.Profile.Database;

        public abstract void Dispose();

        [Fact]
        public void SqlReservedWords()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var user = User.Create(this.Transaction);
                user.From = "Nowhere";
                Assert.Equal("Nowhere", user.From);

                this.Transaction.Commit();

                user = (User)this.Transaction.Instantiate(user.Id);
                Assert.Equal("Nowhere", user.From);
            }
        }

        [Fact]
        public void MultipleInits()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                if (this.Population is IDatabase database)
                {
                    for (var i = 0; i < 100; i++)
                    {
                        database.Init();
                    }
                }
            }
        }

        [Fact]
        public void DeleteAssociations()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var c1A = C1.Create(this.Transaction);
                var c1B = C1.Create(this.Transaction);
                c1A.C1C1many2one = c1B;

                foreach (var c in c1B.C1sWhereC1C1many2one)
                {
                    c.Strategy.Delete();
                }

                Assert.False(c1B.ExistC1sWhereC1C1many2one);
            }
        }

        [Fact]
        public void EqualsWithParameter()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var population = new TestPopulation(this.Transaction);

                var extent = new Extent(m.C1)
                {
                    Predicate = new Equals(m.C1.C1AllorsString) { Parameter = "pString" },
                };

                var arguments = new Mock<IArguments>();
                arguments.Setup(arg => arg.HasArgument("pString")).Returns(true);
                arguments.Setup(arg => arg.ResolveUnit(UnitTags.String, "pString")).Returns("ᴀbra");
                var objects = this.Transaction.Resolve<C1>(extent, arguments.Object);

                Assert.Single(objects);
            }
        }

        [Fact]
        public void EqualsMissingParameter()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var population = new TestPopulation(this.Transaction);

                var extent = new Extent(m.C1)
                {
                    Predicate = new Equals(m.C1.C1AllorsString) { Parameter = "pString" },
                };

                var objects = this.Transaction.Resolve<C1>(extent);

                Assert.Equal(4, objects.Length);
            }
        }

        // TODO: Move to Protocol
        //[Fact]
        //public void LoadExtent()
        //{
        //    foreach (var init in this.Inits)
        //    {
        //        init();
        //        var m = this.Transaction.Database.State().M;
        //        var population = new TestPopulation(this.Transaction);

        //        var schemaExtent = new Protocol.Data.Extent
        //        {
        //            Kind = Protocol.Data.ExtentKind.Extent,
        //            ObjectType = m.C1.ObjectType.Id,
        //            Predicate = new Predicate
        //            {
        //                Kind = Protocol.Data.PredicateKind.Equals,
        //                RoleType = m.C1.C1AllorsString.RelationType.Id,
        //                Value = "ᴀbra",
        //            },
        //        };

        //        var extent = schemaExtent.Import(this.Transaction);

        //        var objects = this.Transaction.Resolve<C1>(extent);

        //        Assert.Single(objects);
        //    }
        //}

        // TODO: Move to Protocol
        //[Fact]
        //public void SaveExtent()
        //{
        //    foreach (var init in this.Inits)
        //    {
        //        init();
        //        var m = this.Transaction.Database.State().M;
        //        var population = new TestPopulation(this.Transaction);

        //        var extent = new Extent(m.C1.ObjectType)
        //        {
        //            Predicate = new Equals(m.C1.C1AllorsString) { Parameter = "pString" },
        //        };

        //        var schemaExtent = extent.Save();

        //        Assert.NotNull(schemaExtent);

        //        Assert.Equal(ExtentKind.Extent, schemaExtent.Kind);

        //        var predicate = schemaExtent.Predicate;

        //        Assert.NotNull(predicate);
        //        Assert.Equal(PredicateKind.Equals, predicate.Kind);
        //        Assert.Equal("pString", predicate.Parameter);
        //    }
        //}

        [Fact]
        public void ScratchPad()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;
                var population = new TestPopulation(this.Transaction);

                this.Profile.Transaction.Commit();

            }
        }
    }
}
