// <copyright file="ParametrizedTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the ApplicationTests type.
// </summary>

namespace Allors.Database.Domain.Tests
{
    using System.Collections.Generic;
    using Allors.Database.Data;
    using Xunit;

    public class ParametrizedTests : DomainTest, IClassFixture<Fixture>
    {
        public ParametrizedTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void EqualsWithParameters()
        {
            var filter = new Extent(this.M.Person.ObjectType)
            {
                Predicate = new Equals { PropertyType = this.M.Person.FirstName, Parameter = "firstName" },
            };

            var arguments = new Dictionary<string, string> { { "firstName", "John" } };
            var queryExtent = filter.Build(this.Transaction, arguments);

            var extent = this.Transaction.Extent(this.M.Person.ObjectType);
            extent.Filter.AddEquals(this.M.Person.FirstName, "John");

            Assert.Equal(extent.ToArray(), queryExtent.ToArray());
        }

        [Fact]
        public void EqualsWithDependencies()
        {
            var filter = new Extent(this.M.Person.ObjectType)
            {
                Predicate = new Equals { Dependencies = new[] { "useFirstname" }, PropertyType = this.M.Person.FirstName, Value = "John" },
            };

            var arguments = new Dictionary<string, string>();
            var queryExtent = filter.Build(this.Transaction, arguments);

            var extent = this.Transaction.Extent(this.M.Person.ObjectType);

            Assert.Equal(extent.ToArray(), queryExtent.ToArray());

            arguments.Add("useFirstname", "x");
            queryExtent = filter.Build(this.Transaction, arguments);

            extent = this.Transaction.Extent(this.M.Person.ObjectType);
            extent.Filter.AddEquals(this.M.Person.FirstName, "John");

            Assert.Equal(extent.ToArray(), queryExtent.ToArray());
        }

        [Fact]
        public void EqualsWithoutParameters()
        {
            var filter = new Extent(this.M.Person.ObjectType)
            {
                Predicate = new Equals { PropertyType = this.M.Person.FirstName, Parameter = "firstName" },
            };

            var queryExtent = filter.Build(this.Transaction);

            var extent = this.Transaction.Extent(this.M.Person.ObjectType);

            Assert.Equal(extent.ToArray(), queryExtent.ToArray());
        }

        [Fact]
        public void AndWithParameters()
        {
            // select from Person where FirstName='John' and LastName='Doe'
            var filter = new Extent(this.M.Person.ObjectType)
            {
                Predicate = new And
                {
                    Operands = new IPredicate[]
                                {
                                    new Equals
                                        {
                                            PropertyType = this.M.Person.FirstName,
                                            Parameter = "firstName",
                                        },
                                    new Equals
                                        {
                                            PropertyType = this.M.Person.LastName,
                                            Parameter = "lastName"
                                        },
                                },
                },
            };

            var arguments = new Dictionary<string, string>
                                {
                                    { "firstName", "John" },
                                    { "lastName", "Doe" },
                                };
            var queryExtent = filter.Build(this.Transaction, arguments);

            var extent = this.Transaction.Extent(this.M.Person.ObjectType);
            var and = extent.Filter.AddAnd();
            and.AddEquals(this.M.Person.FirstName, "John");
            and.AddEquals(this.M.Person.LastName, "Doe");

            Assert.Equal(extent.ToArray(), queryExtent.ToArray());
        }

        [Fact]
        public void AndWithoutParameters()
        {
            // select from Person where FirstName='John' and LastName='Doe'
            var filter = new Extent(this.M.Person.ObjectType)
            {
                Predicate = new And
                {
                    Operands = new IPredicate[]
                        {
                            new Equals
                                {
                                    PropertyType = this.M.Person.FirstName,
                                    Parameter = "firstName",
                                },
                            new Equals
                                {
                                    PropertyType = this.M.Person.LastName,
                                    Parameter = "lastName"
                                },
                        },
                },
            };
            {
                var arguments = new Dictionary<string, string>
                                    {
                                        { "firstName", "John" },
                                    };
                var queryExtent = filter.Build(this.Transaction, arguments);

                var extent = this.Transaction.Extent(this.M.Person.ObjectType);
                extent.Filter.AddEquals(this.M.Person.FirstName, "John");

                Assert.Equal(extent.ToArray(), queryExtent.ToArray());
            }

            {
                var queryExtent = filter.Build(this.Transaction);

                var extent = this.Transaction.Extent(this.M.Person.ObjectType);

                Assert.Equal(extent.ToArray(), queryExtent.ToArray());
            }
        }

