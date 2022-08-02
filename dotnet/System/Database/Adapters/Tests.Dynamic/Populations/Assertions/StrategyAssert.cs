// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StrategyAssert.cs" company="Allors bvba">
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
    using System;
    using Meta;

    using Xunit;

    public class StrategyAssert
    {
        public static void AssociationExistHasException(IObject allorsObject, IAssociationType associationType)
        {
            bool exceptionOccured = false;
            try
            {
                object o = allorsObject.Strategy.GetAssociation(associationType);
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
            bool exceptionOccured = false;
            try
            {
                object o = allorsObject.Strategy.GetAssociation(associationType);
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
            bool exceptionOccured = false;
            try
            {
                object o = allorsObject.Strategy.ExistRole(roleType);
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
            bool exceptionOccured = false;
            try
            {
                object o = allorsObject.Strategy.GetRole(roleType);
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
            foreach (IRoleType roleType in roleTypes)
            {
                if (!allorsObject.Strategy.Class.ExistRoleType(roleType))
                {
                    Assert.True(false); // Fail
                }
            }

            foreach (IRoleType roleType in allorsObject.Strategy.Class.DatabaseRoleTypes)
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
