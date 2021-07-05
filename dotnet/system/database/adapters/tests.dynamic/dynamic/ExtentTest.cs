// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExtentTest.cs" company="Allors bvba">
//   Copyright 2002-2012 Allors bvba.
// 
// Dual Licensed under
//   a) the Lesser General Public Licence v3 (LGPL)
//   b) the Allors License
// 
// The LGPL License is included in the file lgpl.txt.
// The Allors License is an addendum to your contract.
// 
// Allors Platform is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// For more information visit http://www.allors.com/legal
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Allors.Database.Adapters
{
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Xunit;

    public abstract class ExtentTest : Test
    {
        [Fact]
        [Trait("Category", "Dynamic")]
        public void ObjectTypes()
        {
            var objectsByObjectType = new Dictionary<IObjectType, List<IObject>>();

            const int Max = 1;
            foreach (var concreteCompositeType in this.GetMetaPopulation().Classes)
            {
                var objects = new List<IObject>();
                for (var i = 0; i < Max; i++)
                {
                    objects.Add(this.GetTransaction().Create(concreteCompositeType));
                }

                objectsByObjectType[concreteCompositeType] = objects;
            }

            foreach (var objectType in this.GetMetaPopulation().Composites)
            {
                if (!objectType.IsClass)
                {
                    var objects = new List<IObject>();

                    foreach (var concreteCompositeType in objectType.Classes)
                    {
                        objects.AddRange(objectsByObjectType[concreteCompositeType]);
                    }

                    objectsByObjectType[objectType] = objects;
                }
            }

            foreach (var objectType in this.GetMetaPopulation().Composites)
            {
                if (objectType.Classes.Count() > 0)
                {
                    object[] extent = this.GetTransaction().Extent(objectType);
                    var objects = objectsByObjectType[objectType];

                    Assert.Equal(objects.Count(), extent.Length);
                    foreach (object extentObject in extent)
                    {
                        Assert.Contains(extentObject, objects);
                    }
                }
            }
        }

        [Fact]
        [Trait("Category", "Dynamic")]
        public void AddEquals()
        {
            foreach (var concreteClass in this.GetTestTypes())
            {
                var extent = this.GetTransaction().Extent(concreteClass);

                foreach (var role in concreteClass.DatabaseRoleTypes)
                {
                    if (role.ObjectType.IsUnit)
                    {
                        //TODO:
                    }
                    else if (role.IsOne)
                    {
                        foreach (var concreteType in ((Composite)role.ObjectType).Classes)
                        {
                            var roleObject = this.GetTransaction().Create(concreteType);
                            extent.Filter.AddEquals(role, roleObject);
                        }
                    }
                }

                foreach (var association in concreteClass.DatabaseAssociationTypes)
                {
                    if (association.IsOne)
                    {
                        foreach (var concreteType in association.ObjectType.DatabaseClasses)
                        {
                            var associationObject = this.GetTransaction().Create(concreteType);
                            extent.Filter.AddEquals(association, associationObject);
                        }
                    }
                }

                int count = extent.Count;
            }
        }

        [Fact]
        [Trait("Category", "Dynamic")]
        public void AddExist()
        {
            foreach (var concreteClass in this.GetTestTypes())
            {
                foreach (var role in concreteClass.DatabaseRoleTypes)
                {
                    var extent = this.GetTransaction().Extent(concreteClass);
                    extent.Filter.AddExists(role);

                    foreach (var association in concreteClass.DatabaseAssociationTypes)
                    {
                        extent.Filter.AddExists(association);
                    }

                    int count = extent.Count;
                }
            }
        }

        private IClass[] GetTestTypes() => this.GetMetaPopulation().Classes.ToArray();
    }
}
