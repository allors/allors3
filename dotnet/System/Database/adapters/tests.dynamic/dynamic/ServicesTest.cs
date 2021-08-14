// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServicesTest.cs" company="Allors bvba">
//   Copyright 2002-2009 Allors bvba.
// Dual Licensed under
//   a) the Lesser General Public Licence v3 (LGPL)
//   b) the Allors License
// The LGPL License is included in the file lgpl.txt.
// The Allors License is an addendum to your contract.
// Allors Platform is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// For more information visit http://www.allors.com/legal
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Allors.Database.Adapters
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Linq;
    using Meta;
    using Xunit;
    using DateTime = System.DateTime;

    public abstract class ServicesTest : Test
    {
        private readonly bool[] manyFlags = { false, true };

        [Fact]
        [Trait("Category", "Dynamic")]
        public void CreateMany()
        {
            for (var iRepeat = 0; iRepeat < this.GetRepeats().Length; iRepeat++)
            {
                var repeat = this.GetRepeats()[iRepeat];
                for (var iTestRepeat = 0; iTestRepeat < this.GetTestRepeats().Length; iTestRepeat++)
                {
                    for (var iAssertRepeat = 0; iAssertRepeat < this.GetAssertRepeats().Length; iAssertRepeat++)
                    {
                        for (var transactionFlagIndex = 0; transactionFlagIndex < this.GetBooleanFlags().Length; transactionFlagIndex++)
                        {
                            var transactionFlag = this.GetBooleanFlags()[transactionFlagIndex];

                            for (var iTestType = 0; iTestType < this.GetTestTypes().Length; iTestType++)
                            {
                                var testType = this.GetTestTypes()[iTestType];

                                for (var objectCount = 1; objectCount < 100 * 10; objectCount = objectCount + 100)
                                {
                                    var allorsObjects = this.GetTransaction().Create(testType, objectCount);

                                    Assert.Equal(objectCount, allorsObjects.Count());

                                    this.Commit(transactionFlag);

                                    var ids = new ArrayList();
                                    for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                    {
                                        Assert.Equal(objectCount, allorsObjects.Count());
                                        for (var iAllorsType = 0; iAllorsType < objectCount; iAllorsType++)
                                        {
                                            var allorsObject = allorsObjects[iAllorsType];
                                            Assert.False(ids.Contains(allorsObject.Strategy.ObjectId.ToString()));
                                            ids.Add(allorsObject.Strategy.ObjectId.ToString());
                                        }

                                        this.Commit(transactionFlag);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (this.IsRollbackSupported())
            {
                for (var iRepeat = 0; iRepeat < this.GetRepeats().Length; iRepeat++)
                {
                    for (var iTestRepeat = 0; iTestRepeat < this.GetTestRepeats().Length; iTestRepeat++)
                    {
                        for (var iAssertRepeat = 0; iAssertRepeat < this.GetAssertRepeats().Length; iAssertRepeat++)
                        {
                            for (var iTestType = 0; iTestType < this.GetTestTypes().Length; iTestType++)
                            {
                                var testType = this.GetTestTypes()[iTestType];

                                for (var objectCount = 1; objectCount < 5; objectCount = objectCount + 1000)
                                {
                                    var allorsObjects = this.GetTransaction().Create(testType, objectCount);
                                    var ids = new string[objectCount];
                                    for (var i = 0; i < objectCount; i++)
                                    {
                                        var allorsObject = allorsObjects[i];
                                        ids[i] = allorsObject.Strategy.ObjectId.ToString();
                                    }

                                    Assert.Equal(objectCount, allorsObjects.Count());

                                    this.GetTransaction().Rollback();

                                    allorsObjects = this.GetTransaction().Instantiate(ids);

                                    Assert.Empty(allorsObjects);
                                }
                            }
                        }
                    }
                }
            }
        }

        [Fact]
        [Trait("Category", "Dynamic")]
        public void Delete()
        {
            for (var iRepeat = 0; iRepeat < this.GetRepeats().Length; iRepeat++)
            {
                var repeat = this.GetRepeats()[iRepeat];
                for (var iTestRepeat = 0; iTestRepeat < this.GetTestRepeats().Length; iTestRepeat++)
                {
                    for (var iAssertRepeat = 0; iAssertRepeat < this.GetAssertRepeats().Length; iAssertRepeat++)
                    {
                        for (var transactionFlagIndex = 0; transactionFlagIndex < this.GetBooleanFlags().Length; transactionFlagIndex++)
                        {
                            var transactionFlag = this.GetBooleanFlags()[transactionFlagIndex];

                            for (var secondTransactionFlagIndex = 0; secondTransactionFlagIndex < this.GetBooleanFlags().Length; secondTransactionFlagIndex++)
                            {
                                var secondTransactionFlag = this.GetBooleanFlags()[secondTransactionFlagIndex];
                                for (var thirdTransactionFlagIndex = 0; thirdTransactionFlagIndex < this.GetBooleanFlags().Length; thirdTransactionFlagIndex++)
                                {
                                    var thirdTransactionFlag = this.GetBooleanFlags()[thirdTransactionFlagIndex];

                                    for (var fourthTransactionFlagIndex = 0; fourthTransactionFlagIndex < this.GetBooleanFlags().Length; fourthTransactionFlagIndex++)
                                    {
                                        var fourthTransactionFlag = this.GetBooleanFlags()[fourthTransactionFlagIndex];

                                        for (var useRollbackFlagIndex = 0; useRollbackFlagIndex < this.GetBooleanFlags().Length; useRollbackFlagIndex++)
                                        {
                                            var useRollbackFlag = this.GetBooleanFlags()[useRollbackFlagIndex];
                                            if (!this.IsRollbackSupported())
                                            {
                                                useRollbackFlag = false;
                                            }

                                            for (var iTestType = 0; iTestType < this.GetTestTypes().Length; iTestType++)
                                            {
                                                var testType = this.GetTestTypes()[iTestType];

                                                var allorsObject = this.GetTransaction().Create(testType);
                                                for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                {
                                                    Assert.False(allorsObject.Strategy.IsDeleted);
                                                }

                                                this.Commit(secondTransactionFlag);
                                                allorsObject.Strategy.Delete();
                                                this.Commit(thirdTransactionFlag);

                                                for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                {
                                                    Assert.True(allorsObject.Strategy.IsDeleted);
                                                    if (useRollbackFlag)
                                                    {
                                                        this.Rollback(transactionFlag);
                                                    }
                                                    else
                                                    {
                                                        this.Commit(transactionFlag);
                                                    }
                                                }

                                                allorsObject = this.GetTransaction().Create(testType);
                                                string id = allorsObject.Strategy.ObjectId.ToString();
                                                this.Commit(secondTransactionFlag);
                                                allorsObject.Strategy.Delete();
                                                this.Commit(thirdTransactionFlag);
                                                allorsObject = this.GetTransaction().Instantiate(id);
                                                this.Commit(fourthTransactionFlag);

                                                for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                {
                                                    Assert.Null(allorsObject);
                                                    if (useRollbackFlag)
                                                    {
                                                        this.Rollback(transactionFlag);
                                                    }
                                                    else
                                                    {
                                                        this.Commit(transactionFlag);
                                                    }
                                                }

                                                IObject proxy = this.GetTransaction().Create(testType);
                                                id = proxy.Strategy.ObjectId.ToString();
                                                this.Commit(secondTransactionFlag);
                                                IObject subject = this.GetTransaction().Instantiate(id);
                                                subject.Strategy.Delete();
                                                this.Commit(thirdTransactionFlag);

                                                for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                {
                                                    Assert.True(proxy.Strategy.IsDeleted);
                                                    if (useRollbackFlag)
                                                    {
                                                        this.Rollback(transactionFlag);
                                                    }
                                                    else
                                                    {
                                                        this.Commit(transactionFlag);
                                                    }
                                                }

                                                allorsObject = this.GetTransaction().Create(testType);
                                                IObject[] beforeExtent = this.GetTransaction().Extent(testType);
                                                this.Commit(secondTransactionFlag);
                                                allorsObject.Strategy.Delete();
                                                this.Commit(thirdTransactionFlag);

                                                for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                {
                                                    IObject[] afterExtent = this.GetTransaction().Extent(testType);
                                                    Assert.Equal(beforeExtent.Count(), afterExtent.Count() + 1);
                                                    if (useRollbackFlag)
                                                    {
                                                        this.Rollback(transactionFlag);
                                                    }
                                                    else
                                                    {
                                                        this.Commit(transactionFlag);
                                                    }
                                                }

                                                // Units
                                                var testRoleTypes = this.GetUnitRoles(testType);
                                                var beforeValues = new Units(true);
                                                for (var testRoleTypeIndex = 0; testRoleTypeIndex < testRoleTypes.Count(); testRoleTypeIndex++)
                                                {
                                                    var testRoleType = testRoleTypes[testRoleTypeIndex];
                                                    for (var useCachingFlagIndex = 0; useCachingFlagIndex < this.GetBooleanFlags().Length; useCachingFlagIndex++)
                                                    {
                                                        bool useCachingFlag = this.GetBooleanFlags()[useCachingFlagIndex];

                                                        allorsObject = this.GetTransaction().Create(testType);
                                                        if (useCachingFlag)
                                                        {
                                                            try
                                                            {
                                                                this.GetUnit(allorsObject, testRoleType, Units.Dummy);
                                                            }
                                                            catch
                                                            {
                                                            }
                                                        }

                                                        this.SetUnit(allorsObject, testRoleType, beforeValues);
                                                        this.Commit(secondTransactionFlag);
                                                        allorsObject.Strategy.Delete();
                                                        this.Commit(thirdTransactionFlag);

                                                        for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                        {
                                                            var exceptionThrown = false;
                                                            try
                                                            {
                                                                this.GetUnit(allorsObject, testRoleType, Units.Dummy);
                                                            }
                                                            catch
                                                            {
                                                                exceptionThrown = true;
                                                            }

                                                            Assert.True(exceptionThrown);
                                                            if (useRollbackFlag)
                                                            {
                                                                this.Rollback(transactionFlag);
                                                            }
                                                            else
                                                            {
                                                                this.Commit(transactionFlag);
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            // One2One
                                            var relationTypes = this.GetOne2OneRelations(this.GetMetaPopulation());
                                            for (var relationIndex = 0; relationIndex < relationTypes.Count(); relationIndex++)
                                            {
                                                var relationType = relationTypes[relationIndex];
                                                for (var useRoleCachingFlagIndex = 0; useRoleCachingFlagIndex < this.GetBooleanFlags().Length; useRoleCachingFlagIndex++)
                                                {
                                                    bool useRoleCachingFlag = this.GetBooleanFlags()[useRoleCachingFlagIndex];
                                                    for (var useAssociationCachingFlagIndex = 0; useAssociationCachingFlagIndex < this.GetBooleanFlags().Length; useAssociationCachingFlagIndex++)
                                                    {
                                                        bool useAssociationCachingFlag = this.GetBooleanFlags()[useAssociationCachingFlagIndex];

                                                        var associationTypes = relationType.AssociationType.ObjectType.DatabaseClasses.ToArray();
                                                        for (var iAssociationType = 0; iAssociationType < associationTypes.Count(); iAssociationType++)
                                                        {
                                                            var associationType = associationTypes[iAssociationType];
                                                            var roleTypes = this.GetClasses(relationType);
                                                            for (var iRoleType = 0; iRoleType < roleTypes.Count(); iRoleType++)
                                                            {
                                                                var roleType = roleTypes[iRoleType];

                                                                // delete association
                                                                var association = this.GetTransaction().Create(associationType);
                                                                var role = this.GetTransaction().Create(roleType);

                                                                if (useRoleCachingFlag)
                                                                {
                                                                    association.Strategy.GetRole(relationType.RoleType);
                                                                }

                                                                if (useAssociationCachingFlag)
                                                                {
                                                                    role.Strategy.GetCompositeAssociation(relationType.AssociationType);
                                                                }

                                                                association.Strategy.SetCompositeRole(relationType.RoleType, role);
                                                                this.Commit(secondTransactionFlag);
                                                                association.Strategy.Delete();
                                                                this.Commit(thirdTransactionFlag);

                                                                for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                                {
                                                                    var exceptionThrown = false;
                                                                    try
                                                                    {
                                                                        association.Strategy.GetRole(relationType.RoleType);
                                                                        role.Strategy.GetCompositeAssociation(relationType.AssociationType);
                                                                    }
                                                                    catch
                                                                    {
                                                                        exceptionThrown = true;
                                                                    }

                                                                    Assert.True(exceptionThrown);

                                                                    Assert.Null(role.Strategy.GetCompositeAssociation(relationType.AssociationType));

                                                                    if (useRollbackFlag)
                                                                    {
                                                                        this.Rollback(transactionFlag);
                                                                    }
                                                                    else
                                                                    {
                                                                        this.Commit(transactionFlag);
                                                                    }
                                                                }

                                                                // delete role
                                                                association = this.GetTransaction().Create(associationType);
                                                                role = this.GetTransaction().Create(roleType);

                                                                if (useRoleCachingFlag)
                                                                {
                                                                    association.Strategy.GetRole(relationType.RoleType);
                                                                }

                                                                if (useAssociationCachingFlag)
                                                                {
                                                                    role.Strategy.GetCompositeAssociation(relationType.AssociationType);
                                                                }

                                                                association.Strategy.SetCompositeRole(relationType.RoleType, role);
                                                                this.Commit(secondTransactionFlag);
                                                                role.Strategy.Delete();
                                                                this.Commit(thirdTransactionFlag);

                                                                for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                                {
                                                                    var exceptionThrown = false;
                                                                    try
                                                                    {
                                                                        role.Strategy.GetCompositeAssociation(relationType.AssociationType);
                                                                    }
                                                                    catch
                                                                    {
                                                                        exceptionThrown = true;
                                                                    }

                                                                    Assert.True(exceptionThrown);

                                                                    Assert.Null(association.Strategy.GetRole(relationType.RoleType));

                                                                    if (useRollbackFlag)
                                                                    {
                                                                        this.Rollback(transactionFlag);
                                                                    }
                                                                    else
                                                                    {
                                                                        this.Commit(transactionFlag);
                                                                    }
                                                                }

                                                                // reuse
                                                                association = this.GetTransaction().Create(associationType);
                                                                role = this.GetTransaction().Create(roleType);

                                                                this.Commit(secondTransactionFlag);
                                                                role.Strategy.Delete();
                                                                this.Commit(thirdTransactionFlag);

                                                                for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                                {
                                                                    var exceptionThrown = false;
                                                                    try
                                                                    {
                                                                        association.Strategy.SetCompositeRole(relationType.RoleType, role);
                                                                    }
                                                                    catch
                                                                    {
                                                                        exceptionThrown = true;
                                                                    }

                                                                    Assert.True(exceptionThrown);
                                                                }

                                                                association = this.GetTransaction().Create(associationType);
                                                                role = this.GetTransaction().Create(roleType);

                                                                this.Commit(secondTransactionFlag);
                                                                association.Strategy.Delete();
                                                                this.Commit(thirdTransactionFlag);

                                                                for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                                {
                                                                    var exceptionThrown = false;
                                                                    try
                                                                    {
                                                                        association.Strategy.SetCompositeRole(relationType.RoleType, role);
                                                                    }
                                                                    catch
                                                                    {
                                                                        exceptionThrown = true;
                                                                    }

                                                                    Assert.True(exceptionThrown);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            // Many2One
                                            relationTypes = this.GetMany2OneRelations(this.GetMetaPopulation());
                                            for (var relationIndex = 0; relationIndex < relationTypes.Count(); relationIndex++)
                                            {
                                                var relationType = relationTypes[relationIndex];
                                                for (var useRoleCachingFlagIndex = 0; useRoleCachingFlagIndex < this.GetBooleanFlags().Length; useRoleCachingFlagIndex++)
                                                {
                                                    bool useRoleCachingFlag = this.GetBooleanFlags()[useRoleCachingFlagIndex];

                                                    for (var useAssociationCachingFlagIndex = 0; useAssociationCachingFlagIndex < this.GetBooleanFlags().Length; useAssociationCachingFlagIndex++)
                                                    {
                                                        bool useAssociationCachingFlag = this.GetBooleanFlags()[useAssociationCachingFlagIndex];

                                                        var associationTypes = relationType.AssociationType.ObjectType.DatabaseClasses.ToArray();
                                                        for (var iAssociationType = 0; iAssociationType < associationTypes.Count(); iAssociationType++)
                                                        {
                                                            var associationType = associationTypes[iAssociationType];
                                                            var roleTypes = this.GetClasses(relationType);
                                                            for (var iRoleType = 0; iRoleType < roleTypes.Count(); iRoleType++)
                                                            {
                                                                var roleType = roleTypes[iRoleType];

                                                                // AssociationType
                                                                IObject[] associations = this.CreateArray(relationType.AssociationType.ObjectType, 3);
                                                                associations[0] = this.GetTransaction().Create(associationType);
                                                                associations[1] = this.GetTransaction().Create(associationType);
                                                                associations[2] = this.GetTransaction().Create(associationType);
                                                                IObject[] roles = this.CreateArray(relationType.RoleType.ObjectType, 2);
                                                                roles[0] = this.GetTransaction().Create(roleType);

                                                                roles[1] = this.GetTransaction().Create(roleType);

                                                                if (useRoleCachingFlag)
                                                                {
                                                                    associations[0].Strategy.GetRole(relationType.RoleType);
                                                                    associations[1].Strategy.GetRole(relationType.RoleType);
                                                                    associations[2].Strategy.GetRole(relationType.RoleType);
                                                                }

                                                                if (useAssociationCachingFlag)
                                                                {
                                                                    roles[0].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType).ToArray();
                                                                    roles[1].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType).ToArray();
                                                                }

                                                                associations[0].Strategy.SetCompositeRole(relationType.RoleType, roles[0]);
                                                                associations[1].Strategy.SetCompositeRole(relationType.RoleType, roles[1]);
                                                                associations[2].Strategy.SetCompositeRole(relationType.RoleType, roles[1]);
                                                                this.Commit(secondTransactionFlag);
                                                                associations[0].Strategy.Delete();
                                                                associations[1].Strategy.Delete();
                                                                this.Commit(thirdTransactionFlag);

                                                                for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                                {
                                                                    var exceptionThrown = false;
                                                                    try
                                                                    {
                                                                        associations[0].Strategy.GetRole(relationType.RoleType);
                                                                    }
                                                                    catch
                                                                    {
                                                                        exceptionThrown = true;
                                                                    }

                                                                    Assert.True(exceptionThrown);
                                                                    exceptionThrown = false;
                                                                    try
                                                                    {
                                                                        associations[1].Strategy.GetRole(relationType.RoleType);
                                                                    }
                                                                    catch
                                                                    {
                                                                        exceptionThrown = true;
                                                                    }

                                                                    Assert.True(exceptionThrown);

                                                                    Assert.Equal(roles[1], associations[2].Strategy.GetRole(relationType.RoleType));

                                                                    Assert.Empty((IObject[])roles[0].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                                    Assert.Single((IObject[])roles[1].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                                    Assert.Equal(associations[2], ((IObject[])roles[1].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType))[0]);

                                                                    if (useRollbackFlag)
                                                                    {
                                                                        this.Rollback(transactionFlag);
                                                                    }
                                                                    else
                                                                    {
                                                                        this.Commit(transactionFlag);
                                                                    }
                                                                }

                                                                // Role
                                                                associations = this.CreateArray(relationType.AssociationType.ObjectType, 3);
                                                                associations[0] = this.GetTransaction().Create(associationType);
                                                                associations[1] = this.GetTransaction().Create(associationType);
                                                                associations[2] = this.GetTransaction().Create(associationType);
                                                                roles = this.CreateArray(relationType.RoleType.ObjectType, 2);
                                                                roles[0] = this.GetTransaction().Create(roleType);

                                                                roles[1] = this.GetTransaction().Create(roleType);

                                                                if (useRoleCachingFlag)
                                                                {
                                                                    associations[0].Strategy.GetRole(relationType.RoleType);
                                                                    associations[1].Strategy.GetRole(relationType.RoleType);
                                                                    associations[2].Strategy.GetRole(relationType.RoleType);
                                                                }

                                                                if (useRoleCachingFlag)
                                                                {
                                                                    roles[0].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType).ToArray();
                                                                    roles[1].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType).ToArray();
                                                                }

                                                                associations[0].Strategy.SetCompositeRole(relationType.RoleType, roles[0]);
                                                                associations[1].Strategy.SetCompositeRole(relationType.RoleType, roles[1]);
                                                                associations[2].Strategy.SetCompositeRole(relationType.RoleType, roles[1]);
                                                                this.Commit(secondTransactionFlag);
                                                                roles[0].Strategy.Delete();
                                                                this.Commit(thirdTransactionFlag);

                                                                for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                                {
                                                                    Assert.Null(associations[0].Strategy.GetRole(relationType.RoleType));
                                                                    var exceptionThrown = false;
                                                                    try
                                                                    {
                                                                        roles[0].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType).ToArray();
                                                                    }
                                                                    catch
                                                                    {
                                                                        exceptionThrown = true;
                                                                    }

                                                                    Assert.True(exceptionThrown);

                                                                    Assert.Equal(roles[1], associations[1].Strategy.GetRole(relationType.RoleType));
                                                                    Assert.Equal(roles[1], associations[2].Strategy.GetRole(relationType.RoleType));

                                                                    Assert.Equal(2, ((IObject[])roles[1].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType)).Count());
                                                                    Assert.Contains(associations[1], (IObject[])roles[1].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                                    Assert.Contains(associations[2], (IObject[])roles[1].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));

                                                                    if (useRollbackFlag)
                                                                    {
                                                                        this.Rollback(transactionFlag);
                                                                    }
                                                                    else
                                                                    {
                                                                        this.Commit(transactionFlag);
                                                                    }
                                                                }

                                                                // reuse
                                                                var association = this.GetTransaction().Create(associationType);
                                                                var role = this.GetTransaction().Create(roleType);

                                                                this.Commit(secondTransactionFlag);
                                                                role.Strategy.Delete();
                                                                this.Commit(thirdTransactionFlag);

                                                                for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                                {
                                                                    var exceptionThrown = false;
                                                                    try
                                                                    {
                                                                        association.Strategy.SetCompositeRole(relationType.RoleType, role);
                                                                    }
                                                                    catch
                                                                    {
                                                                        exceptionThrown = true;
                                                                    }

                                                                    Assert.True(exceptionThrown);
                                                                }

                                                                association = this.GetTransaction().Create(associationType);
                                                                role = this.GetTransaction().Create(roleType);

                                                                this.Commit(secondTransactionFlag);
                                                                association.Strategy.Delete();
                                                                this.Commit(thirdTransactionFlag);

                                                                for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                                {
                                                                    var exceptionThrown = false;
                                                                    try
                                                                    {
                                                                        association.Strategy.SetCompositeRole(relationType.RoleType, role);
                                                                    }
                                                                    catch
                                                                    {
                                                                        exceptionThrown = true;
                                                                    }

                                                                    Assert.True(exceptionThrown);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            // One2Many
                                            relationTypes = this.GetOne2ManyRelations(this.GetMetaPopulation());
                                            for (var relationIndex = 0; relationIndex < relationTypes.Count(); relationIndex++)
                                            {
                                                var relationType = relationTypes[relationIndex];
                                                for (var useRoleCachingFlagIndex = 0; useRoleCachingFlagIndex < this.GetBooleanFlags().Length; useRoleCachingFlagIndex++)
                                                {
                                                    bool useRoleCachingFlag = this.GetBooleanFlags()[useRoleCachingFlagIndex];

                                                    for (var useAssociationCachingFlagIndex = 0; useAssociationCachingFlagIndex < this.GetBooleanFlags().Length; useAssociationCachingFlagIndex++)
                                                    {
                                                        bool useAssociationCachingFlag = this.GetBooleanFlags()[useAssociationCachingFlagIndex];
                                                        var associationTypes = relationType.AssociationType.ObjectType.DatabaseClasses.ToArray();
                                                        for (var iAssociationType = 0; iAssociationType < associationTypes.Count(); iAssociationType++)
                                                        {
                                                            var associationType = associationTypes[iAssociationType];
                                                            var roleTypes = this.GetClasses(relationType);
                                                            for (var iRoleType = 0; iRoleType < roleTypes.Count(); iRoleType++)
                                                            {
                                                                var roleType = roleTypes[iRoleType];

                                                                // AssociationType
                                                                IObject[] associations = this.CreateArray(relationType.AssociationType.ObjectType, 2);
                                                                associations[0] = this.GetTransaction().Create(associationType);
                                                                associations[1] = this.GetTransaction().Create(associationType);
                                                                IObject[] roles = this.CreateArray(relationType.RoleType.ObjectType, 3);
                                                                roles[0] = this.GetTransaction().Create(roleType);

                                                                roles[1] = this.GetTransaction().Create(roleType);

                                                                roles[2] = this.GetTransaction().Create(roleType);

                                                                if (useRoleCachingFlag)
                                                                {
                                                                    associations[0].Strategy.GetCompositesRole<IObject>(relationType.RoleType).ToArray();
                                                                    associations[1].Strategy.GetCompositesRole<IObject>(relationType.RoleType).ToArray();
                                                                }

                                                                if (useAssociationCachingFlag)
                                                                {
                                                                    roles[0].Strategy.GetCompositeAssociation(relationType.AssociationType);
                                                                    roles[1].Strategy.GetCompositeAssociation(relationType.AssociationType);
                                                                    roles[2].Strategy.GetCompositeAssociation(relationType.AssociationType);
                                                                }

                                                                associations[0].Strategy.AddCompositesRole(relationType.RoleType, roles[0]);
                                                                associations[0].Strategy.AddCompositesRole(relationType.RoleType, roles[1]);
                                                                associations[1].Strategy.AddCompositesRole(relationType.RoleType, roles[2]);
                                                                this.Commit(secondTransactionFlag);
                                                                associations[0].Strategy.Delete();
                                                                this.Commit(thirdTransactionFlag);

                                                                for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                                {
                                                                    var exceptionThrown = false;
                                                                    try
                                                                    {
                                                                        associations[0].Strategy.GetCompositesRole<IObject>(relationType.RoleType).ToArray();
                                                                    }
                                                                    catch
                                                                    {
                                                                        exceptionThrown = true;
                                                                    }

                                                                    Assert.True(exceptionThrown);

                                                                    IObject[] association1Roles = associations[1].Strategy.GetCompositesRole<IObject>(relationType.RoleType).ToArray();
                                                                    Assert.DoesNotContain(roles[0], association1Roles);
                                                                    Assert.DoesNotContain(roles[1], association1Roles);
                                                                    Assert.Contains(roles[2], association1Roles);

                                                                    Assert.Null(roles[0].Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                                    Assert.Null(roles[1].Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                                    Assert.Equal(associations[1], roles[2].Strategy.GetCompositeAssociation(relationType.AssociationType));

                                                                    if (useRollbackFlag)
                                                                    {
                                                                        this.Rollback(transactionFlag);
                                                                    }
                                                                    else
                                                                    {
                                                                        this.Commit(transactionFlag);
                                                                    }
                                                                }

                                                                // Role
                                                                associations = this.CreateArray(relationType.AssociationType.ObjectType, 2);
                                                                associations[0] = this.GetTransaction().Create(associationType);
                                                                associations[1] = this.GetTransaction().Create(associationType);
                                                                roles = this.CreateArray(relationType.RoleType.ObjectType, 3);
                                                                roles[0] = this.GetTransaction().Create(roleType);

                                                                roles[1] = this.GetTransaction().Create(roleType);

                                                                roles[2] = this.GetTransaction().Create(roleType);

                                                                if (useRoleCachingFlag)
                                                                {
                                                                    associations[0].Strategy.GetCompositesRole<IObject>(relationType.RoleType).ToArray();
                                                                    associations[1].Strategy.GetCompositesRole<IObject>(relationType.RoleType).ToArray();
                                                                }

                                                                if (useAssociationCachingFlag)
                                                                {
                                                                    roles[0].Strategy.GetCompositeAssociation(relationType.AssociationType);
                                                                    roles[1].Strategy.GetCompositeAssociation(relationType.AssociationType);
                                                                    roles[2].Strategy.GetCompositeAssociation(relationType.AssociationType);
                                                                }

                                                                associations[0].Strategy.AddCompositesRole(relationType.RoleType, roles[0]);
                                                                associations[0].Strategy.AddCompositesRole(relationType.RoleType, roles[1]);
                                                                associations[1].Strategy.AddCompositesRole(relationType.RoleType, roles[2]);
                                                                this.Commit(secondTransactionFlag);
                                                                roles[2].Strategy.Delete();
                                                                this.Commit(thirdTransactionFlag);

                                                                for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                                {
                                                                    IObject[] association0Roles = associations[0].Strategy.GetCompositesRole<IObject>(relationType.RoleType).ToArray();
                                                                    Assert.Contains(roles[0], association0Roles);
                                                                    Assert.Contains(roles[1], association0Roles);
                                                                    Assert.Equal(associations[0], roles[0].Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                                    Assert.Equal(associations[0], roles[1].Strategy.GetCompositeAssociation(relationType.AssociationType));

                                                                    Assert.Empty((IObject[])associations[1].Strategy.GetCompositesRole<IObject>(relationType.RoleType));
                                                                    var exceptionThrown = false;
                                                                    try
                                                                    {
                                                                        roles[2].Strategy.GetCompositeAssociation(relationType.AssociationType);
                                                                    }
                                                                    catch
                                                                    {
                                                                        exceptionThrown = true;
                                                                    }

                                                                    Assert.True(exceptionThrown);

                                                                    if (useRollbackFlag)
                                                                    {
                                                                        this.Rollback(transactionFlag);
                                                                    }
                                                                    else
                                                                    {
                                                                        this.Commit(transactionFlag);
                                                                    }
                                                                }

                                                                // reuse
                                                                var association = this.GetTransaction().Create(associationType);
                                                                var role = this.GetTransaction().Create(roleType);

                                                                this.Commit(secondTransactionFlag);
                                                                role.Strategy.Delete();
                                                                this.Commit(thirdTransactionFlag);

                                                                for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                                {
                                                                    var exceptionThrown = false;
                                                                    try
                                                                    {
                                                                        association.Strategy.AddCompositesRole(relationType.RoleType, role);
                                                                    }
                                                                    catch
                                                                    {
                                                                        exceptionThrown = true;
                                                                    }

                                                                    Assert.True(exceptionThrown);
                                                                }

                                                                association = this.GetTransaction().Create(associationType);
                                                                role = this.GetTransaction().Create(roleType);

                                                                this.Commit(secondTransactionFlag);
                                                                association.Strategy.Delete();
                                                                this.Commit(thirdTransactionFlag);

                                                                for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                                {
                                                                    var exceptionThrown = false;
                                                                    try
                                                                    {
                                                                        association.Strategy.AddCompositesRole(relationType.RoleType, role);
                                                                    }
                                                                    catch
                                                                    {
                                                                        exceptionThrown = true;
                                                                    }

                                                                    Assert.True(exceptionThrown);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            // Many2Many
                                            relationTypes = this.GetMany2ManyRelations(this.GetMetaPopulation());
                                            for (var relationIndex = 0; relationIndex < relationTypes.Count(); relationIndex++)
                                            {
                                                var relationType = relationTypes[relationIndex];
                                                for (var useRoleCachingFlagIndex = 0; useRoleCachingFlagIndex < this.GetBooleanFlags().Length; useRoleCachingFlagIndex++)
                                                {
                                                    bool useRoleCachingFlag = this.GetBooleanFlags()[useRoleCachingFlagIndex];

                                                    for (var useAssociationCachingFlagIndex = 0; useAssociationCachingFlagIndex < this.GetBooleanFlags().Length; useAssociationCachingFlagIndex++)
                                                    {
                                                        bool useAssociationCachingFlag = this.GetBooleanFlags()[useAssociationCachingFlagIndex];
                                                        var associationTypes = relationType.AssociationType.ObjectType.DatabaseClasses.ToArray();
                                                        for (var iAssociationType = 0; iAssociationType < associationTypes.Count(); iAssociationType++)
                                                        {
                                                            var associationType = associationTypes[iAssociationType];
                                                            var roleTypes = this.GetClasses(relationType);
                                                            for (var iRoleType = 0; iRoleType < roleTypes.Count(); iRoleType++)
                                                            {
                                                                var roleType = roleTypes[iRoleType];

                                                                // AssociationType
                                                                IObject[] associations = this.CreateArray(relationType.AssociationType.ObjectType, 2);
                                                                associations[0] = this.GetTransaction().Create(associationType);
                                                                associations[1] = this.GetTransaction().Create(associationType);
                                                                IObject[] roles = this.CreateArray(relationType.RoleType.ObjectType, 3);
                                                                roles[0] = this.GetTransaction().Create(roleType);

                                                                roles[1] = this.GetTransaction().Create(roleType);

                                                                roles[2] = this.GetTransaction().Create(roleType);

                                                                if (useRoleCachingFlag)
                                                                {
                                                                    associations[0].Strategy.GetCompositesRole<IObject>(relationType.RoleType).ToArray();
                                                                    associations[1].Strategy.GetCompositesRole<IObject>(relationType.RoleType).ToArray();
                                                                }

                                                                if (useAssociationCachingFlag)
                                                                {
                                                                    roles[0].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType).ToArray();
                                                                    roles[1].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType).ToArray();
                                                                    roles[2].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType).ToArray();
                                                                }

                                                                associations[0].Strategy.AddCompositesRole(relationType.RoleType, roles[0]);
                                                                associations[0].Strategy.AddCompositesRole(relationType.RoleType, roles[1]);
                                                                associations[1].Strategy.AddCompositesRole(relationType.RoleType, roles[1]);
                                                                associations[1].Strategy.AddCompositesRole(relationType.RoleType, roles[2]);
                                                                this.Commit(secondTransactionFlag);
                                                                associations[0].Strategy.Delete();
                                                                this.Commit(thirdTransactionFlag);

                                                                for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                                {
                                                                    var exceptionThrown = false;
                                                                    try
                                                                    {
                                                                        associations[0].Strategy.GetCompositesRole<IObject>(relationType.RoleType).ToArray();
                                                                    }
                                                                    catch
                                                                    {
                                                                        exceptionThrown = true;
                                                                    }

                                                                    Assert.True(exceptionThrown);

                                                                    IObject[] association1Roles = associations[1].Strategy.GetCompositesRole<IObject>(relationType.RoleType).ToArray();
                                                                    Assert.DoesNotContain(roles[0], association1Roles);
                                                                    Assert.Contains(roles[1], association1Roles);
                                                                    Assert.Contains(roles[2], association1Roles);

                                                                    Assert.Empty((IObject[])roles[0].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                                    Assert.Single((IObject[])roles[1].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                                    Assert.Equal(associations[1], ((IObject[])roles[1].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType))[0]);
                                                                    Assert.Single((IObject[])roles[2].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                                    Assert.Equal(associations[1], ((IObject[])roles[2].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType))[0]);

                                                                    if (useRollbackFlag)
                                                                    {
                                                                        this.Rollback(transactionFlag);
                                                                    }
                                                                    else
                                                                    {
                                                                        this.Commit(transactionFlag);
                                                                    }
                                                                }

                                                                // Role
                                                                associations = this.CreateArray(relationType.AssociationType.ObjectType, 2);
                                                                associations[0] = this.GetTransaction().Create(associationType);
                                                                associations[1] = this.GetTransaction().Create(associationType);
                                                                roles = this.CreateArray(relationType.RoleType.ObjectType, 3);
                                                                roles[0] = this.GetTransaction().Create(roleType);
                                                                roles[1] = this.GetTransaction().Create(roleType);
                                                                roles[2] = this.GetTransaction().Create(roleType);

                                                                if (useRoleCachingFlag)
                                                                {
                                                                    associations[0].Strategy.GetCompositesRole<IObject>(relationType.RoleType).ToArray();
                                                                    associations[1].Strategy.GetCompositesRole<IObject>(relationType.RoleType).ToArray();
                                                                }

                                                                if (useAssociationCachingFlag)
                                                                {
                                                                    roles[0].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType).ToArray();
                                                                    roles[1].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType).ToArray();
                                                                    roles[2].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType).ToArray();
                                                                }

                                                                associations[0].Strategy.AddCompositesRole(relationType.RoleType, roles[0]);
                                                                associations[0].Strategy.AddCompositesRole(relationType.RoleType, roles[1]);
                                                                associations[1].Strategy.AddCompositesRole(relationType.RoleType, roles[1]);
                                                                associations[1].Strategy.AddCompositesRole(relationType.RoleType, roles[2]);
                                                                this.Commit(secondTransactionFlag);
                                                                roles[0].Strategy.Delete();
                                                                this.Commit(thirdTransactionFlag);

                                                                for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                                {
                                                                    var exceptionThrown = false;
                                                                    try
                                                                    {
                                                                        roles[0].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType).ToArray();
                                                                    }
                                                                    catch
                                                                    {
                                                                        exceptionThrown = true;
                                                                    }

                                                                    Assert.True(exceptionThrown);

                                                                    IObject[] association0Roles = associations[0].Strategy.GetCompositesRole<IObject>(relationType.RoleType).ToArray();
                                                                    Assert.Single(association0Roles);
                                                                    Assert.Contains(roles[1], association0Roles);

                                                                    IObject[] association1Roles = associations[1].Strategy.GetCompositesRole<IObject>(relationType.RoleType).ToArray();
                                                                    Assert.Equal(2, association1Roles.Count());
                                                                    Assert.Contains(roles[1], association1Roles);
                                                                    Assert.Contains(roles[2], association1Roles);

                                                                    Assert.Equal(2, ((IObject[])roles[1].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType)).Count());
                                                                    Assert.Contains(associations[0], (IObject[])roles[1].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                                    Assert.Contains(associations[1], (IObject[])roles[1].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));

                                                                    Assert.Single((IObject[])roles[2].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                                    Assert.Equal(associations[1], ((IObject[])roles[2].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType))[0]);

                                                                    if (useRollbackFlag)
                                                                    {
                                                                        this.Rollback(transactionFlag);
                                                                    }
                                                                    else
                                                                    {
                                                                        this.Commit(transactionFlag);
                                                                    }
                                                                }

                                                                // reuse
                                                                var association = this.GetTransaction().Create(associationType);
                                                                var role = this.GetTransaction().Create(roleType);

                                                                this.Commit(secondTransactionFlag);
                                                                role.Strategy.Delete();
                                                                this.Commit(thirdTransactionFlag);

                                                                for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                                {
                                                                    var exceptionThrown = false;
                                                                    try
                                                                    {
                                                                        association.Strategy.AddCompositesRole(relationType.RoleType, role);
                                                                    }
                                                                    catch
                                                                    {
                                                                        exceptionThrown = true;
                                                                    }

                                                                    Assert.True(exceptionThrown);
                                                                }

                                                                association = this.GetTransaction().Create(associationType);
                                                                role = this.GetTransaction().Create(roleType);

                                                                this.Commit(secondTransactionFlag);
                                                                association.Strategy.Delete();
                                                                this.Commit(thirdTransactionFlag);

                                                                for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                                {
                                                                    var exceptionThrown = false;
                                                                    try
                                                                    {
                                                                        association.Strategy.AddCompositesRole(relationType.RoleType, role);
                                                                    }
                                                                    catch
                                                                    {
                                                                        exceptionThrown = true;
                                                                    }

                                                                    Assert.True(exceptionThrown);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (this.IsRollbackSupported())
            {
                for (var iRepeat = 0; iRepeat < this.GetRepeats().Length; iRepeat++)
                {
                    var repeat = this.GetRepeats()[iRepeat];
                    for (var iTestRepeat = 0; iTestRepeat < this.GetTestRepeats().Length; iTestRepeat++)
                    {
                        for (var iAssertRepeat = 0; iAssertRepeat < this.GetAssertRepeats().Length; iAssertRepeat++)
                        {
                            for (var iTestType = 0; iTestType < this.GetTestTypes().Length; iTestType++)
                            {
                                var testType = this.GetTestTypes()[iTestType];

                                for (var useRollbackFlagIndex = 0; useRollbackFlagIndex < this.GetBooleanFlags().Length; useRollbackFlagIndex++)
                                {
                                    var useRollbackFlag = this.GetBooleanFlags()[useRollbackFlagIndex];

                                    for (var transactionFlagIndex = 0; transactionFlagIndex < this.GetBooleanFlags().Length; transactionFlagIndex++)
                                    {
                                        var transactionFlag = this.GetBooleanFlags()[transactionFlagIndex];

                                        for (var secondTransactionFlagIndex = 0; secondTransactionFlagIndex < this.GetBooleanFlags().Length; secondTransactionFlagIndex++)
                                        {
                                            var secondTransactionFlag = this.GetBooleanFlags()[secondTransactionFlagIndex];

                                            // Rollback
                                            var allorsObject = this.GetTransaction().Create(testType);
                                            allorsObject.Strategy.Delete();
                                            this.GetTransaction().Rollback();

                                            for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                            {
                                                Assert.True(allorsObject.Strategy.IsDeleted);
                                                if (useRollbackFlag)
                                                {
                                                    this.Rollback(transactionFlag);
                                                }
                                                else
                                                {
                                                    this.Commit(transactionFlag);
                                                }
                                            }

                                            allorsObject = this.GetTransaction().Create(testType);
                                            string id = allorsObject.Strategy.ObjectId.ToString();
                                            allorsObject.Strategy.Delete();
                                            this.GetTransaction().Rollback();
                                            allorsObject = this.GetTransaction().Instantiate(id);
                                            this.Commit(secondTransactionFlag);

                                            for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                            {
                                                Assert.Null(allorsObject);
                                                if (useRollbackFlag)
                                                {
                                                    this.Rollback(transactionFlag);
                                                }
                                                else
                                                {
                                                    this.Commit(transactionFlag);
                                                }
                                            }

                                            // Commit + Rollback
                                            allorsObject = this.GetTransaction().Create(testType);
                                            this.GetTransaction().Commit();
                                            allorsObject.Strategy.Delete();
                                            this.GetTransaction().Rollback();

                                            for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                            {
                                                Assert.False(allorsObject.Strategy.IsDeleted);
                                                if (useRollbackFlag)
                                                {
                                                    this.Rollback(transactionFlag);
                                                }
                                                else
                                                {
                                                    this.Commit(transactionFlag);
                                                }
                                            }

                                            allorsObject = this.GetTransaction().Create(testType);
                                            id = allorsObject.Strategy.ObjectId.ToString();
                                            this.GetTransaction().Commit();
                                            allorsObject.Strategy.Delete();
                                            this.GetTransaction().Rollback();
                                            allorsObject = this.GetTransaction().Instantiate(id);
                                            this.Commit(secondTransactionFlag);

                                            for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                            {
                                                Assert.False(allorsObject.Strategy.IsDeleted);
                                                if (useRollbackFlag)
                                                {
                                                    this.Rollback(transactionFlag);
                                                }
                                                else
                                                {
                                                    this.Commit(transactionFlag);
                                                }
                                            }

                                            IObject proxy = this.GetTransaction().Create(testType);
                                            id = proxy.Strategy.ObjectId.ToString();
                                            this.GetTransaction().Commit();
                                            IObject subject = this.GetTransaction().Instantiate(id);
                                            subject.Strategy.Delete();
                                            this.GetTransaction().Rollback();

                                            for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                            {
                                                Assert.False(proxy.Strategy.IsDeleted);
                                                if (useRollbackFlag)
                                                {
                                                    this.Rollback(transactionFlag);
                                                }
                                                else
                                                {
                                                    this.Commit(transactionFlag);
                                                }
                                            }

                                            allorsObject = this.GetTransaction().Create(testType);
                                            IObject[] beforeExtent = this.GetTransaction().Extent(testType);
                                            id = allorsObject.Strategy.ObjectId.ToString();
                                            this.GetTransaction().Commit();
                                            allorsObject.Strategy.Delete();
                                            this.GetTransaction().Rollback();
                                            this.GetTransaction().Instantiate(id);
                                            this.Commit(secondTransactionFlag);

                                            for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                            {
                                                IObject[] afterExtent = this.GetTransaction().Extent(testType);
                                                Assert.Equal(beforeExtent.Count(), afterExtent.Count());
                                                if (useRollbackFlag)
                                                {
                                                    this.Rollback(transactionFlag);
                                                }
                                                else
                                                {
                                                    this.Commit(transactionFlag);
                                                }
                                            }

                                            // Rollback + Rollback
                                            allorsObject = this.GetTransaction().Create(testType);
                                            this.GetTransaction().Rollback();
                                            var exceptionThrown = false;
                                            try
                                            {
                                                allorsObject.Strategy.Delete();
                                            }
                                            catch
                                            {
                                                exceptionThrown = true;
                                            }

                                            Assert.True(exceptionThrown);

                                            // Units
                                            var testRoleTypes = this.GetUnitRoles(testType);
                                            var beforeValues = new Units(true);
                                            for (var testRoleTypeIndex = 0; testRoleTypeIndex < testRoleTypes.Count(); testRoleTypeIndex++)
                                            {
                                                var testRoleType = testRoleTypes[testRoleTypeIndex];
                                                for (var useCachingFlagIndex = 0; useCachingFlagIndex < this.GetBooleanFlags().Length; useCachingFlagIndex++)
                                                {
                                                    bool useCachingFlag = this.GetBooleanFlags()[useCachingFlagIndex];

                                                    // Rollback
                                                    allorsObject = this.GetTransaction().Create(testType);
                                                    if (useCachingFlag)
                                                    {
                                                        this.GetUnit(allorsObject, testRoleType, Units.Dummy);
                                                    }

                                                    this.SetUnit(allorsObject, testRoleType, beforeValues);
                                                    allorsObject.Strategy.Delete();
                                                    this.GetTransaction().Rollback();

                                                    for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                    {
                                                        exceptionThrown = false;
                                                        try
                                                        {
                                                            this.GetUnit(allorsObject, testRoleType, Units.Dummy);
                                                        }
                                                        catch
                                                        {
                                                            exceptionThrown = true;
                                                        }

                                                        Assert.True(exceptionThrown);
                                                        if (useRollbackFlag)
                                                        {
                                                            this.Rollback(transactionFlag);
                                                        }
                                                        else
                                                        {
                                                            this.Commit(transactionFlag);
                                                        }
                                                    }

                                                    // Commit + Rollback
                                                    allorsObject = this.GetTransaction().Create(testType);
                                                    if (useCachingFlag)
                                                    {
                                                        this.GetUnit(allorsObject, testRoleType, Units.Dummy);
                                                    }

                                                    this.SetUnit(allorsObject, testRoleType, beforeValues);
                                                    this.GetTransaction().Commit();
                                                    allorsObject.Strategy.Delete();
                                                    this.GetTransaction().Rollback();

                                                    for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                    {
                                                        this.GetUnit(allorsObject, testRoleType, Units.Dummy);
                                                        if (useRollbackFlag)
                                                        {
                                                            this.Rollback(transactionFlag);
                                                        }
                                                        else
                                                        {
                                                            this.Commit(transactionFlag);
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        // One2One
                                        var relationTypes = this.GetOne2OneRelations(this.GetMetaPopulation());
                                        for (var relationIndex = 0; relationIndex < relationTypes.Count(); relationIndex++)
                                        {
                                            var relationType = relationTypes[relationIndex];
                                            for (var useRoleCachingFlagIndex = 0; useRoleCachingFlagIndex < this.GetBooleanFlags().Length; useRoleCachingFlagIndex++)
                                            {
                                                bool useRoleCachingFlag = this.GetBooleanFlags()[useRoleCachingFlagIndex];
                                                for (var useAssociationCachingFlagIndex = 0; useAssociationCachingFlagIndex < this.GetBooleanFlags().Length; useAssociationCachingFlagIndex++)
                                                {
                                                    bool useAssociationCachingFlag = this.GetBooleanFlags()[useAssociationCachingFlagIndex];

                                                    var associationTypes = relationType.AssociationType.ObjectType.DatabaseClasses.ToArray();
                                                    for (var iAssociationType = 0; iAssociationType < associationTypes.Count(); iAssociationType++)
                                                    {
                                                        var associationType = associationTypes[iAssociationType];
                                                        var roleTypes = this.GetClasses(relationType);
                                                        for (var iRoleType = 0; iRoleType < roleTypes.Count(); iRoleType++)
                                                        {
                                                            var roleType = roleTypes[iRoleType];

                                                            var association = this.GetTransaction().Create(associationType);
                                                            var role = this.GetTransaction().Create(roleType);

                                                            if (useRoleCachingFlag)
                                                            {
                                                                association.Strategy.GetRole(relationType.RoleType);
                                                            }

                                                            if (useAssociationCachingFlag)
                                                            {
                                                                role.Strategy.GetCompositeAssociation(relationType.AssociationType);
                                                            }

                                                            association.Strategy.SetCompositeRole(relationType.RoleType, role);
                                                            this.GetTransaction().Commit();

                                                            // delete association
                                                            association.Strategy.Delete();
                                                            this.GetTransaction().Rollback();

                                                            for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                            {
                                                                Assert.Equal(association, role.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                                Assert.Equal(role, association.Strategy.GetRole(relationType.RoleType));

                                                                if (useRollbackFlag)
                                                                {
                                                                    this.Rollback(transactionFlag);
                                                                }
                                                                else
                                                                {
                                                                    this.Commit(transactionFlag);
                                                                }
                                                            }

                                                            // delete role
                                                            role.Strategy.Delete();
                                                            this.GetTransaction().Rollback();

                                                            for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                            {
                                                                Assert.Equal(association, role.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                                Assert.Equal(role, association.Strategy.GetRole(relationType.RoleType));

                                                                if (useRollbackFlag)
                                                                {
                                                                    this.Rollback(transactionFlag);
                                                                }
                                                                else
                                                                {
                                                                    this.Commit(transactionFlag);
                                                                }
                                                            }

                                                            // reuse
                                                            association = this.GetTransaction().Create(associationType);
                                                            role = this.GetTransaction().Create(roleType);

                                                            this.GetTransaction().Commit();

                                                            role.Strategy.Delete();
                                                            this.GetTransaction().Rollback();
                                                            association.Strategy.SetCompositeRole(relationType.RoleType, role);

                                                            association.Strategy.Delete();
                                                            this.GetTransaction().Rollback();
                                                            association.Strategy.SetCompositeRole(relationType.RoleType, role);
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        // Many2One
                                        relationTypes = this.GetMany2OneRelations(this.GetMetaPopulation());
                                        for (var relationIndex = 0; relationIndex < relationTypes.Count(); relationIndex++)
                                        {
                                            var relationType = relationTypes[relationIndex];
                                            for (var useRoleCachingFlagIndex = 0; useRoleCachingFlagIndex < this.GetBooleanFlags().Length; useRoleCachingFlagIndex++)
                                            {
                                                bool useRoleCachingFlag = this.GetBooleanFlags()[useRoleCachingFlagIndex];

                                                for (var useAssociationCachingFlagIndex = 0; useAssociationCachingFlagIndex < this.GetBooleanFlags().Length; useAssociationCachingFlagIndex++)
                                                {
                                                    bool useAssociationCachingFlag = this.GetBooleanFlags()[useAssociationCachingFlagIndex];

                                                    var associationTypes = relationType.AssociationType.ObjectType.DatabaseClasses.ToArray();
                                                    for (var iAssociationType = 0; iAssociationType < associationTypes.Count(); iAssociationType++)
                                                    {
                                                        var associationType = associationTypes[iAssociationType];
                                                        var roleTypes = this.GetClasses(relationType);
                                                        for (var iRoleType = 0; iRoleType < roleTypes.Count(); iRoleType++)
                                                        {
                                                            var roleType = roleTypes[iRoleType];

                                                            // AssociationType
                                                            IObject[] associations = this.CreateArray(relationType.AssociationType.ObjectType, 3);
                                                            associations[0] = this.GetTransaction().Create(associationType);
                                                            associations[1] = this.GetTransaction().Create(associationType);
                                                            associations[2] = this.GetTransaction().Create(associationType);
                                                            IObject[] roles = this.CreateArray(relationType.RoleType.ObjectType, 2);
                                                            roles[0] = this.GetTransaction().Create(roleType);

                                                            roles[1] = this.GetTransaction().Create(roleType);

                                                            if (useRoleCachingFlag)
                                                            {
                                                                associations[0].Strategy.GetRole(relationType.RoleType);
                                                                associations[1].Strategy.GetRole(relationType.RoleType);
                                                                associations[2].Strategy.GetRole(relationType.RoleType);
                                                            }

                                                            if (useAssociationCachingFlag)
                                                            {
                                                                roles[0].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType).ToArray();
                                                                roles[1].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType).ToArray();
                                                            }

                                                            associations[0].Strategy.SetCompositeRole(relationType.RoleType, roles[0]);
                                                            associations[1].Strategy.SetCompositeRole(relationType.RoleType, roles[1]);
                                                            associations[2].Strategy.SetCompositeRole(relationType.RoleType, roles[1]);
                                                            this.GetTransaction().Commit();
                                                            associations[0].Strategy.Delete();
                                                            associations[1].Strategy.Delete();
                                                            this.GetTransaction().Rollback();

                                                            for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                            {
                                                                Assert.Equal(roles[0], associations[0].Strategy.GetRole(relationType.RoleType));
                                                                Assert.Equal(roles[1], associations[1].Strategy.GetRole(relationType.RoleType));
                                                                Assert.Equal(roles[1], associations[2].Strategy.GetRole(relationType.RoleType));

                                                                Assert.Single((IObject[])roles[0].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                                Assert.Equal(associations[0], ((IObject[])roles[0].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType))[0]);
                                                                Assert.Equal(2, ((IObject[])roles[1].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType)).Count());
                                                                Assert.Contains(associations[1], (IObject[])roles[1].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                                Assert.Contains(associations[2], (IObject[])roles[1].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));

                                                                if (useRollbackFlag)
                                                                {
                                                                    this.Rollback(transactionFlag);
                                                                }
                                                                else
                                                                {
                                                                    this.Commit(transactionFlag);
                                                                }
                                                            }

                                                            // Role
                                                            associations = this.CreateArray(relationType.AssociationType.ObjectType, 3);
                                                            associations[0] = this.GetTransaction().Create(associationType);
                                                            associations[1] = this.GetTransaction().Create(associationType);
                                                            associations[2] = this.GetTransaction().Create(associationType);
                                                            roles = this.CreateArray(relationType.RoleType.ObjectType, 2);
                                                            roles[0] = this.GetTransaction().Create(roleType);

                                                            roles[1] = this.GetTransaction().Create(roleType);

                                                            if (useRoleCachingFlag)
                                                            {
                                                                associations[0].Strategy.GetRole(relationType.RoleType);
                                                                associations[1].Strategy.GetRole(relationType.RoleType);
                                                                associations[2].Strategy.GetRole(relationType.RoleType);
                                                            }

                                                            if (useRoleCachingFlag)
                                                            {
                                                                roles[0].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType).ToArray();
                                                                roles[1].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType).ToArray();
                                                            }

                                                            associations[0].Strategy.SetCompositeRole(relationType.RoleType, roles[0]);
                                                            associations[1].Strategy.SetCompositeRole(relationType.RoleType, roles[1]);
                                                            associations[2].Strategy.SetCompositeRole(relationType.RoleType, roles[1]);
                                                            this.GetTransaction().Commit();
                                                            roles[0].Strategy.Delete();
                                                            this.GetTransaction().Rollback();

                                                            for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                            {
                                                                Assert.Equal(roles[0], associations[0].Strategy.GetRole(relationType.RoleType));
                                                                Assert.Equal(roles[1], associations[1].Strategy.GetRole(relationType.RoleType));
                                                                Assert.Equal(roles[1], associations[2].Strategy.GetRole(relationType.RoleType));

                                                                Assert.Single((IObject[])roles[0].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                                Assert.Equal(associations[0], ((IObject[])roles[0].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType))[0]);
                                                                Assert.Equal(2, ((IObject[])roles[1].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType)).Count());
                                                                Assert.Contains(associations[1], (IObject[])roles[1].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                                Assert.Contains(associations[2], (IObject[])roles[1].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));

                                                                if (useRollbackFlag)
                                                                {
                                                                    this.Rollback(transactionFlag);
                                                                }
                                                                else
                                                                {
                                                                    this.Commit(transactionFlag);
                                                                }
                                                            }

                                                            // reuse
                                                            var association = this.GetTransaction().Create(associationType);
                                                            var role = this.GetTransaction().Create(roleType);

                                                            this.GetTransaction().Commit();

                                                            role.Strategy.Delete();
                                                            this.GetTransaction().Rollback();
                                                            association.Strategy.SetCompositeRole(relationType.RoleType, role);

                                                            association.Strategy.Delete();
                                                            this.GetTransaction().Rollback();
                                                            association.Strategy.SetCompositeRole(relationType.RoleType, role);
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        // One2Many
                                        relationTypes = this.GetOne2ManyRelations(this.GetMetaPopulation());
                                        for (var relationIndex = 0; relationIndex < relationTypes.Count(); relationIndex++)
                                        {
                                            var relationType = relationTypes[relationIndex];
                                            for (var useRoleCachingFlagIndex = 0; useRoleCachingFlagIndex < this.GetBooleanFlags().Length; useRoleCachingFlagIndex++)
                                            {
                                                bool useRoleCachingFlag = this.GetBooleanFlags()[useRoleCachingFlagIndex];

                                                for (var useAssociationCachingFlagIndex = 0; useAssociationCachingFlagIndex < this.GetBooleanFlags().Length; useAssociationCachingFlagIndex++)
                                                {
                                                    bool useAssociationCachingFlag = this.GetBooleanFlags()[useAssociationCachingFlagIndex];
                                                    var associationTypes = relationType.AssociationType.ObjectType.DatabaseClasses.ToArray();
                                                    for (var iAssociationType = 0; iAssociationType < associationTypes.Count(); iAssociationType++)
                                                    {
                                                        var associationType = associationTypes[iAssociationType];
                                                        var roleTypes = this.GetClasses(relationType);
                                                        for (var iRoleType = 0; iRoleType < roleTypes.Count(); iRoleType++)
                                                        {
                                                            var roleType = roleTypes[iRoleType];

                                                            // AssociationType
                                                            IObject[] associations = this.CreateArray(relationType.AssociationType.ObjectType, 2);
                                                            associations[0] = this.GetTransaction().Create(associationType);
                                                            associations[1] = this.GetTransaction().Create(associationType);
                                                            IObject[] roles = this.CreateArray(relationType.RoleType.ObjectType, 3);
                                                            roles[0] = this.GetTransaction().Create(roleType);

                                                            roles[1] = this.GetTransaction().Create(roleType);

                                                            roles[2] = this.GetTransaction().Create(roleType);

                                                            if (useRoleCachingFlag)
                                                            {
                                                                associations[0].Strategy.GetCompositesRole<IObject>(relationType.RoleType).ToArray();
                                                                associations[1].Strategy.GetCompositesRole<IObject>(relationType.RoleType).ToArray();
                                                            }

                                                            if (useAssociationCachingFlag)
                                                            {
                                                                roles[0].Strategy.GetCompositeAssociation(relationType.AssociationType);
                                                                roles[1].Strategy.GetCompositeAssociation(relationType.AssociationType);
                                                                roles[2].Strategy.GetCompositeAssociation(relationType.AssociationType);
                                                            }

                                                            associations[0].Strategy.AddCompositesRole(relationType.RoleType, roles[0]);
                                                            associations[0].Strategy.AddCompositesRole(relationType.RoleType, roles[1]);
                                                            associations[1].Strategy.AddCompositesRole(relationType.RoleType, roles[2]);
                                                            this.GetTransaction().Commit();
                                                            associations[0].Strategy.Delete();
                                                            this.GetTransaction().Rollback();

                                                            for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                            {
                                                                IObject[] association0Roles = associations[0].Strategy.GetCompositesRole<IObject>(relationType.RoleType).ToArray();
                                                                Assert.Equal(2, association0Roles.Count());
                                                                Assert.Contains(roles[0], association0Roles);
                                                                Assert.Contains(roles[1], association0Roles);

                                                                IObject[] association1Roles = associations[1].Strategy.GetCompositesRole<IObject>(relationType.RoleType).ToArray();
                                                                Assert.Single(association1Roles);
                                                                Assert.Contains(roles[2], association1Roles);

                                                                Assert.Equal(associations[0], roles[0].Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                                Assert.Equal(associations[0], roles[1].Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                                Assert.Equal(associations[1], roles[2].Strategy.GetCompositeAssociation(relationType.AssociationType));

                                                                if (useRollbackFlag)
                                                                {
                                                                    this.Rollback(transactionFlag);
                                                                }
                                                                else
                                                                {
                                                                    this.Commit(transactionFlag);
                                                                }
                                                            }

                                                            // Role
                                                            associations = this.CreateArray(relationType.AssociationType.ObjectType, 2);
                                                            associations[0] = this.GetTransaction().Create(associationType);
                                                            associations[1] = this.GetTransaction().Create(associationType);
                                                            roles = this.CreateArray(relationType.RoleType.ObjectType, 3);
                                                            roles[0] = this.GetTransaction().Create(roleType);

                                                            roles[1] = this.GetTransaction().Create(roleType);

                                                            roles[2] = this.GetTransaction().Create(roleType);

                                                            if (useRoleCachingFlag)
                                                            {
                                                                associations[0].Strategy.GetCompositesRole<IObject>(relationType.RoleType).ToArray();
                                                                associations[1].Strategy.GetCompositesRole<IObject>(relationType.RoleType).ToArray();
                                                            }

                                                            if (useAssociationCachingFlag)
                                                            {
                                                                roles[0].Strategy.GetCompositeAssociation(relationType.AssociationType);
                                                                roles[1].Strategy.GetCompositeAssociation(relationType.AssociationType);
                                                                roles[2].Strategy.GetCompositeAssociation(relationType.AssociationType);
                                                            }

                                                            associations[0].Strategy.AddCompositesRole(relationType.RoleType, roles[0]);
                                                            associations[0].Strategy.AddCompositesRole(relationType.RoleType, roles[1]);
                                                            associations[1].Strategy.AddCompositesRole(relationType.RoleType, roles[2]);
                                                            this.GetTransaction().Commit();
                                                            roles[2].Strategy.Delete();
                                                            this.GetTransaction().Rollback();

                                                            for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                            {
                                                                IObject[] association0Roles = associations[0].Strategy.GetCompositesRole<IObject>(relationType.RoleType).ToArray();
                                                                Assert.Equal(2, association0Roles.Count());
                                                                Assert.Contains(roles[0], association0Roles);
                                                                Assert.Contains(roles[1], association0Roles);

                                                                IObject[] association1Roles = associations[1].Strategy.GetCompositesRole<IObject>(relationType.RoleType).ToArray();
                                                                Assert.Single(association1Roles);
                                                                Assert.Contains(roles[2], association1Roles);

                                                                Assert.Equal(associations[0], roles[0].Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                                Assert.Equal(associations[0], roles[1].Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                                Assert.Equal(associations[1], roles[2].Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                            }

                                                            // reuse
                                                            var association = this.GetTransaction().Create(associationType);
                                                            var role = this.GetTransaction().Create(roleType);

                                                            this.GetTransaction().Commit();

                                                            role.Strategy.Delete();
                                                            this.GetTransaction().Rollback();
                                                            association.Strategy.AddCompositesRole(relationType.RoleType, role);

                                                            association.Strategy.Delete();
                                                            this.GetTransaction().Rollback();
                                                            association.Strategy.AddCompositesRole(relationType.RoleType, role);
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        // Many2Many
                                        relationTypes = this.GetMany2ManyRelations(this.GetMetaPopulation());
                                        for (var relationIndex = 0; relationIndex < relationTypes.Count(); relationIndex++)
                                        {
                                            var relationType = relationTypes[relationIndex];
                                            for (var useRoleCachingFlagIndex = 0; useRoleCachingFlagIndex < this.GetBooleanFlags().Length; useRoleCachingFlagIndex++)
                                            {
                                                bool useRoleCachingFlag = this.GetBooleanFlags()[useRoleCachingFlagIndex];

                                                for (var useAssociationCachingFlagIndex = 0; useAssociationCachingFlagIndex < this.GetBooleanFlags().Length; useAssociationCachingFlagIndex++)
                                                {
                                                    bool useAssociationCachingFlag = this.GetBooleanFlags()[useAssociationCachingFlagIndex];
                                                    var associationTypes = relationType.AssociationType.ObjectType.DatabaseClasses.ToArray();
                                                    for (var iAssociationType = 0; iAssociationType < associationTypes.Count(); iAssociationType++)
                                                    {
                                                        var associationType = associationTypes[iAssociationType];
                                                        var roleTypes = this.GetClasses(relationType);
                                                        for (var iRoleType = 0; iRoleType < roleTypes.Count(); iRoleType++)
                                                        {
                                                            var roleType = roleTypes[iRoleType];

                                                            // AssociationType
                                                            IObject[] associations = this.CreateArray(relationType.AssociationType.ObjectType, 2);
                                                            associations[0] = this.GetTransaction().Create(associationType);
                                                            associations[1] = this.GetTransaction().Create(associationType);
                                                            IObject[] roles = this.CreateArray(relationType.RoleType.ObjectType, 3);
                                                            roles[0] = this.GetTransaction().Create(roleType);

                                                            roles[1] = this.GetTransaction().Create(roleType);

                                                            roles[2] = this.GetTransaction().Create(roleType);

                                                            if (useRoleCachingFlag)
                                                            {
                                                                associations[0].Strategy.GetCompositesRole<IObject>(relationType.RoleType).ToArray();
                                                                associations[1].Strategy.GetCompositesRole<IObject>(relationType.RoleType).ToArray();
                                                            }

                                                            if (useAssociationCachingFlag)
                                                            {
                                                                roles[0].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType).ToArray();
                                                                roles[1].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType).ToArray();
                                                                roles[2].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType).ToArray();
                                                            }

                                                            associations[0].Strategy.AddCompositesRole(relationType.RoleType, roles[0]);
                                                            associations[0].Strategy.AddCompositesRole(relationType.RoleType, roles[1]);
                                                            associations[1].Strategy.AddCompositesRole(relationType.RoleType, roles[1]);
                                                            associations[1].Strategy.AddCompositesRole(relationType.RoleType, roles[2]);
                                                            this.GetTransaction().Commit();
                                                            associations[0].Strategy.Delete();
                                                            this.GetTransaction().Rollback();

                                                            for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                            {
                                                                IObject[] association0Roles = associations[0].Strategy.GetCompositesRole<IObject>(relationType.RoleType).ToArray();
                                                                Assert.Equal(2, association0Roles.Count());
                                                                Assert.Contains(roles[0], association0Roles);
                                                                Assert.Contains(roles[1], association0Roles);

                                                                IObject[] association1Roles = associations[1].Strategy.GetCompositesRole<IObject>(relationType.RoleType).ToArray();
                                                                Assert.Equal(2, association1Roles.Count());
                                                                Assert.Contains(roles[1], association1Roles);
                                                                Assert.Contains(roles[2], association1Roles);

                                                                IObject[] role0Associations = roles[0].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType).ToArray();
                                                                Assert.Single(role0Associations);
                                                                Assert.Equal(associations[0], role0Associations[0]);
                                                                IObject[] role1Associations = roles[1].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType).ToArray();
                                                                Assert.Equal(2, role1Associations.Count());
                                                                Assert.Contains(associations[0], role1Associations);
                                                                Assert.Contains(associations[1], role1Associations);
                                                                IObject[] role2Associations = roles[2].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType).ToArray();
                                                                Assert.Single(role2Associations);
                                                                Assert.Equal(associations[1], role2Associations[0]);

                                                                if (useRollbackFlag)
                                                                {
                                                                    this.Rollback(transactionFlag);
                                                                }
                                                                else
                                                                {
                                                                    this.Commit(transactionFlag);
                                                                }
                                                            }

                                                            // Role
                                                            associations = this.CreateArray(relationType.AssociationType.ObjectType, 2);
                                                            associations[0] = this.GetTransaction().Create(associationType);
                                                            associations[1] = this.GetTransaction().Create(associationType);
                                                            roles = this.CreateArray(relationType.RoleType.ObjectType, 3);
                                                            roles[0] = this.GetTransaction().Create(roleType);
                                                            roles[1] = this.GetTransaction().Create(roleType);
                                                            roles[2] = this.GetTransaction().Create(roleType);

                                                            if (useRoleCachingFlag)
                                                            {
                                                                associations[0].Strategy.GetCompositesRole<IObject>(relationType.RoleType).ToArray();
                                                                associations[1].Strategy.GetCompositesRole<IObject>(relationType.RoleType).ToArray();
                                                            }

                                                            if (useAssociationCachingFlag)
                                                            {
                                                                roles[0].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType).ToArray();
                                                                roles[1].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType).ToArray();
                                                                roles[2].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType).ToArray();
                                                            }

                                                            associations[0].Strategy.AddCompositesRole(relationType.RoleType, roles[0]);
                                                            associations[0].Strategy.AddCompositesRole(relationType.RoleType, roles[1]);
                                                            associations[1].Strategy.AddCompositesRole(relationType.RoleType, roles[1]);
                                                            associations[1].Strategy.AddCompositesRole(relationType.RoleType, roles[2]);
                                                            this.GetTransaction().Commit();
                                                            roles[0].Strategy.Delete();
                                                            this.GetTransaction().Rollback();

                                                            for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                            {
                                                                IObject[] association0Roles = associations[0].Strategy.GetCompositesRole<IObject>(relationType.RoleType).ToArray();
                                                                Assert.Equal(2, association0Roles.Count());
                                                                Assert.Contains(roles[0], association0Roles);
                                                                Assert.Contains(roles[1], association0Roles);

                                                                IObject[] association1Roles = associations[1].Strategy.GetCompositesRole<IObject>(relationType.RoleType).ToArray();
                                                                Assert.Equal(2, association1Roles.Count());
                                                                Assert.Contains(roles[1], association1Roles);
                                                                Assert.Contains(roles[2], association1Roles);

                                                                IObject[] role0Associations = roles[0].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType).ToArray();
                                                                Assert.Single(role0Associations);
                                                                Assert.Equal(associations[0], role0Associations[0]);
                                                                IObject[] role1Associations = roles[1].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType).ToArray();
                                                                Assert.Equal(2, role1Associations.Count());
                                                                Assert.Contains(associations[0], role1Associations);
                                                                Assert.Contains(associations[1], role1Associations);
                                                                IObject[] role2Associations = roles[2].Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType).ToArray();
                                                                Assert.Single(role2Associations);
                                                                Assert.Equal(associations[1], role2Associations[0]);
                                                            }

                                                            // reuse
                                                            var association = this.GetTransaction().Create(associationType);
                                                            var role = this.GetTransaction().Create(roleType);

                                                            this.GetTransaction().Commit();

                                                            role.Strategy.Delete();
                                                            this.GetTransaction().Rollback();
                                                            association.Strategy.AddCompositesRole(relationType.RoleType, role);

                                                            association.Strategy.Delete();
                                                            this.GetTransaction().Rollback();
                                                            association.Strategy.AddCompositesRole(relationType.RoleType, role);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        [Fact]
        [Trait("Category", "Dynamic")]
        public void Identity()
        {
            for (var iRepeat = 0; iRepeat < this.GetRepeats().Length; iRepeat++)
            {
                var repeat = this.GetRepeats()[iRepeat];
                for (var iTestRepeat = 0; iTestRepeat < this.GetTestRepeats().Length; iTestRepeat++)
                {
                    for (var iAssertRepeat = 0; iAssertRepeat < this.GetAssertRepeats().Length; iAssertRepeat++)
                    {
                        for (var transactionFlagIndex = 0; transactionFlagIndex < this.GetBooleanFlags().Length; transactionFlagIndex++)
                        {
                            var transactionFlag = this.GetBooleanFlags()[transactionFlagIndex];

                            for (var secondTransactionFlagIndex = 0; secondTransactionFlagIndex < this.GetBooleanFlags().Length; secondTransactionFlagIndex++)
                            {
                                var secondTransactionFlag = this.GetBooleanFlags()[secondTransactionFlagIndex];
                                for (var thirdTransactionFlagIndex = 0; thirdTransactionFlagIndex < this.GetBooleanFlags().Length; thirdTransactionFlagIndex++)
                                {
                                    for (var fourthTransactionFlagIndex = 0; fourthTransactionFlagIndex < this.GetBooleanFlags().Length; fourthTransactionFlagIndex++)
                                    {
                                        for (var useRollbackFlagIndex = 0; useRollbackFlagIndex < this.GetBooleanFlags().Length; useRollbackFlagIndex++)
                                        {
                                            var useRollbackFlag = this.GetBooleanFlags()[useRollbackFlagIndex];
                                            if (!this.IsRollbackSupported())
                                            {
                                                useRollbackFlag = false;
                                            }

                                            for (var iTestType = 0; iTestType < this.GetTestTypes().Length; iTestType++)
                                            {
                                                var testType = this.GetTestTypes()[iTestType];

                                                var anObject = this.GetTransaction().Create(testType);
                                                var anId = anObject.Strategy.ObjectId.ToString();
                                                var aProxy = this.GetTransaction().Instantiate(anId);

                                                var anotherObject = this.GetTransaction().Create(testType);
                                                var anotherId = anotherObject.Strategy.ObjectId.ToString();
                                                var anotherProxy = this.GetTransaction().Instantiate(anotherId);

                                                this.Commit(secondTransactionFlag);
                                                for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                {
                                                    Assert.Equal(anObject, aProxy);
                                                    Assert.Equal(anotherObject, anotherProxy);
                                                    Assert.NotEqual(anObject, anotherObject);
                                                    Assert.NotEqual(anObject, anotherProxy);
                                                    Assert.NotEqual(aProxy, anotherObject);
                                                    Assert.NotEqual(aProxy, anotherProxy);

                                                    if (useRollbackFlag)
                                                    {
                                                        this.Rollback(transactionFlag);
                                                    }
                                                    else
                                                    {
                                                        this.Commit(transactionFlag);
                                                    }
                                                }

                                                anObject = this.GetTransaction().Create(testType);
                                                anId = anObject.Strategy.ObjectId.ToString();

                                                anotherObject = this.GetTransaction().Create(testType);
                                                anotherId = anotherObject.Strategy.ObjectId.ToString();

                                                this.Commit(secondTransactionFlag);

                                                anObject = this.GetTransaction().Instantiate(anId);
                                                aProxy = this.GetTransaction().Instantiate(anId);
                                                anotherObject = this.GetTransaction().Instantiate(anotherId);
                                                anotherProxy = this.GetTransaction().Instantiate(anotherId);

                                                for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                {
                                                    Assert.Equal(anObject, aProxy);
                                                    Assert.Equal(anotherObject, anotherProxy);
                                                    Assert.NotEqual(anObject, anotherObject);
                                                    Assert.NotEqual(anObject, anotherProxy);
                                                    Assert.NotEqual(aProxy, anotherObject);
                                                    Assert.NotEqual(aProxy, anotherProxy);

                                                    if (useRollbackFlag)
                                                    {
                                                        this.Rollback(transactionFlag);
                                                    }
                                                    else
                                                    {
                                                        this.Commit(transactionFlag);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (this.IsRollbackSupported())
            {
                for (var iRepeat = 0; iRepeat < this.GetRepeats().Length; iRepeat++)
                {
                    var repeat = this.GetRepeats()[iRepeat];
                    for (var iTestRepeat = 0; iTestRepeat < this.GetTestRepeats().Length; iTestRepeat++)
                    {
                        for (var iAssertRepeat = 0; iAssertRepeat < this.GetAssertRepeats().Length; iAssertRepeat++)
                        {
                            for (var iTestType = 0; iTestType < this.GetTestTypes().Length; iTestType++)
                            {
                                var testType = this.GetTestTypes()[iTestType];

                                for (var useRollbackFlagIndex = 0; useRollbackFlagIndex < this.GetBooleanFlags().Length; useRollbackFlagIndex++)
                                {
                                    var useRollbackFlag = this.GetBooleanFlags()[useRollbackFlagIndex];

                                    for (var transactionFlagIndex = 0; transactionFlagIndex < this.GetBooleanFlags().Length; transactionFlagIndex++)
                                    {
                                        var transactionFlag = this.GetBooleanFlags()[transactionFlagIndex];

                                        for (var secondTransactionFlagIndex = 0; secondTransactionFlagIndex < this.GetBooleanFlags().Length; secondTransactionFlagIndex++)
                                        {
                                            var anObject = this.GetTransaction().Create(testType);
                                            var anId = anObject.Strategy.ObjectId.ToString();
                                            var aProxy = this.GetTransaction().Instantiate(anId);

                                            var anotherObject = this.GetTransaction().Create(testType);
                                            var anotherId = anotherObject.Strategy.ObjectId.ToString();
                                            var anotherProxy = this.GetTransaction().Instantiate(anotherId);

                                            this.GetTransaction().Rollback();
                                            for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                            {
                                                Assert.Equal(anObject, aProxy);
                                                Assert.Equal(anotherObject, anotherProxy);
                                                Assert.NotEqual(anObject, anotherObject);
                                                Assert.NotEqual(anObject, anotherProxy);
                                                Assert.NotEqual(aProxy, anotherObject);
                                                Assert.NotEqual(aProxy, anotherProxy);

                                                if (useRollbackFlag)
                                                {
                                                    this.Rollback(transactionFlag);
                                                }
                                                else
                                                {
                                                    this.Commit(transactionFlag);
                                                }
                                            }

                                            anObject = this.GetTransaction().Create(testType);
                                            anId = anObject.Strategy.ObjectId.ToString();
                                            this.GetTransaction().Instantiate(anId); // aProxy

                                            anotherObject = this.GetTransaction().Create(testType);
                                            anotherId = anotherObject.Strategy.ObjectId.ToString();
                                            this.GetTransaction().Instantiate(anotherId); // anotherProxy

                                            this.GetTransaction().Rollback();

                                            anObject = this.GetTransaction().Instantiate(anId);
                                            aProxy = this.GetTransaction().Instantiate(anId);

                                            anotherObject = this.GetTransaction().Instantiate(anotherId);
                                            anotherProxy = this.GetTransaction().Instantiate(anotherId);

                                            for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                            {
                                                Assert.Null(anObject);
                                                Assert.Null(aProxy);
                                                Assert.Null(anotherObject);
                                                Assert.Null(anotherProxy);

                                                if (useRollbackFlag)
                                                {
                                                    this.Rollback(transactionFlag);
                                                }
                                                else
                                                {
                                                    this.Commit(transactionFlag);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        [Fact]
        [Trait("Category", "Dynamic")]
        public void InstantiateTest()
        {
            for (var iRepeat = 0; iRepeat < this.GetRepeats().Length; iRepeat++)
            {
                var repeat = this.GetRepeats()[iRepeat];
                for (var iTestRepeat = 0; iTestRepeat < this.GetTestRepeats().Length; iTestRepeat++)
                {
                    var testRepeat = this.GetTestRepeats()[iTestRepeat];
                    for (var iAssertRepeat = 0; iAssertRepeat < this.GetAssertRepeats().Length; iAssertRepeat++)
                    {
                        for (var iManyFlag = 0; iManyFlag < this.manyFlags.Count(); iManyFlag++)
                        {
                            bool manyFlag = this.manyFlags[iManyFlag];

                            for (var transactionFlagIndex = 0; transactionFlagIndex < this.GetBooleanFlags().Length; transactionFlagIndex++)
                            {
                                var transactionFlag = this.GetBooleanFlags()[transactionFlagIndex];

                                for (var iTestType = 0; iTestType < this.GetTestTypes().Length; iTestType++)
                                {
                                    var testType = this.GetTestTypes()[iTestType];
                                    {
                                        // Non existing Id's
                                        for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                        {
                                            IObject unexistingObject = this.Instantiate(int.MaxValue - 1, manyFlag);
                                            Assert.Null(unexistingObject);
                                        }
                                    }

                                    {
                                        // Equality & Id's
                                        var anObject = this.GetTransaction().Create(testType);
                                        var id = int.Parse(anObject.Strategy.ObjectId.ToString());
                                        IObject sameObject = this.Instantiate(id, manyFlag);

                                        for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                        {
                                            for (var testRepeatIndex = 0;
                                                 testRepeatIndex < testRepeat;
                                                 testRepeatIndex++)
                                            {
                                                Assert.Equal(anObject, sameObject);
                                                Assert.Equal(anObject.Strategy.ObjectId, sameObject.Strategy.ObjectId);
                                                this.Commit(transactionFlag);
                                            }

                                            sameObject = this.Instantiate(id, manyFlag);

                                            for (var testRepeatIndex = 0;
                                                 testRepeatIndex < testRepeat;
                                                 testRepeatIndex++)
                                            {
                                                Assert.Equal(anObject, sameObject);
                                                Assert.Equal(anObject.Strategy.ObjectId, sameObject.Strategy.ObjectId);
                                                this.Commit(transactionFlag);
                                            }

                                            anObject = this.Instantiate(id, manyFlag);

                                            for (var testRepeatIndex = 0;
                                                 testRepeatIndex < testRepeat;
                                                 testRepeatIndex++)
                                            {
                                                Assert.Equal(anObject, sameObject);
                                                Assert.Equal(anObject.Strategy.ObjectId, sameObject.Strategy.ObjectId);
                                                this.Commit(transactionFlag);
                                            }
                                        }
                                    }

                                    {
                                        // String RelationTypes
                                        IObject subject = this.GetTransaction().Create(testType);
                                        var id = int.Parse(subject.Strategy.ObjectId.ToString());
                                        var testRoleTypes = this.GetStringRoles(testType);

                                        string valueA = this.ValueGenerator.GenerateString(100);
                                        string valueB = this.ValueGenerator.GenerateString(100);
                                        string valueC = this.ValueGenerator.GenerateString(100);
                                        string valueD = this.ValueGenerator.GenerateString(100);

                                        for (var testRoleTypeIndex = 0;
                                             testRoleTypeIndex < testRoleTypes.Count();
                                             testRoleTypeIndex++)
                                        {
                                            var testRoleType = testRoleTypes[testRoleTypeIndex];
                                            this.Commit(transactionFlag);

                                            subject.Strategy.SetUnitRole(testRoleType.RoleType, valueA);
                                            IObject proxy = this.Instantiate(id, manyFlag);
                                            proxy.Strategy.SetUnitRole(testRoleType.RoleType, valueB);

                                            Assert.Equal(valueB, subject.Strategy.GetUnitRole(testRoleType.RoleType));
                                            Assert.Equal(valueB, proxy.Strategy.GetUnitRole(testRoleType.RoleType));
                                            this.Commit(transactionFlag);

                                            subject.Strategy.SetUnitRole(testRoleType.RoleType, valueC);
                                            proxy = this.Instantiate(id, manyFlag);
                                            proxy.Strategy.SetUnitRole(testRoleType.RoleType, valueD);

                                            Assert.Equal(valueD, subject.Strategy.GetUnitRole(testRoleType.RoleType));
                                            Assert.Equal(valueD, proxy.Strategy.GetUnitRole(testRoleType.RoleType));
                                            this.Commit(transactionFlag);

                                            subject.Strategy.SetUnitRole(testRoleType.RoleType, valueA);
                                            proxy = this.Instantiate(id, manyFlag);
                                            proxy.Strategy.SetUnitRole(testRoleType.RoleType, valueB);
                                            this.Commit(transactionFlag);
                                            Assert.Equal(valueB, subject.Strategy.GetUnitRole(testRoleType.RoleType));
                                            Assert.Equal(valueB, proxy.Strategy.GetUnitRole(testRoleType.RoleType));
                                            this.Commit(transactionFlag);

                                            subject.Strategy.SetUnitRole(testRoleType.RoleType, valueC);
                                            proxy = this.Instantiate(id, manyFlag);
                                            proxy.Strategy.SetUnitRole(testRoleType.RoleType, valueD);
                                            this.Commit(transactionFlag);
                                            Assert.Equal(valueD, subject.Strategy.GetUnitRole(testRoleType.RoleType));
                                            Assert.Equal(valueD, proxy.Strategy.GetUnitRole(testRoleType.RoleType));
                                            this.Commit(transactionFlag);

                                            subject.Strategy.SetUnitRole(testRoleType.RoleType, valueA);
                                            proxy = this.Instantiate(id, manyFlag);
                                            proxy.Strategy.SetUnitRole(testRoleType.RoleType, valueB);
                                            Assert.Equal(valueB, subject.Strategy.GetUnitRole(testRoleType.RoleType));
                                            this.Commit(transactionFlag);
                                            Assert.Equal(valueB, proxy.Strategy.GetUnitRole(testRoleType.RoleType));
                                            this.Commit(transactionFlag);

                                            subject.Strategy.SetUnitRole(testRoleType.RoleType, valueC);
                                            proxy = this.Instantiate(id, manyFlag);
                                            proxy.Strategy.SetUnitRole(testRoleType.RoleType, valueD);
                                            Assert.Equal(valueD, subject.Strategy.GetUnitRole(testRoleType.RoleType));
                                            this.Commit(transactionFlag);
                                            Assert.Equal(valueD, proxy.Strategy.GetUnitRole(testRoleType.RoleType));
                                            this.Commit(transactionFlag);

                                            subject.Strategy.SetUnitRole(testRoleType.RoleType, valueA);
                                            proxy = this.Instantiate(id, manyFlag);
                                            proxy.Strategy.SetUnitRole(testRoleType.RoleType, valueB);
                                            this.Commit(transactionFlag);
                                            Assert.Equal(valueB, subject.Strategy.GetUnitRole(testRoleType.RoleType));
                                            this.Commit(transactionFlag);
                                            Assert.Equal(valueB, proxy.Strategy.GetUnitRole(testRoleType.RoleType));
                                            this.Commit(transactionFlag);

                                            subject.Strategy.SetUnitRole(testRoleType.RoleType, valueC);
                                            proxy = this.Instantiate(id, manyFlag);
                                            proxy.Strategy.SetUnitRole(testRoleType.RoleType, valueD);
                                            this.Commit(transactionFlag);
                                            Assert.Equal(valueD, subject.Strategy.GetUnitRole(testRoleType.RoleType));
                                            this.Commit(transactionFlag);
                                            Assert.Equal(valueD, proxy.Strategy.GetUnitRole(testRoleType.RoleType));
                                            this.Commit(transactionFlag);
                                        }
                                    }
                                }

                                {
                                    // One2One RelationTypes
                                    var relationTypes = this.GetOne2OneRelations(this.GetMetaPopulation());
                                    for (var iRelation = 0; iRelation < relationTypes.Count(); iRelation++)
                                    {
                                        var relationType = relationTypes[iRelation];
                                        var associationTypes =
                                            relationType.AssociationType.ObjectType.DatabaseClasses.ToArray();
                                        for (var iAssociationType = 0;
                                             iAssociationType < associationTypes.Count();
                                             iAssociationType++)
                                        {
                                            var associationType = associationTypes[iAssociationType];
                                            var roleTypes = this.GetClasses(relationType);
                                            for (var iRoleType = 0; iRoleType < roleTypes.Count(); iRoleType++)
                                            {
                                                var roleType = roleTypes[iRoleType];
                                                var association = this.GetTransaction().Create(associationType);
                                                var role = this.GetTransaction().Create(roleType);
                                                var associationId = int.Parse(association.Strategy.ObjectId.ToString());
                                                var roleId = int.Parse(role.Strategy.ObjectId.ToString());
                                                var associationProxy = this.Instantiate(associationId, manyFlag);
                                                var roleProxy = this.Instantiate(roleId, manyFlag);
                                                this.Commit(transactionFlag);

                                                for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                {
                                                    // set association
                                                    association.Strategy.SetCompositeRole(relationType.RoleType, role);
                                                    this.Commit(transactionFlag);

                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Equal(roleProxy, association.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Equal(associationProxy, role.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        Assert.Equal(role, associationProxy.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Equal(association, roleProxy.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        this.Commit(transactionFlag);
                                                    }

                                                    association.Strategy.SetCompositeRole(relationType.RoleType, null);
                                                    this.Commit(transactionFlag);
                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Null(association.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Null(role.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        Assert.Null(associationProxy.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Null(roleProxy.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        this.Commit(transactionFlag);
                                                    }

                                                    // instantiate all, set association with role
                                                    association = this.Instantiate(associationId, manyFlag);
                                                    associationProxy = this.Instantiate(associationId, manyFlag);
                                                    role = this.Instantiate(roleId, manyFlag);
                                                    roleProxy = this.Instantiate(roleId, manyFlag);
                                                    association.Strategy.SetCompositeRole(relationType.RoleType, role);
                                                    this.Commit(transactionFlag);

                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Equal(roleProxy, association.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Equal(associationProxy, role.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        Assert.Equal(role, associationProxy.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Equal(association, roleProxy.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        this.Commit(transactionFlag);
                                                    }

                                                    association.Strategy.SetCompositeRole(relationType.RoleType, null);
                                                    this.Commit(transactionFlag);
                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Null(association.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Null(role.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        Assert.Null(associationProxy.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Null(roleProxy.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        this.Commit(transactionFlag);
                                                    }

                                                    // instantiate all, set association with roleProxy
                                                    association = this.Instantiate(associationId, manyFlag);
                                                    associationProxy = this.Instantiate(associationId, manyFlag);
                                                    role = this.Instantiate(roleId, manyFlag);
                                                    roleProxy = this.Instantiate(roleId, manyFlag);
                                                    association.Strategy.SetCompositeRole(relationType.RoleType, roleProxy);
                                                    this.Commit(transactionFlag);

                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Equal(roleProxy, association.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Equal(associationProxy, role.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        Assert.Equal(role, associationProxy.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Equal(association, roleProxy.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        this.Commit(transactionFlag);
                                                    }

                                                    association.Strategy.SetCompositeRole(relationType.RoleType, null);
                                                    this.Commit(transactionFlag);
                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Null(association.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Null(role.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        Assert.Null(associationProxy.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Null(roleProxy.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        this.Commit(transactionFlag);
                                                    }

                                                    // instantiate all, set associationProxy with role
                                                    association = this.Instantiate(associationId, manyFlag);
                                                    associationProxy = this.Instantiate(associationId, manyFlag);
                                                    role = this.Instantiate(roleId, manyFlag);
                                                    roleProxy = this.Instantiate(roleId, manyFlag);
                                                    association.Strategy.SetCompositeRole(relationType.RoleType, role);
                                                    this.Commit(transactionFlag);

                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Equal(roleProxy, association.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Equal(associationProxy, role.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        Assert.Equal(role, associationProxy.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Equal(association, roleProxy.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        this.Commit(transactionFlag);
                                                    }

                                                    association.Strategy.SetCompositeRole(relationType.RoleType, null);
                                                    this.Commit(transactionFlag);
                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Null(association.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Null(role.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        Assert.Null(associationProxy.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Null(roleProxy.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        this.Commit(transactionFlag);
                                                    }

                                                    // instantiate all, set associationProxy with roleProxy
                                                    association = this.Instantiate(associationId, manyFlag);
                                                    associationProxy = this.Instantiate(associationId, manyFlag);
                                                    role = this.Instantiate(roleId, manyFlag);
                                                    roleProxy = this.Instantiate(roleId, manyFlag);
                                                    association.Strategy.SetCompositeRole(relationType.RoleType, roleProxy);
                                                    this.Commit(transactionFlag);

                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Equal(roleProxy, association.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Equal(associationProxy, role.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        Assert.Equal(role, associationProxy.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Equal(association, roleProxy.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        this.Commit(transactionFlag);
                                                    }

                                                    association.Strategy.SetCompositeRole(relationType.RoleType, null);
                                                    this.Commit(transactionFlag);
                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Null(association.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Null(role.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        Assert.Null(associationProxy.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Null(roleProxy.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        this.Commit(transactionFlag);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                {
                                    // One2Many RelationTypes
                                    var relationTypes = this.GetOne2ManyRelations(this.GetMetaPopulation());
                                    for (var iRelation = 0; iRelation < relationTypes.Count(); iRelation++)
                                    {
                                        var relationType = relationTypes[iRelation];
                                        var associationTypes =
                                            relationType.AssociationType.ObjectType.DatabaseClasses.ToArray();
                                        for (var iAssociationType = 0;
                                             iAssociationType < associationTypes.Count();
                                             iAssociationType++)
                                        {
                                            var associationType = associationTypes[iAssociationType];
                                            var roleTypes = this.GetClasses(relationType);
                                            for (var iRoleType = 0; iRoleType < roleTypes.Count(); iRoleType++)
                                            {
                                                var roleType = roleTypes[iRoleType];
                                                var association = this.GetTransaction().Create(associationType);
                                                var role = this.GetTransaction().Create(roleType);
                                                var associationId = int.Parse(association.Strategy.ObjectId.ToString());
                                                var roleId = int.Parse(role.Strategy.ObjectId.ToString());
                                                var associationProxy = this.Instantiate(associationId, manyFlag);
                                                var roleProxy = this.Instantiate(roleId, manyFlag);
                                                this.Commit(transactionFlag);

                                                for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                {
                                                    // set association
                                                    association.Strategy.AddCompositesRole(relationType.RoleType, role);
                                                    this.Commit(transactionFlag);

                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Equal(roleProxy, ((IObject[])association.Strategy.GetCompositesRole<IObject>(relationType.RoleType))[0]);
                                                        Assert.Equal(associationProxy, role.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        Assert.Equal(role, ((IObject[])associationProxy.Strategy.GetCompositesRole<IObject>(relationType.RoleType))[0]);
                                                        Assert.Equal(association, roleProxy.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        this.Commit(transactionFlag);
                                                    }

                                                    association.Strategy.SetCompositesRole(relationType.RoleType, null);
                                                    this.Commit(transactionFlag);
                                                    for (var testRepeatIndex = 0; testRepeatIndex < testRepeat; testRepeatIndex++)
                                                    {
                                                        Assert.Empty((IObject[])association.Strategy.GetCompositesRole<IObject>(relationType.RoleType));
                                                        Assert.Null(role.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        Assert.Empty((IObject[])associationProxy.Strategy.GetCompositesRole<IObject>(relationType.RoleType));
                                                        Assert.Null(roleProxy.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        this.Commit(transactionFlag);
                                                    }

                                                    // instantiate all, set association with role
                                                    association = this.Instantiate(associationId, manyFlag);
                                                    associationProxy = this.Instantiate(associationId, manyFlag);
                                                    role = this.Instantiate(roleId, manyFlag);
                                                    roleProxy = this.Instantiate(roleId, manyFlag);
                                                    association.Strategy.AddCompositesRole(relationType.RoleType, role);
                                                    this.Commit(transactionFlag);

                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Equal(roleProxy, ((IObject[])association.Strategy.GetCompositesRole<IObject>(relationType.RoleType))[0]);
                                                        Assert.Equal(associationProxy, role.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        Assert.Equal(role, ((IObject[])associationProxy.Strategy.GetCompositesRole<IObject>(relationType.RoleType))[0]);
                                                        Assert.Equal(association, roleProxy.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        this.Commit(transactionFlag);
                                                    }

                                                    association.Strategy.SetCompositesRole(relationType.RoleType, null);
                                                    this.Commit(transactionFlag);
                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Empty((IObject[])association.Strategy.GetCompositesRole<IObject>(relationType.RoleType));
                                                        Assert.Null(role.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        Assert.Empty((IObject[])associationProxy.Strategy.GetCompositesRole<IObject>(relationType.RoleType));
                                                        Assert.Null(roleProxy.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        this.Commit(transactionFlag);
                                                    }

                                                    // instantiate all, set association with roleProxy
                                                    association = this.Instantiate(associationId, manyFlag);
                                                    associationProxy = this.Instantiate(associationId, manyFlag);
                                                    role = this.Instantiate(roleId, manyFlag);
                                                    roleProxy = this.Instantiate(roleId, manyFlag);
                                                    association.Strategy.AddCompositesRole(relationType.RoleType, roleProxy);
                                                    this.Commit(transactionFlag);

                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Equal(roleProxy, ((IObject[])association.Strategy.GetCompositesRole<IObject>(relationType.RoleType))[0]);
                                                        Assert.Equal(associationProxy, role.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        Assert.Equal(role, ((IObject[])associationProxy.Strategy.GetCompositesRole<IObject>(relationType.RoleType))[0]);
                                                        Assert.Equal(association, roleProxy.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        this.Commit(transactionFlag);
                                                    }

                                                    association.Strategy.SetCompositesRole(relationType.RoleType, null);
                                                    this.Commit(transactionFlag);
                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Empty((IObject[])association.Strategy.GetCompositesRole<IObject>(relationType.RoleType));
                                                        Assert.Null(role.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        Assert.Empty((IObject[])associationProxy.Strategy.GetCompositesRole<IObject>(relationType.RoleType));
                                                        Assert.Null(roleProxy.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        this.Commit(transactionFlag);
                                                    }

                                                    // instantiate all, set associationProxy with role
                                                    association = this.Instantiate(associationId, manyFlag);
                                                    associationProxy = this.Instantiate(associationId, manyFlag);
                                                    role = this.Instantiate(roleId, manyFlag);
                                                    roleProxy = this.Instantiate(roleId, manyFlag);
                                                    associationProxy.Strategy.AddCompositesRole(relationType.RoleType, role);
                                                    this.Commit(transactionFlag);

                                                    for (var testRepeatIndex = 0; testRepeatIndex < testRepeat; testRepeatIndex++)
                                                    {
                                                        Assert.Equal(roleProxy, ((IObject[])association.Strategy.GetCompositesRole<IObject>(relationType.RoleType))[0]);
                                                        Assert.Equal(associationProxy, role.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        Assert.Equal(role, ((IObject[])associationProxy.Strategy.GetCompositesRole<IObject>(relationType.RoleType))[0]);
                                                        Assert.Equal(association, roleProxy.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        this.Commit(transactionFlag);
                                                    }

                                                    association.Strategy.SetCompositesRole(relationType.RoleType, null);
                                                    this.Commit(transactionFlag);
                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Empty((IObject[])association.Strategy.GetCompositesRole<IObject>(relationType.RoleType));
                                                        Assert.Null(role.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        Assert.Empty((IObject[])associationProxy.Strategy.GetCompositesRole<IObject>(relationType.RoleType));
                                                        Assert.Null(roleProxy.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        this.Commit(transactionFlag);
                                                    }

                                                    // instantiate all, set associationProxy with roleProxy
                                                    association = this.Instantiate(associationId, manyFlag);
                                                    associationProxy = this.Instantiate(associationId, manyFlag);
                                                    role = this.Instantiate(roleId, manyFlag);
                                                    roleProxy = this.Instantiate(roleId, manyFlag);
                                                    associationProxy.Strategy.AddCompositesRole(relationType.RoleType, roleProxy);
                                                    this.Commit(transactionFlag);

                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Equal(roleProxy, ((IObject[])association.Strategy.GetCompositesRole<IObject>(relationType.RoleType))[0]);
                                                        Assert.Equal(associationProxy, role.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        Assert.Equal(role, ((IObject[])associationProxy.Strategy.GetCompositesRole<IObject>(relationType.RoleType))[0]);
                                                        Assert.Equal(association, roleProxy.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        this.Commit(transactionFlag);
                                                    }

                                                    association.Strategy.SetCompositesRole(relationType.RoleType, null);
                                                    this.Commit(transactionFlag);
                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Empty((IObject[])association.Strategy.GetCompositesRole<IObject>(relationType.RoleType));
                                                        Assert.Null(role.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        Assert.Empty((IObject[])associationProxy.Strategy.GetCompositesRole<IObject>(relationType.RoleType));
                                                        Assert.Null(roleProxy.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        this.Commit(transactionFlag);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                {
                                    // Many2One RelationTypes
                                    var relationTypes = this.GetMany2OneRelations(this.GetMetaPopulation());
                                    for (var iRelation = 0; iRelation < relationTypes.Count(); iRelation++)
                                    {
                                        var relationType = relationTypes[iRelation];
                                        var associationTypes = relationType.AssociationType.ObjectType.DatabaseClasses.ToArray();
                                        for (var iAssociationType = 0; iAssociationType < associationTypes.Count(); iAssociationType++)
                                        {
                                            var associationType = associationTypes[iAssociationType];
                                            var roleTypes = this.GetClasses(relationType);
                                            for (var iRoleType = 0; iRoleType < roleTypes.Count(); iRoleType++)
                                            {
                                                var roleType = roleTypes[iRoleType];
                                                var association = this.GetTransaction().Create(associationType);
                                                var role = this.GetTransaction().Create(roleType);
                                                var associationId = int.Parse(association.Strategy.ObjectId.ToString());
                                                var roleId = int.Parse(role.Strategy.ObjectId.ToString());
                                                var associationProxy = this.Instantiate(associationId, manyFlag);
                                                var roleProxy = this.Instantiate(roleId, manyFlag);
                                                this.Commit(transactionFlag);

                                                for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                {
                                                    // set association
                                                    association.Strategy.SetCompositeRole(relationType.RoleType, role);
                                                    this.Commit(transactionFlag);

                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Equal(roleProxy, association.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Equal(associationProxy, ((IObject[])role.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType))[0]);
                                                        Assert.Equal(role, associationProxy.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Equal(association, ((IObject[])roleProxy.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType))[0]);
                                                        this.Commit(transactionFlag);
                                                    }

                                                    association.Strategy.SetCompositeRole(relationType.RoleType, null);
                                                    this.Commit(transactionFlag);
                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Null(association.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Empty((IObject[])role.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                        Assert.Null(associationProxy.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Empty((IObject[])roleProxy.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                        this.Commit(transactionFlag);
                                                    }

                                                    // instantiate all, set association with role
                                                    association = this.Instantiate(associationId, manyFlag);
                                                    associationProxy = this.Instantiate(associationId, manyFlag);
                                                    role = this.Instantiate(roleId, manyFlag);
                                                    roleProxy = this.Instantiate(roleId, manyFlag);
                                                    association.Strategy.SetCompositeRole(relationType.RoleType, role);
                                                    this.Commit(transactionFlag);
                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Equal(roleProxy, association.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Equal(associationProxy, ((IObject[])role.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType))[0]);
                                                        Assert.Equal(role, associationProxy.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Equal(association, ((IObject[])roleProxy.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType))[0]);
                                                        this.Commit(transactionFlag);
                                                    }

                                                    association.Strategy.SetCompositeRole(relationType.RoleType, null);
                                                    this.Commit(transactionFlag);
                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Null(association.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Empty((IObject[])role.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                        Assert.Null(associationProxy.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Empty((IObject[])roleProxy.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                        this.Commit(transactionFlag);
                                                    }

                                                    // instantiate all, set association with roleProxy
                                                    association = this.Instantiate(associationId, manyFlag);
                                                    associationProxy = this.Instantiate(associationId, manyFlag);
                                                    role = this.Instantiate(roleId, manyFlag);
                                                    roleProxy = this.Instantiate(roleId, manyFlag);
                                                    association.Strategy.SetCompositeRole(relationType.RoleType, roleProxy);
                                                    this.Commit(transactionFlag);

                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Equal(roleProxy, association.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Equal(associationProxy, ((IObject[])role.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType))[0]);
                                                        Assert.Equal(role, associationProxy.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Equal(association, ((IObject[])roleProxy.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType))[0]);
                                                        this.Commit(transactionFlag);
                                                    }

                                                    association.Strategy.SetCompositeRole(relationType.RoleType, null);
                                                    this.Commit(transactionFlag);
                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Null(association.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Empty((IObject[])role.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                        Assert.Null(associationProxy.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Empty((IObject[])roleProxy.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                        this.Commit(transactionFlag);
                                                    }

                                                    // instantiate all, set associationProxy with role
                                                    association = this.Instantiate(associationId, manyFlag);
                                                    associationProxy = this.Instantiate(associationId, manyFlag);
                                                    role = this.Instantiate(roleId, manyFlag);
                                                    roleProxy = this.Instantiate(roleId, manyFlag);
                                                    association.Strategy.SetCompositeRole(relationType.RoleType, role);
                                                    this.Commit(transactionFlag);

                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Equal(roleProxy, association.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Equal(associationProxy, ((IObject[])role.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType))[0]);
                                                        Assert.Equal(role, associationProxy.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Equal(association, ((IObject[])roleProxy.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType))[0]);
                                                        this.Commit(transactionFlag);
                                                    }

                                                    association.Strategy.SetCompositeRole(relationType.RoleType, null);
                                                    this.Commit(transactionFlag);
                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Null(association.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Empty((IObject[])role.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                        Assert.Null(associationProxy.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Empty((IObject[])roleProxy.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                        this.Commit(transactionFlag);
                                                    }

                                                    // instantiate all, set associationProxy with roleProxy
                                                    association = this.Instantiate(associationId, manyFlag);
                                                    associationProxy = this.Instantiate(associationId, manyFlag);
                                                    role = this.Instantiate(roleId, manyFlag);
                                                    roleProxy = this.Instantiate(roleId, manyFlag);
                                                    association.Strategy.SetCompositeRole(relationType.RoleType, roleProxy);
                                                    this.Commit(transactionFlag);

                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Equal(roleProxy, association.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Equal(associationProxy, ((IObject[])role.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType))[0]);
                                                        Assert.Equal(role, associationProxy.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Equal(association, ((IObject[])roleProxy.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType))[0]);
                                                        this.Commit(transactionFlag);
                                                    }

                                                    association.Strategy.SetCompositeRole(relationType.RoleType, null);
                                                    this.Commit(transactionFlag);
                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Null(association.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Empty((IObject[])role.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                        Assert.Null(associationProxy.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Empty((IObject[])roleProxy.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                        this.Commit(transactionFlag);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                {
                                    // Many2Many RelationTypes
                                    var relationTypes = this.GetMany2ManyRelations(this.GetMetaPopulation());
                                    for (var iRelation = 0; iRelation < relationTypes.Count(); iRelation++)
                                    {
                                        var relationType = relationTypes[iRelation];
                                        var associationTypes =
                                            relationType.AssociationType.ObjectType.DatabaseClasses.ToArray();
                                        for (var iAssociationType = 0;
                                             iAssociationType < associationTypes.Count();
                                             iAssociationType++)
                                        {
                                            var associationType = associationTypes[iAssociationType];
                                            var roleTypes = this.GetClasses(relationType);
                                            for (var iRoleType = 0; iRoleType < roleTypes.Count(); iRoleType++)
                                            {
                                                var roleType = roleTypes[iRoleType];
                                                var association = this.GetTransaction().Create(associationType);
                                                var role = this.GetTransaction().Create(roleType);
                                                var associationId = int.Parse(association.Strategy.ObjectId.ToString());
                                                var roleId = int.Parse(role.Strategy.ObjectId.ToString());
                                                var associationProxy = this.Instantiate(associationId, manyFlag);
                                                var roleProxy = this.Instantiate(roleId, manyFlag);
                                                this.Commit(transactionFlag);

                                                for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                {
                                                    // set association
                                                    association.Strategy.AddCompositesRole(relationType.RoleType, role);
                                                    this.Commit(transactionFlag);

                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Equal(roleProxy, ((IObject[])association.Strategy.GetCompositesRole<IObject>(relationType.RoleType))[0]);
                                                        Assert.Equal(associationProxy, ((IObject[])role.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType))[0]);
                                                        Assert.Equal(role, ((IObject[])associationProxy.Strategy.GetCompositesRole<IObject>(relationType.RoleType))[0]);
                                                        Assert.Equal(association, ((IObject[])roleProxy.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType))[0]);
                                                        this.Commit(transactionFlag);
                                                    }

                                                    association.Strategy.SetCompositesRole(relationType.RoleType, null);
                                                    this.Commit(transactionFlag);

                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Empty((IObject[])association.Strategy.GetCompositesRole<IObject>(relationType.RoleType));
                                                        Assert.Empty((IObject[])role.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                        Assert.Empty((IObject[])associationProxy.Strategy.GetCompositesRole<IObject>(relationType.RoleType));
                                                        Assert.Empty((IObject[])roleProxy.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                        this.Commit(transactionFlag);
                                                    }

                                                    // instantiate all, set association with role
                                                    association = this.Instantiate(associationId, manyFlag);
                                                    associationProxy = this.Instantiate(associationId, manyFlag);
                                                    role = this.Instantiate(roleId, manyFlag);
                                                    roleProxy = this.Instantiate(roleId, manyFlag);
                                                    association.Strategy.AddCompositesRole(relationType.RoleType, role);
                                                    this.Commit(transactionFlag);

                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Equal(roleProxy, ((IObject[])association.Strategy.GetCompositesRole<IObject>(relationType.RoleType))[0]);
                                                        Assert.Equal(associationProxy, ((IObject[])role.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType))[0]);
                                                        Assert.Equal(role, ((IObject[])associationProxy.Strategy.GetCompositesRole<IObject>(relationType.RoleType))[0]);
                                                        Assert.Equal(association, ((IObject[])roleProxy.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType))[0]);
                                                        this.Commit(transactionFlag);
                                                    }

                                                    association.Strategy.SetCompositesRole(relationType.RoleType, null);
                                                    this.Commit(transactionFlag);
                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Empty((IObject[])association.Strategy.GetCompositesRole<IObject>(relationType.RoleType));
                                                        Assert.Empty((IObject[])role.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                        Assert.Empty((IObject[])associationProxy.Strategy.GetCompositesRole<IObject>(relationType.RoleType));
                                                        Assert.Empty((IObject[])roleProxy.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                        this.Commit(transactionFlag);
                                                    }

                                                    // instantiate all, set association with roleProxy
                                                    association = this.Instantiate(associationId, manyFlag);
                                                    associationProxy = this.Instantiate(associationId, manyFlag);
                                                    role = this.Instantiate(roleId, manyFlag);
                                                    roleProxy = this.Instantiate(roleId, manyFlag);
                                                    association.Strategy.AddCompositesRole(relationType.RoleType, roleProxy);
                                                    this.Commit(transactionFlag);

                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Equal(roleProxy, ((IObject[])association.Strategy.GetCompositesRole<IObject>(relationType.RoleType))[0]);
                                                        Assert.Equal(associationProxy, ((IObject[])role.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType))[0]);
                                                        Assert.Equal(role, ((IObject[])associationProxy.Strategy.GetCompositesRole<IObject>(relationType.RoleType))[0]);
                                                        Assert.Equal(association, ((IObject[])roleProxy.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType))[0]);
                                                        this.Commit(transactionFlag);
                                                    }

                                                    association.Strategy.SetCompositesRole(relationType.RoleType, null);
                                                    this.Commit(transactionFlag);
                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Empty((IObject[])association.Strategy.GetCompositesRole<IObject>(relationType.RoleType));
                                                        Assert.Empty((IObject[])role.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                        Assert.Empty((IObject[])associationProxy.Strategy.GetCompositesRole<IObject>(relationType.RoleType));
                                                        Assert.Empty((IObject[])roleProxy.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                        this.Commit(transactionFlag);
                                                    }

                                                    // instantiate all, set associationProxy with role
                                                    association = this.Instantiate(associationId, manyFlag);
                                                    associationProxy = this.Instantiate(associationId, manyFlag);
                                                    role = this.Instantiate(roleId, manyFlag);
                                                    roleProxy = this.Instantiate(roleId, manyFlag);
                                                    associationProxy.Strategy.AddCompositesRole(relationType.RoleType, role);
                                                    this.Commit(transactionFlag);

                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Equal(roleProxy, ((IObject[])association.Strategy.GetCompositesRole<IObject>(relationType.RoleType))[0]);
                                                        Assert.Equal(associationProxy, ((IObject[])role.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType))[0]);
                                                        Assert.Equal(role, ((IObject[])associationProxy.Strategy.GetCompositesRole<IObject>(relationType.RoleType))[0]);
                                                        Assert.Equal(association, ((IObject[])roleProxy.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType))[0]);
                                                        this.Commit(transactionFlag);
                                                    }

                                                    association.Strategy.SetCompositesRole(relationType.RoleType, null);
                                                    this.Commit(transactionFlag);
                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Empty((IObject[])association.Strategy.GetCompositesRole<IObject>(relationType.RoleType));
                                                        Assert.Empty((IObject[])role.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                        Assert.Empty((IObject[])associationProxy.Strategy.GetCompositesRole<IObject>(relationType.RoleType));
                                                        Assert.Empty((IObject[])roleProxy.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                        this.Commit(transactionFlag);
                                                    }

                                                    // instantiate all, set associationProxy with roleProxy
                                                    association = this.Instantiate(associationId, manyFlag);
                                                    associationProxy = this.Instantiate(associationId, manyFlag);
                                                    role = this.Instantiate(roleId, manyFlag);
                                                    roleProxy = this.Instantiate(roleId, manyFlag);
                                                    associationProxy.Strategy.AddCompositesRole(relationType.RoleType, roleProxy);
                                                    this.Commit(transactionFlag);

                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Equal(roleProxy, ((IObject[])association.Strategy.GetCompositesRole<IObject>(relationType.RoleType))[0]);
                                                        Assert.Equal(associationProxy, ((IObject[])role.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType))[0]);
                                                        Assert.Equal(role, ((IObject[])associationProxy.Strategy.GetCompositesRole<IObject>(relationType.RoleType))[0]);
                                                        Assert.Equal(association, ((IObject[])roleProxy.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType))[0]);
                                                        this.Commit(transactionFlag);
                                                    }

                                                    association.Strategy.SetCompositesRole(relationType.RoleType, null);
                                                    this.Commit(transactionFlag);
                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Empty((IObject[])association.Strategy.GetCompositesRole<IObject>(relationType.RoleType));
                                                        Assert.Empty((IObject[])role.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                        Assert.Empty((IObject[])associationProxy.Strategy.GetCompositesRole<IObject>(relationType.RoleType));
                                                        Assert.Empty((IObject[])roleProxy.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                        this.Commit(transactionFlag);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (this.IsRollbackSupported())
            {
                for (var iRepeat = 0; iRepeat < this.GetRepeats().Length; iRepeat++)
                {
                    var repeat = this.GetRepeats()[iRepeat];
                    for (var iTestRepeat = 0; iTestRepeat < this.GetTestRepeats().Length; iTestRepeat++)
                    {
                        var testRepeat = this.GetTestRepeats()[iTestRepeat];
                        for (var iAssertRepeat = 0; iAssertRepeat < this.GetAssertRepeats().Length; iAssertRepeat++)
                        {
                            for (var iManyFlag = 0; iManyFlag < this.manyFlags.Count(); iManyFlag++)
                            {
                                bool manyFlag = this.manyFlags[iManyFlag];
                                for (var iTestType = 0; iTestType < this.GetTestTypes().Length; iTestType++)
                                {
                                    var testType = this.GetTestTypes()[iTestType];
                                    {
                                        // Equality & Id's
                                        var anObject = this.GetTransaction().Create(testType);
                                        var id = int.Parse(anObject.Strategy.ObjectId.ToString());
                                        this.Instantiate(id, manyFlag);
                                        this.GetTransaction().Commit();

                                        for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                        {
                                            IObject sameObject = this.Instantiate(id, manyFlag);

                                            for (var testRepeatIndex = 0;
                                                 testRepeatIndex < testRepeat;
                                                 testRepeatIndex++)
                                            {
                                                Assert.Equal(anObject, sameObject);
                                                Assert.Equal(anObject.Strategy.ObjectId, sameObject.Strategy.ObjectId);
                                            }

                                            this.GetTransaction().Rollback();

                                            anObject = this.Instantiate(id, manyFlag);

                                            for (var testRepeatIndex = 0;
                                                 testRepeatIndex < testRepeat;
                                                 testRepeatIndex++)
                                            {
                                                Assert.Equal(anObject, sameObject);
                                                Assert.Equal(anObject.Strategy.ObjectId, sameObject.Strategy.ObjectId);
                                            }

                                            this.GetTransaction().Rollback();
                                        }
                                    }

                                    {
                                        // String RelationTypes
                                        IObject subject = this.GetTransaction().Create(testType);
                                        var id = int.Parse(subject.Strategy.ObjectId.ToString());
                                        this.Instantiate(id, manyFlag);
                                        this.GetTransaction().Commit();

                                        string valueA = this.ValueGenerator.GenerateString(100);
                                        string valueB = this.ValueGenerator.GenerateString(100);

                                        var testRoleTypes = this.GetStringRoles(testType);
                                        for (var testRoleTypeIndex = 0;
                                             testRoleTypeIndex < testRoleTypes.Count();
                                             testRoleTypeIndex++)
                                        {
                                            var testRoleType = testRoleTypes[testRoleTypeIndex];

                                            // set subject, set proxy
                                            subject.Strategy.SetUnitRole(testRoleType.RoleType, valueA);
                                            IObject proxy = this.Instantiate(id, manyFlag);

                                            this.GetTransaction().Rollback();

                                            Assert.False(subject.Strategy.ExistRole(testRoleType.RoleType));
                                            Assert.False(proxy.Strategy.ExistRole(testRoleType.RoleType));

                                            this.GetTransaction().Commit();

                                            // set subject, instantiate proxy
                                            subject.Strategy.SetUnitRole(testRoleType.RoleType, valueA);
                                            proxy = this.Instantiate(id, manyFlag);

                                            this.GetTransaction().Rollback();

                                            Assert.False(subject.Strategy.ExistRole(testRoleType.RoleType));
                                            Assert.False(proxy.Strategy.ExistRole(testRoleType.RoleType));

                                            this.GetTransaction().Commit();

                                            // instantiate both , set subject 
                                            subject = this.Instantiate(id, manyFlag);
                                            proxy = this.Instantiate(id, manyFlag);
                                            subject.Strategy.SetUnitRole(testRoleType.RoleType, valueA);

                                            this.GetTransaction().Rollback();

                                            Assert.False(subject.Strategy.ExistRole(testRoleType.RoleType));
                                            Assert.False(proxy.Strategy.ExistRole(testRoleType.RoleType));

                                            // instantiate both , set proxy 
                                            subject = this.Instantiate(id, manyFlag);
                                            proxy = this.Instantiate(id, manyFlag);
                                            proxy.Strategy.SetUnitRole(testRoleType.RoleType, valueB);

                                            this.GetTransaction().Rollback();

                                            Assert.False(subject.Strategy.ExistRole(testRoleType.RoleType));
                                            Assert.False(proxy.Strategy.ExistRole(testRoleType.RoleType));

                                            // instantiate both , set subject & proxy
                                            subject = this.Instantiate(id, manyFlag);
                                            proxy = this.Instantiate(id, manyFlag);
                                            subject.Strategy.SetUnitRole(testRoleType.RoleType, valueA);
                                            proxy.Strategy.SetUnitRole(testRoleType.RoleType, valueB);

                                            this.GetTransaction().Rollback();

                                            Assert.False(subject.Strategy.ExistRole(testRoleType.RoleType));
                                            Assert.False(proxy.Strategy.ExistRole(testRoleType.RoleType));

                                            // instantiate both , set proxy & subject  
                                            subject = this.Instantiate(id, manyFlag);
                                            proxy = this.Instantiate(id, manyFlag);
                                            proxy.Strategy.SetUnitRole(testRoleType.RoleType, valueB);
                                            subject.Strategy.SetUnitRole(testRoleType.RoleType, valueA);

                                            this.GetTransaction().Rollback();

                                            Assert.False(subject.Strategy.ExistRole(testRoleType.RoleType));
                                            Assert.False(proxy.Strategy.ExistRole(testRoleType.RoleType));
                                        }
                                    }
                                }

                                {
                                    // One2One RelationTypes
                                    var relationTypes = this.GetOne2OneRelations(this.GetMetaPopulation());
                                    for (var iRelation = 0; iRelation < relationTypes.Count(); iRelation++)
                                    {
                                        var relationType = relationTypes[iRelation];
                                        var associationTypes = relationType.AssociationType.ObjectType.DatabaseClasses.ToArray();
                                        for (var iAssociationType = 0; iAssociationType < associationTypes.Count(); iAssociationType++)
                                        {
                                            var associationType = associationTypes[iAssociationType];
                                            var roleTypes = this.GetClasses(relationType);
                                            for (var iRoleType = 0; iRoleType < roleTypes.Count(); iRoleType++)
                                            {
                                                var roleType = roleTypes[iRoleType];
                                                var association = this.GetTransaction().Create(associationType);
                                                var role = this.GetTransaction().Create(roleType);
                                                var associationId = int.Parse(association.Strategy.ObjectId.ToString());
                                                var roleId = int.Parse(role.Strategy.ObjectId.ToString());
                                                var associationProxy = this.Instantiate(associationId, manyFlag);
                                                var roleProxy = this.Instantiate(roleId, manyFlag);
                                                this.GetTransaction().Commit();

                                                for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                {
                                                    // set association
                                                    association.Strategy.SetCompositeRole(relationType.RoleType, role);
                                                    this.GetTransaction().Rollback();

                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Null(association.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Null(role.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        Assert.Null(associationProxy.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Null(roleProxy.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                    }

                                                    this.GetTransaction().Commit();

                                                    // instantiate all, set association with role
                                                    association = this.Instantiate(associationId, manyFlag);
                                                    associationProxy = this.Instantiate(associationId, manyFlag);
                                                    role = this.Instantiate(roleId, manyFlag);
                                                    roleProxy = this.Instantiate(roleId, manyFlag);
                                                    association.Strategy.SetCompositeRole(relationType.RoleType, role);
                                                    this.GetTransaction().Rollback();

                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Null(association.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Null(role.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        Assert.Null(associationProxy.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Null(roleProxy.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                    }

                                                    this.GetTransaction().Commit();

                                                    // instantiate all, set association with roleProxy
                                                    association = this.Instantiate(associationId, manyFlag);
                                                    associationProxy = this.Instantiate(associationId, manyFlag);
                                                    role = this.Instantiate(roleId, manyFlag);
                                                    roleProxy = this.Instantiate(roleId, manyFlag);
                                                    association.Strategy.SetCompositeRole(relationType.RoleType, roleProxy);
                                                    this.GetTransaction().Rollback();

                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Null(association.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Null(role.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        Assert.Null(associationProxy.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Null(roleProxy.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                    }

                                                    this.GetTransaction().Commit();

                                                    // instantiate all, set associationProxy with role
                                                    association = this.Instantiate(associationId, manyFlag);
                                                    associationProxy = this.Instantiate(associationId, manyFlag);
                                                    role = this.Instantiate(roleId, manyFlag);
                                                    roleProxy = this.Instantiate(roleId, manyFlag);
                                                    association.Strategy.SetCompositeRole(relationType.RoleType, role);
                                                    this.GetTransaction().Rollback();

                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Null(association.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Null(role.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        Assert.Null(associationProxy.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Null(roleProxy.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                    }

                                                    this.GetTransaction().Commit();

                                                    // instantiate all, set associationProxy with roleProxy
                                                    association = this.Instantiate(associationId, manyFlag);
                                                    associationProxy = this.Instantiate(associationId, manyFlag);
                                                    role = this.Instantiate(roleId, manyFlag);
                                                    roleProxy = this.Instantiate(roleId, manyFlag);
                                                    association.Strategy.SetCompositeRole(relationType.RoleType, roleProxy);
                                                    this.GetTransaction().Rollback();

                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Null(association.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Null(role.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        Assert.Null(associationProxy.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Null(roleProxy.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                    }

                                                    this.GetTransaction().Commit();
                                                }
                                            }
                                        }
                                    }
                                }

                                {
                                    // One2Many RelationTypes
                                    var relationTypes = this.GetOne2ManyRelations(this.GetMetaPopulation());
                                    for (var iRelation = 0; iRelation < relationTypes.Count(); iRelation++)
                                    {
                                        var relationType = relationTypes[iRelation];
                                        var associationTypes =
                                            relationType.AssociationType.ObjectType.DatabaseClasses.ToArray();
                                        for (var iAssociationType = 0;
                                             iAssociationType < associationTypes.Count();
                                             iAssociationType++)
                                        {
                                            var associationType = associationTypes[iAssociationType];
                                            var roleTypes = this.GetClasses(relationType);
                                            for (var iRoleType = 0; iRoleType < roleTypes.Count(); iRoleType++)
                                            {
                                                var roleType = roleTypes[iRoleType];
                                                var association = this.GetTransaction().Create(associationType);
                                                var role = this.GetTransaction().Create(roleType);
                                                var associationId = int.Parse(association.Strategy.ObjectId.ToString());
                                                var roleId = int.Parse(role.Strategy.ObjectId.ToString());
                                                var associationProxy = this.Instantiate(associationId, manyFlag);
                                                var roleProxy = this.Instantiate(roleId, manyFlag);
                                                this.GetTransaction().Commit();

                                                for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                {
                                                    // set association
                                                    association.Strategy.AddCompositesRole(relationType.RoleType, role);
                                                    this.GetTransaction().Rollback();

                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Empty((IObject[])association.Strategy.GetCompositesRole<IObject>(relationType.RoleType));
                                                        Assert.Null(role.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        Assert.Empty((IObject[])associationProxy.Strategy.GetCompositesRole<IObject>(relationType.RoleType));
                                                        Assert.Null(roleProxy.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                    }

                                                    this.GetTransaction().Commit();

                                                    // instantiate all, set association with role
                                                    association = this.Instantiate(associationId, manyFlag);
                                                    associationProxy = this.Instantiate(associationId, manyFlag);
                                                    role = this.Instantiate(roleId, manyFlag);
                                                    roleProxy = this.Instantiate(roleId, manyFlag);
                                                    association.Strategy.AddCompositesRole(relationType.RoleType, role);
                                                    this.GetTransaction().Rollback();

                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Empty((IObject[])association.Strategy.GetCompositesRole<IObject>(relationType.RoleType));
                                                        Assert.Null(role.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        Assert.Empty((IObject[])associationProxy.Strategy.GetCompositesRole<IObject>(relationType.RoleType));
                                                        Assert.Null(roleProxy.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                    }

                                                    this.GetTransaction().Commit();

                                                    // instantiate all, set association with roleProxy
                                                    association = this.Instantiate(associationId, manyFlag);
                                                    associationProxy = this.Instantiate(associationId, manyFlag);
                                                    role = this.Instantiate(roleId, manyFlag);
                                                    roleProxy = this.Instantiate(roleId, manyFlag);
                                                    association.Strategy.AddCompositesRole(relationType.RoleType, roleProxy);
                                                    this.GetTransaction().Rollback();

                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Empty((IObject[])association.Strategy.GetCompositesRole<IObject>(relationType.RoleType));
                                                        Assert.Null(role.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        Assert.Empty((IObject[])associationProxy.Strategy.GetCompositesRole<IObject>(relationType.RoleType));
                                                        Assert.Null(roleProxy.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                    }

                                                    this.GetTransaction().Commit();

                                                    // instantiate all, set associationProxy with role
                                                    association = this.Instantiate(associationId, manyFlag);
                                                    associationProxy = this.Instantiate(associationId, manyFlag);
                                                    role = this.Instantiate(roleId, manyFlag);
                                                    roleProxy = this.Instantiate(roleId, manyFlag);
                                                    associationProxy.Strategy.AddCompositesRole(relationType.RoleType, role);
                                                    this.GetTransaction().Rollback();

                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Empty((IObject[])association.Strategy.GetCompositesRole<IObject>(relationType.RoleType));
                                                        Assert.Null(role.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        Assert.Empty((IObject[])associationProxy.Strategy.GetCompositesRole<IObject>(relationType.RoleType));
                                                        Assert.Null(roleProxy.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                    }

                                                    this.GetTransaction().Commit();

                                                    // instantiate all, set associationProxy with roleProxy
                                                    association = this.Instantiate(associationId, manyFlag);
                                                    associationProxy = this.Instantiate(associationId, manyFlag);
                                                    role = this.Instantiate(roleId, manyFlag);
                                                    roleProxy = this.Instantiate(roleId, manyFlag);
                                                    associationProxy.Strategy.AddCompositesRole(relationType.RoleType, roleProxy);
                                                    this.GetTransaction().Rollback();

                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Empty((IObject[])association.Strategy.GetCompositesRole<IObject>(relationType.RoleType));
                                                        Assert.Null(role.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                        Assert.Empty((IObject[])associationProxy.Strategy.GetCompositesRole<IObject>(relationType.RoleType));
                                                        Assert.Null(roleProxy.Strategy.GetCompositeAssociation(relationType.AssociationType));
                                                    }

                                                    this.GetTransaction().Commit();
                                                }
                                            }
                                        }
                                    }
                                }

                                {
                                    // Many2One RelationTypes
                                    var relationTypes = this.GetMany2OneRelations(this.GetMetaPopulation());
                                    for (var iRelation = 0; iRelation < relationTypes.Count(); iRelation++)
                                    {
                                        var relationType = relationTypes[iRelation];
                                        var associationTypes = relationType.AssociationType.ObjectType.DatabaseClasses.ToArray();
                                        for (var iAssociationType = 0; iAssociationType < associationTypes.Count(); iAssociationType++)
                                        {
                                            var associationType = associationTypes[iAssociationType];
                                            var roleTypes = this.GetClasses(relationType);
                                            for (var iRoleType = 0; iRoleType < roleTypes.Count(); iRoleType++)
                                            {
                                                var roleType = roleTypes[iRoleType];
                                                var association = this.GetTransaction().Create(associationType);
                                                var role = this.GetTransaction().Create(roleType);
                                                var associationId = int.Parse(association.Strategy.ObjectId.ToString());
                                                var roleId = int.Parse(role.Strategy.ObjectId.ToString());
                                                var associationProxy = this.Instantiate(associationId, manyFlag);
                                                var roleProxy = this.Instantiate(roleId, manyFlag);
                                                this.GetTransaction().Commit();

                                                for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                {
                                                    // set association
                                                    association.Strategy.SetCompositeRole(relationType.RoleType, role);
                                                    this.GetTransaction().Rollback();

                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Null(association.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Empty((IObject[])role.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                        Assert.Null(associationProxy.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Empty((IObject[])roleProxy.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                    }

                                                    this.GetTransaction().Commit();

                                                    // instantiate all, set association with role
                                                    association = this.Instantiate(associationId, manyFlag);
                                                    associationProxy = this.Instantiate(associationId, manyFlag);
                                                    role = this.Instantiate(roleId, manyFlag);
                                                    roleProxy = this.Instantiate(roleId, manyFlag);
                                                    association.Strategy.SetCompositeRole(relationType.RoleType, role);
                                                    this.GetTransaction().Rollback();

                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Null(association.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Empty((IObject[])role.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                        Assert.Null(associationProxy.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Empty((IObject[])roleProxy.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                    }

                                                    this.GetTransaction().Commit();

                                                    // instantiate all, set association with roleProxy
                                                    association = this.Instantiate(associationId, manyFlag);
                                                    associationProxy = this.Instantiate(associationId, manyFlag);
                                                    role = this.Instantiate(roleId, manyFlag);
                                                    roleProxy = this.Instantiate(roleId, manyFlag);
                                                    association.Strategy.SetCompositeRole(relationType.RoleType, roleProxy);
                                                    this.GetTransaction().Rollback();

                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Null(association.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Empty((IObject[])role.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                        Assert.Null(associationProxy.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Empty((IObject[])roleProxy.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                    }

                                                    this.GetTransaction().Commit();

                                                    // instantiate all, set associationProxy with role
                                                    association = this.Instantiate(associationId, manyFlag);
                                                    associationProxy = this.Instantiate(associationId, manyFlag);
                                                    role = this.Instantiate(roleId, manyFlag);
                                                    roleProxy = this.Instantiate(roleId, manyFlag);
                                                    association.Strategy.SetCompositeRole(relationType.RoleType, role);
                                                    this.GetTransaction().Rollback();

                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Null(association.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Empty((IObject[])role.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                        Assert.Null(associationProxy.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Empty((IObject[])roleProxy.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                    }

                                                    this.GetTransaction().Commit();

                                                    // instantiate all, set associationProxy with roleProxy
                                                    association = this.Instantiate(associationId, manyFlag);
                                                    associationProxy = this.Instantiate(associationId, manyFlag);
                                                    role = this.Instantiate(roleId, manyFlag);
                                                    roleProxy = this.Instantiate(roleId, manyFlag);
                                                    association.Strategy.SetCompositeRole(relationType.RoleType, roleProxy);
                                                    this.GetTransaction().Rollback();

                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Null(association.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Empty((IObject[])role.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                        Assert.Null(associationProxy.Strategy.GetRole(relationType.RoleType));
                                                        Assert.Empty((IObject[])roleProxy.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                    }

                                                    this.GetTransaction().Commit();
                                                }
                                            }
                                        }
                                    }
                                }

                                {
                                    // Many2Many RelationTypes
                                    var relationTypes = this.GetMany2ManyRelations(this.GetMetaPopulation());
                                    for (var iRelation = 0; iRelation < relationTypes.Count(); iRelation++)
                                    {
                                        var relationType = relationTypes[iRelation];
                                        var associationTypes = relationType.AssociationType.ObjectType.DatabaseClasses.ToArray();
                                        for (var iAssociationType = 0; iAssociationType < associationTypes.Count(); iAssociationType++)
                                        {
                                            var associationType = associationTypes[iAssociationType];
                                            var roleTypes = this.GetClasses(relationType);
                                            for (var iRoleType = 0; iRoleType < roleTypes.Count(); iRoleType++)
                                            {
                                                var roleType = roleTypes[iRoleType];
                                                var association = this.GetTransaction().Create(associationType);
                                                var role = this.GetTransaction().Create(roleType);
                                                var associationId = int.Parse(association.Strategy.ObjectId.ToString());
                                                var roleId = int.Parse(role.Strategy.ObjectId.ToString());
                                                var associationProxy = this.Instantiate(associationId, manyFlag);
                                                var roleProxy = this.Instantiate(roleId, manyFlag);
                                                this.GetTransaction().Commit();

                                                for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                {
                                                    // set association
                                                    association.Strategy.AddCompositesRole(relationType.RoleType, role);
                                                    this.GetTransaction().Rollback();

                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Empty((IObject[])association.Strategy.GetCompositesRole<IObject>(relationType.RoleType));
                                                        Assert.Empty((IObject[])role.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                        Assert.Empty((IObject[])associationProxy.Strategy.GetCompositesRole<IObject>(relationType.RoleType));
                                                        Assert.Empty((IObject[])roleProxy.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                    }

                                                    this.GetTransaction().Commit();

                                                    // instantiate all, set association with role
                                                    association = this.Instantiate(associationId, manyFlag);
                                                    associationProxy = this.Instantiate(associationId, manyFlag);
                                                    role = this.Instantiate(roleId, manyFlag);
                                                    roleProxy = this.Instantiate(roleId, manyFlag);
                                                    association.Strategy.AddCompositesRole(relationType.RoleType, role);
                                                    this.GetTransaction().Rollback();

                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Empty((IObject[])association.Strategy.GetCompositesRole<IObject>(relationType.RoleType));
                                                        Assert.Empty((IObject[])role.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                        Assert.Empty((IObject[])associationProxy.Strategy.GetCompositesRole<IObject>(relationType.RoleType));
                                                        Assert.Empty((IObject[])roleProxy.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                    }

                                                    this.GetTransaction().Commit();

                                                    // instantiate all, set association with roleProxy
                                                    association = this.Instantiate(associationId, manyFlag);
                                                    associationProxy = this.Instantiate(associationId, manyFlag);
                                                    role = this.Instantiate(roleId, manyFlag);
                                                    roleProxy = this.Instantiate(roleId, manyFlag);
                                                    association.Strategy.AddCompositesRole(relationType.RoleType, roleProxy);
                                                    this.GetTransaction().Rollback();

                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Empty((IObject[])association.Strategy.GetCompositesRole<IObject>(relationType.RoleType));
                                                        Assert.Empty((IObject[])role.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                        Assert.Empty((IObject[])associationProxy.Strategy.GetCompositesRole<IObject>(relationType.RoleType));
                                                        Assert.Empty((IObject[])roleProxy.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                    }

                                                    this.GetTransaction().Commit();

                                                    // instantiate all, set associationProxy with role
                                                    association = this.Instantiate(associationId, manyFlag);
                                                    associationProxy = this.Instantiate(associationId, manyFlag);
                                                    role = this.Instantiate(roleId, manyFlag);
                                                    roleProxy = this.Instantiate(roleId, manyFlag);
                                                    associationProxy.Strategy.AddCompositesRole(relationType.RoleType, role);
                                                    this.GetTransaction().Rollback();

                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Empty((IObject[])association.Strategy.GetCompositesRole<IObject>(relationType.RoleType));
                                                        Assert.Empty((IObject[])role.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                        Assert.Empty((IObject[])associationProxy.Strategy.GetCompositesRole<IObject>(relationType.RoleType));
                                                        Assert.Empty((IObject[])roleProxy.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                    }

                                                    this.GetTransaction().Commit();

                                                    // instantiate all, set associationProxy with roleProxy
                                                    association = this.Instantiate(associationId, manyFlag);
                                                    associationProxy = this.Instantiate(associationId, manyFlag);
                                                    role = this.Instantiate(roleId, manyFlag);
                                                    roleProxy = this.Instantiate(roleId, manyFlag);
                                                    associationProxy.Strategy.AddCompositesRole(relationType.RoleType, roleProxy);
                                                    this.GetTransaction().Rollback();

                                                    for (var testRepeatIndex = 0;
                                                         testRepeatIndex < testRepeat;
                                                         testRepeatIndex++)
                                                    {
                                                        Assert.Empty((IObject[])association.Strategy.GetCompositesRole<IObject>(relationType.RoleType));
                                                        Assert.Empty((IObject[])role.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                        Assert.Empty((IObject[])associationProxy.Strategy.GetCompositesRole<IObject>(relationType.RoleType));
                                                        Assert.Empty((IObject[])roleProxy.Strategy.GetCompositesAssociation<IObject>(relationType.AssociationType));
                                                    }

                                                    this.GetTransaction().Commit();
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        [Fact]
        [Trait("Category", "Dynamic")]
        public void InstantiateMany()
        {
            for (var iRepeat = 0; iRepeat < this.GetRepeats().Length; iRepeat++)
            {
                var repeat = this.GetRepeats()[iRepeat];
                for (var iTestRepeat = 0; iTestRepeat < this.GetTestRepeats().Length; iTestRepeat++)
                {
                    for (var iAssertRepeat = 0; iAssertRepeat < this.GetAssertRepeats().Length; iAssertRepeat++)
                    {
                        for (var transactionFlagIndex = 0; transactionFlagIndex < this.GetBooleanFlags().Length; transactionFlagIndex++)
                        {
                            var transactionFlag = this.GetBooleanFlags()[transactionFlagIndex];

                            for (var iTestType = 0; iTestType < this.GetTestTypes().Length; iTestType++)
                            {
                                var testType = this.GetTestTypes()[iTestType];

                                var ids = new string[10];
                                for (var i = 0; i < 10; i++)
                                {
                                    var anObject = this.GetTransaction().Create(testType);
                                    ids[i] = anObject.Strategy.ObjectId.ToString();
                                }

                                for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                {
                                    var allorsObjects = this.GetTransaction().Instantiate(ids);

                                    for (var iAllorsType = 0; iAllorsType < allorsObjects.Count(); iAllorsType++)
                                    {
                                        var allorsObject = allorsObjects[iAllorsType];
                                        Assert.Contains(allorsObject.Strategy.ObjectId.ToString(), ids);
                                    }

                                    this.Commit(transactionFlag);
                                }

                                ids = Array.Empty<string>();
                                for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                {
                                    var allorsObjects = this.GetTransaction().Instantiate(ids);
                                    Assert.Empty(allorsObjects);
                                    this.Commit(transactionFlag);
                                }

                                ids = new[] { (int.MaxValue - 1).ToString() };
                                for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                {
                                    var allorsObjects = this.GetTransaction().Instantiate(ids);
                                    Assert.Empty(allorsObjects);
                                    this.Commit(transactionFlag);
                                }
                            }
                        }
                    }
                }
            }

            if (this.IsRollbackSupported())
            {
                for (var iRepeat = 0; iRepeat < this.GetRepeats().Length; iRepeat++)
                {
                    var repeat = this.GetRepeats()[iRepeat];
                    for (var iTestRepeat = 0; iTestRepeat < this.GetTestRepeats().Length; iTestRepeat++)
                    {
                        for (var iAssertRepeat = 0; iAssertRepeat < this.GetAssertRepeats().Length; iAssertRepeat++)
                        {
                            for (var iTestType = 0; iTestType < this.GetTestTypes().Length; iTestType++)
                            {
                                var testType = this.GetTestTypes()[iTestType];

                                var ids = new string[10];
                                for (var i = 0; i < 10; i++)
                                {
                                    var anObject = this.GetTransaction().Create(testType);
                                    ids[i] = anObject.Strategy.ObjectId.ToString();
                                }

                                this.GetTransaction().Rollback();

                                for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                {
                                    var allorsObjects = this.GetTransaction().Instantiate(ids);

                                    for (var iAllorsType = 0; iAllorsType < allorsObjects.Count(); iAllorsType++)
                                    {
                                        Assert.Empty(allorsObjects);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        [Fact]
        [Trait("Category", "Dynamic")]
        public void IsDeleted()
        {
            for (var iRepeat = 0; iRepeat < this.GetRepeats().Length; iRepeat++)
            {
                var repeat = this.GetRepeats()[iRepeat];
                for (var iTestRepeat = 0; iTestRepeat < this.GetTestRepeats().Length; iTestRepeat++)
                {
                    for (var iAssertRepeat = 0; iAssertRepeat < this.GetAssertRepeats().Length; iAssertRepeat++)
                    {
                        for (var transactionFlagIndex = 0; transactionFlagIndex < this.GetBooleanFlags().Length; transactionFlagIndex++)
                        {
                            var transactionFlag = this.GetBooleanFlags()[transactionFlagIndex];

                            for (var secondTransactionFlagIndex = 0; secondTransactionFlagIndex < this.GetBooleanFlags().Length; secondTransactionFlagIndex++)
                            {
                                var secondTransactionFlag = this.GetBooleanFlags()[secondTransactionFlagIndex];
                                for (var thirdTransactionFlagIndex = 0; thirdTransactionFlagIndex < this.GetBooleanFlags().Length; thirdTransactionFlagIndex++)
                                {
                                    for (var fourthTransactionFlagIndex = 0; fourthTransactionFlagIndex < this.GetBooleanFlags().Length; fourthTransactionFlagIndex++)
                                    {
                                        for (var useRollbackFlagIndex = 0; useRollbackFlagIndex < this.GetBooleanFlags().Length; useRollbackFlagIndex++)
                                        {
                                            var useRollbackFlag = this.GetBooleanFlags()[useRollbackFlagIndex];
                                            if (!this.IsRollbackSupported())
                                            {
                                                useRollbackFlag = false;
                                            }

                                            for (var iTestType = 0; iTestType < this.GetTestTypes().Length; iTestType++)
                                            {
                                                var testType = this.GetTestTypes()[iTestType];

                                                // Without delete
                                                var allorsObject = this.GetTransaction().Create(testType);
                                                for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                {
                                                    Assert.False(allorsObject.Strategy.IsDeleted);
                                                }

                                                this.Commit(secondTransactionFlag);
                                                for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                {
                                                    Assert.False(allorsObject.Strategy.IsDeleted);
                                                    if (useRollbackFlag)
                                                    {
                                                        this.Rollback(transactionFlag);
                                                    }
                                                    else
                                                    {
                                                        this.Commit(transactionFlag);
                                                    }
                                                }

                                                allorsObject = this.GetTransaction().Create(testType);
                                                string id = allorsObject.Strategy.ObjectId.ToString();
                                                this.Commit(secondTransactionFlag);
                                                allorsObject = this.GetTransaction().Instantiate(id);

                                                for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                {
                                                    Assert.False(allorsObject.Strategy.IsDeleted);
                                                    if (useRollbackFlag)
                                                    {
                                                        this.Rollback(transactionFlag);
                                                    }
                                                    else
                                                    {
                                                        this.Commit(transactionFlag);
                                                    }
                                                }

                                                IObject proxy = this.GetTransaction().Create(testType);
                                                this.Commit(secondTransactionFlag);

                                                // AllorsObject subject = GetTransaction().instantiate( testType, id);
                                                for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                                {
                                                    Assert.False(proxy.Strategy.IsDeleted);
                                                    if (useRollbackFlag)
                                                    {
                                                        this.Rollback(transactionFlag);
                                                    }
                                                    else
                                                    {
                                                        this.Commit(transactionFlag);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (this.IsRollbackSupported())
            {
                for (var iRepeat = 0; iRepeat < this.GetRepeats().Length; iRepeat++)
                {
                    var repeat = this.GetRepeats()[iRepeat];
                    for (var iTestRepeat = 0; iTestRepeat < this.GetTestRepeats().Length; iTestRepeat++)
                    {
                        for (var iAssertRepeat = 0; iAssertRepeat < this.GetAssertRepeats().Length; iAssertRepeat++)
                        {
                            for (var iTestType = 0; iTestType < this.GetTestTypes().Length; iTestType++)
                            {
                                var testType = this.GetTestTypes()[iTestType];

                                for (var useRollbackFlagIndex = 0; useRollbackFlagIndex < this.GetBooleanFlags().Length; useRollbackFlagIndex++)
                                {
                                    var useRollbackFlag = this.GetBooleanFlags()[useRollbackFlagIndex];

                                    for (var transactionFlagIndex = 0; transactionFlagIndex < this.GetBooleanFlags().Length; transactionFlagIndex++)
                                    {
                                        var transactionFlag = this.GetBooleanFlags()[transactionFlagIndex];

                                        for (var secondTransactionFlagIndex = 0; secondTransactionFlagIndex < this.GetBooleanFlags().Length; secondTransactionFlagIndex++)
                                        {
                                            // Without delete
                                            var allorsObject = this.GetTransaction().Create(testType);
                                            for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                            {
                                                Assert.False(allorsObject.Strategy.IsDeleted);
                                            }

                                            this.GetTransaction().Rollback();
                                            for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                            {
                                                Assert.True(allorsObject.Strategy.IsDeleted);
                                                if (useRollbackFlag)
                                                {
                                                    this.Rollback(transactionFlag);
                                                }
                                                else
                                                {
                                                    this.Commit(transactionFlag);
                                                }
                                            }

                                            allorsObject = this.GetTransaction().Create(testType);
                                            string id = allorsObject.Strategy.ObjectId.ToString();
                                            this.GetTransaction().Rollback();
                                            allorsObject = this.GetTransaction().Instantiate(id);

                                            for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                            {
                                                Assert.Null(allorsObject);
                                                if (useRollbackFlag)
                                                {
                                                    this.Rollback(transactionFlag);
                                                }
                                                else
                                                {
                                                    this.Commit(transactionFlag);
                                                }
                                            }

                                            IObject proxy = this.GetTransaction().Create(testType);
                                            id = proxy.Strategy.ObjectId.ToString();
                                            this.GetTransaction().Rollback();
                                            this.GetTransaction().Instantiate(id);

                                            for (var repeatIndex = 0; repeatIndex < repeat; repeatIndex++)
                                            {
                                                Assert.True(proxy.Strategy.IsDeleted);
                                                if (useRollbackFlag)
                                                {
                                                    this.Rollback(transactionFlag);
                                                }
                                                else
                                                {
                                                    this.Commit(transactionFlag);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void GetUnit(IObject allorsObject, IRelationType relationType, Units values)
        {
            var unit = relationType.RoleType.ObjectType as Unit;

            if (unit.IsString)
            {
                values.String = (string)allorsObject.Strategy.GetUnitRole(relationType.RoleType);
            }
            else if (unit.IsInteger)
            {
                values.Integer = (int)allorsObject.Strategy.GetUnitRole(relationType.RoleType);
            }
            else if (unit.IsDecimal)
            {
                values.Decimal = (decimal)allorsObject.Strategy.GetUnitRole(relationType.RoleType);
            }
            else if (unit.IsFloat)
            {
                values.Float = (double)allorsObject.Strategy.GetUnitRole(relationType.RoleType);
            }
            else if (unit.IsBoolean)
            {
                values.Boolean = (bool)allorsObject.Strategy.GetUnitRole(relationType.RoleType);
            }
            else if (unit.IsDateTime)
            {
                values.DateTime = (DateTime)allorsObject.Strategy.GetUnitRole(relationType.RoleType);
            }
            else if (unit.IsUnique)
            {
                values.Unique = (Guid)allorsObject.Strategy.GetUnitRole(relationType.RoleType);
            }
            else if (unit.IsBinary)
            {
                values.Binary = (byte[])allorsObject.Strategy.GetUnitRole(relationType.RoleType);
            }
        }

        public void SetUnit(IObject allorsObject, IRelationType relationType, Units values)
        {
            var unitType = relationType.RoleType.ObjectType as Unit;

            if (unitType.IsString)
            {
                allorsObject.Strategy.SetUnitRole(relationType.RoleType, values.String);
            }
            else if (unitType.IsInteger)
            {
                allorsObject.Strategy.SetUnitRole(relationType.RoleType, values.Integer);
            }
            else if (unitType.IsDecimal)
            {
                allorsObject.Strategy.SetUnitRole(relationType.RoleType, values.Decimal);
            }
            else if (unitType.IsFloat)
            {
                allorsObject.Strategy.SetUnitRole(relationType.RoleType, values.Float);
            }
            else if (unitType.IsBoolean)
            {
                allorsObject.Strategy.SetUnitRole(relationType.RoleType, values.Boolean);
            }
            else if (unitType.IsDateTime)
            {
                allorsObject.Strategy.SetUnitRole(relationType.RoleType, values.DateTime);
            }
            else if (unitType.IsUnique)
            {
                allorsObject.Strategy.SetUnitRole(relationType.RoleType, values.Unique);
            }
            else if (unitType.IsBinary)
            {
                allorsObject.Strategy.SetUnitRole(relationType.RoleType, values.Binary);
            }
        }

        private IClass[] GetTestTypes() => this.GetMetaPopulation().Classes.ToArray();

        private IObject Instantiate(int id, bool many)
        {
            if (many)
            {
                string[] ids = { id.ToString(CultureInfo.InvariantCulture) };
                var results = this.GetTransaction().Instantiate(ids);
                if (results.Count() > 0)
                {
                    return results[0];
                }

                return null;
            }

            return this.GetTransaction().Instantiate(id.ToString(CultureInfo.InvariantCulture));
        }
    }
}
