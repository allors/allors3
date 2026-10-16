// <copyright file="ReadScopeFactory.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System;
    using System.Threading;
    using Allors.Protocol.Json.Api.Read;
    using Security;
    using SqlDatabase = Adapters.Sql.Database;

    /// <summary>
    /// Establishes trusted scope and fresh authorization on a privately owned SQL snapshot.
    /// This foundation does not execute selections or build a response.
    /// </summary>
    public sealed class ReadScopeFactory
    {
        private readonly SqlDatabase database;
        private readonly ReadConfiguration configuration;

        public ReadScopeFactory(IDatabase database, ReadConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(database);
            this.database = database as SqlDatabase ?? throw new NotSupportedException("Protected workspace reads require a supported SQL adapter. Memory remains a domain-test adapter.");
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// The host captures its workspace and authenticated identity after admission has committed.
        /// The resolver must only read the supplied transaction and must perform no external work.
        /// Context is an exact, opaque partition string; expected scope is only a consistency check.
        /// </summary>
        public ReadContext Open(string workspaceName, Func<ITransaction, IUser> resolveUser, string context, ReadScope expectedScope = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(resolveUser);
            return this.Open(workspaceName, resolveUser, null, context, expectedScope, cancellationToken);
        }

        public ReadContext Open(string workspaceName, LocalReadIdentity identity, string context, ReadScope expectedScope = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(identity);
            return this.Open(workspaceName, transaction => transaction.Instantiate(identity.UserId) as IUser, identity.Population, context, expectedScope, cancellationToken);
        }

        private ReadContext Open(string workspaceName, Func<ITransaction, IUser> resolveUser, Guid? population, string context, ReadScope expectedScope, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var revision = this.configuration.Revision(this.database.MetaPopulation, workspaceName);
            var expected = Copy(expectedScope);
            var read = this.database.OpenRead();
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (population.HasValue && population.Value != read.Population.Incarnation)
                {
                    throw new InvalidOperationException("The Local identity belongs to a retired population. Authenticate and bind the user to the current population before reading.");
                }

                var transaction = read.Transaction;
                var user = resolveUser(transaction);
                read.EnsureValid();
                cancellationToken.ThrowIfCancellationRequested();
                if (user == null || user.Id <= 0 || !ReferenceEquals(user.Strategy.Transaction, transaction)
                    || user.Strategy.IsDeleted || user.Strategy.IsNewInTransaction
                    || !ReferenceEquals(transaction.Instantiate(user.Id), user))
                {
                    throw new InvalidOperationException("The authenticated identity did not resolve to an existing committed user in this read transaction. Authenticate again or correct the host's read-only user resolver.");
                }

                var scope = new ReadScope { p = read.Population.Incarnation.ToString("D"), u = user.Id, w = workspaceName, r = revision, x = context };
                if (expected != null && (expected.p != scope.p || expected.u != scope.u || expected.w != scope.w || expected.r != scope.r || expected.x != scope.x))
                {
                    throw new InvalidOperationException("The expected workspace read scope does not match the authenticated population, user, workspace, representation, or context. Retire the old scope before requesting a new read.");
                }

                return new ReadContext(read, scope, user, cancellationToken);
            }
            catch
            {
                read.Dispose();
                throw;
            }
        }

        internal static ReadScope Copy(ReadScope scope) => scope == null ? null : new ReadScope { p = scope.p, u = scope.u, w = scope.w, r = scope.r, x = scope.x };
    }
}