        [Fact]
        public void NestedWithParameters()
        {
            var filter = new Extent(this.M.C1.ObjectType)
            {
                Predicate = new ContainedIn
                {
                    PropertyType = this.M.C1.C1C2One2One,
                    Extent = new Extent(this.M.C2.ObjectType)
                    {
                        Predicate = new Equals
                        {
                            PropertyType = this.M.C2.C2AllorsString,
                            Parameter = "nested",
                        },
                    },
                },
            };

            var arguments = new Dictionary<string, string> { { "nested", "c2B" } };
            var queryExtent = filter.Build(this.Transaction, arguments);

            var c2s = this.Transaction.Extent(this.M.C2.ObjectType);
            c2s.Filter.AddEquals(this.M.C2.C2AllorsString, "c2B");
            var extent = this.Transaction.Extent(this.M.C1.ObjectType);
            extent.Filter.AddContainedIn(this.M.C1.C1C2One2One, c2s);

            Assert.Equal(extent.ToArray(), queryExtent.ToArray());
        }

        [Fact]
        public void NestedWithoutParameters()
        {
            var filter = new Extent(this.M.C1.ObjectType)
            {
                Predicate = new ContainedIn
                {
                    PropertyType = this.M.C1.C1C2One2One,
                    Extent = new Extent(this.M.C2.ObjectType)
                    {
                        Predicate = new Equals
                        {
                            PropertyType = this.M.C2.C2AllorsString,
                            Parameter = "nested",
                        },
                    },
                },
            };

            var arguments = new Dictionary<string, string>();
            var queryExtent = filter.Build(this.Transaction, arguments);

            var extent = this.Transaction.Extent(this.M.C1.ObjectType);

            Assert.Equal(extent.ToArray(), queryExtent.ToArray());
        }

        [Fact]
        public void AndNestedContainedInWithoutParameters()
        {
            var filter = new Extent(this.M.C1.ObjectType)
            {
                Predicate = new And
                {
                    Operands = new IPredicate[]
                    {
                        new ContainedIn
                        {
                            PropertyType = this.M.C1.C1C2One2One,
                            Extent = new Extent(this.M.C2.ObjectType)
                            {
                                Predicate = new Equals
                                {
                                    PropertyType = this.M.C2.C2AllorsString,
                                    Parameter = "nested",
                                },
                            },
                        },
                    },
                },
            };

            var parameters = new Dictionary<string, string>();
            var queryExtent = filter.Build(this.Transaction, parameters);

            var extent = this.Transaction.Extent(this.M.C1.ObjectType);

            Assert.Equal(extent.ToArray(), queryExtent.ToArray());
        }

        [Fact]
        public void AndNestedContainsWithoutParameters()
        {
            var filter = new Extent(this.M.C1.ObjectType)
            {
                Predicate = new And
                {
                    Operands = new IPredicate[]
                    {
                        new ContainedIn
                            {
                                PropertyType = this.M.C1.C1C2One2One,
                                Extent = new Extent(this.M.C2.ObjectType)
                                             {
                                                 Predicate = new Contains
                                                                 {
                                                                     PropertyType = this.M.C2.C1WhereC1C2One2One,
                                                                     Parameter = "nested",
                                                                 },
                                             },
                            },
                    },
                },
            };

            var arguments = new Dictionary<string, string>();
            var queryExtent = filter.Build(this.Transaction, arguments);

            var extent = this.Transaction.Extent(this.M.C1.ObjectType);

            Assert.Equal(extent.ToArray(), queryExtent.ToArray());
        }
    }
}
