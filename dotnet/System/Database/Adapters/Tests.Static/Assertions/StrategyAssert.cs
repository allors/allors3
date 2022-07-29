// <copyright file="StrategyAssert.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters
{
    using System;
    using System.Linq;
    using Meta;

    using Xunit;

    public class StrategyAssert
    {
        public static void AssociationExistHasException(IObject allorsObject, IAssociationType associationType)
        {
            var exceptionOccured = false;
            try
            {
                if (associationType.IsOne)
                {
                    allorsObject.Strategy.GetCompositeAssociation(associationType);
                }
                else
                {
                    allorsObject.Strategy.GetCompositesAssociation<IObject>(associationType).ToArray();
                }
            }
            catch
            {
                exceptionOccured = true;
            }

            if (!exceptionOccured)
            {
                Assert.True(false, "Exist didn't threw an Exception for association " + associationType);
            }
        }

        public static void AssociationGetHasException(IObject allorsObject, IAssociationType associationType)
        {
            var exceptionOccured = false;
            try
            {
                if (associationType.IsOne)
                {
                    allorsObject.Strategy.GetCompositeAssociation(associationType);
                }
                else
                {
                    allorsObject.Strategy.GetCompositesAssociation<IObject>(associationType).ToArray();
                }
            }
            catch
            {
                exceptionOccured = true;
            }

            if (!exceptionOccured)
            {
                Assert.True(false); // Fail
            }
        }

        public static void AssociationsExistExclusive(IObject allorsObject, params IAssociationType[] associationTypes)
        {
            foreach (var associationType in associationTypes)
            {
                if (!allorsObject.Strategy.Class.ExistAssociationType(associationType))
                {
                    Assert.True(false); // Fail
                }
            }

            foreach (var associationType in allorsObject.Strategy.Class.DatabaseAssociationTypes)
            {
                if (Array.IndexOf(associationTypes, associationType) >= 0)
                {
                    if (!allorsObject.Strategy.ExistAssociation(associationType))
                    {
                        Assert.True(false); // Fail
                    }
                }
                else if (allorsObject.Strategy.ExistAssociation(associationType))
                {
                    if (allorsObject.Strategy.ExistAssociation(associationType))
                    {
                        Assert.True(false); // Fail
                    }
                }
            }
        }

        public static void RoleExistHasException(IObject allorsObject, IRoleType roleType)
        {
            var exceptionOccured = false;
            try
            {
                if (roleType.IsOne)
                {
                    allorsObject.Strategy.GetRole(roleType);
                }
                else
                {
                    allorsObject.Strategy.GetCompositesRole<IObject>(roleType).ToArray();
                }
            }
            catch
            {
                exceptionOccured = true;
            }

            if (!exceptionOccured)
            {
                Assert.True(false, "Exist didn't threw an Exception for role " + roleType);
            }
        }

        public static void RoleGetHasException(IObject allorsObject, IRoleType roleType)
        {
            var exceptionOccured = false;
            try
            {
                if (roleType.IsOne)
                {
                    allorsObject.Strategy.GetRole(roleType);
                }
                else
                {
                    allorsObject.Strategy.GetCompositesRole<IObject>(roleType).ToArray();
                }
            }
            catch
            {
                exceptionOccured = true;
            }

            if (!exceptionOccured)
            {
                Assert.True(false); // Fail
            }
        }

        public static void RolesExistExclusive(IObject allorsObject, params IRoleType[] roleTypes)
        {
            foreach (var roleType in roleTypes)
            {
                if (!allorsObject.Strategy.Class.ExistRoleType(roleType))
                {
                    Assert.True(false); // Fail
                }
            }

            foreach (var roleType in allorsObject.Strategy.Class.DatabaseRoleTypes)
            {
                if (Array.IndexOf(roleTypes, roleType) >= 0)
                {
                    if (!allorsObject.Strategy.ExistRole(roleType))
                    {
                        Assert.True(false); // Fail
                    }
                }
                else if (allorsObject.Strategy.ExistRole(roleType))
                {
                    Assert.True(false); // Fail
                }
            }
        }
    }
}
