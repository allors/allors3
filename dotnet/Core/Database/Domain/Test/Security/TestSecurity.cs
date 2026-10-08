// <copyright file="TestSecurity.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Linq;
    using Meta;

    /// <summary>
    /// Changes to the security of the test population that the workspace tests make between two
    /// pulls, in the database, so that a grant or a revocation changes version: the local
    /// profile applies them directly, the test server's TestController on a GET.
    /// </summary>
    public static class TestSecurity
    {
        /// <summary>
        /// Takes the permission for the operation on the role type of the relation type with the
        /// tag away from the Administrator role; the grant of the administrators derives its
        /// effective permissions again and changes version.
        /// </summary>
        public static void RemoveAdministratorPermission(ITransaction transaction, string relationTypeTag, Operations operation)
        {
            var permission = FindPermission(transaction, relationTypeTag, operation);
            new Roles(transaction).Administrator.RemovePermission(permission);
            transaction.Derive();
            transaction.Commit();
        }

        /// <summary>
        /// Adds the permission for the operation on the role type of the relation type with the
        /// tag to the revocation that the Denied objects of the test population carry, which
        /// changes the revocation's version.
        /// </summary>
        public static void DenyPermission(ITransaction transaction, string relationTypeTag, Operations operation)
        {
            var permission = FindPermission(transaction, relationTypeTag, operation);
            var denied = transaction.Extent<Denied>().FirstOrDefault() ?? throw new InvalidOperationException("The test population has no Denied object: set up the population with security first.");
            var revocation = denied.Revocations.FirstOrDefault() ?? throw new InvalidOperationException("The Denied object of the test population carries no revocation.");
            revocation.AddDeniedPermission(permission);
            transaction.Derive();
            transaction.Commit();
        }

        private static Permission FindPermission(ITransaction transaction, string relationTypeTag, Operations operation)
        {
            var m = transaction.Database.MetaPopulation;
            if (m.FindByTag(relationTypeTag) is not IRelationType relationType)
            {
                throw new ArgumentException($"'{relationTypeTag}' is not the tag of a relation type of this database.", nameof(relationTypeTag));
            }

            return new Permissions(transaction).Extent().FirstOrDefault(v => v.Operation == operation && v.OperandType.Equals(relationType.RoleType))
                   ?? throw new ArgumentException($"There is no {operation} permission on {relationType.RoleType}.", nameof(operation));
        }
    }
}
