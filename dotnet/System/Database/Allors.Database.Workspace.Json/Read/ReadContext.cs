// <copyright file="ReadContext.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System;
    using System.Threading;
    using Allors.Protocol.Json.Api.Read;
    using Security;
    using Services;
    using SqlReadScope = Adapters.Sql.ReadScope;

    /// <summary>
    /// Owns one admitted read attempt. Materialize detached data before Complete, which validates
    /// and closes the attempt. Disposal without completion only abandons it. No retries are made.
    /// </summary>
    public sealed class ReadContext : IDisposable
    {
        private readonly SqlReadScope read;
        private readonly ReadScope scope;
        private readonly IUserService userService;
        private readonly IUser user;
        private readonly IAccessControl accessControl;
        private readonly CancellationToken cancellationToken;
        private bool userChanged;
        private bool disposed;

        internal ReadContext(SqlReadScope read, ReadScope scope, IUser user, CancellationToken cancellationToken)
        {
            this.read = read;
            this.scope = scope;
            this.user = user;
            this.cancellationToken = cancellationToken;
            this.userService = read.Transaction.Services.Get<IUserService>();
            this.userService.User = user;
            this.userService.UserChanged += this.OnUserChanged;
            try
            {
                this.accessControl = read.Transaction.Services.Get<IWorkspaceAclsService>().Create(scope.w)
                    ?? throw new InvalidOperationException("The workspace authorization service returned no access control. Configure a fresh ACL for each protected read.");
                this.EnsureValid();
            }
            catch
            {
                this.userService.UserChanged -= this.OnUserChanged;
                throw;
            }
        }

        public ITransaction Transaction
        {
            get
            {
                this.EnsureValid();
                return this.read.Transaction;
            }
        }

        public ReadScope Scope
        {
            get
            {
                this.EnsureValid();
                return ReadScopeFactory.Copy(this.scope);
            }
        }

        public IAccessControl AccessControl
        {
            get
            {
                this.EnsureValid();
                return this.accessControl;
            }
        }

        public void EnsureValid()
        {
            if (this.disposed)
            {
                throw new InvalidOperationException("This workspace read has ended. Open a new read attempt before accessing data or completing a response.");
            }

            this.cancellationToken.ThrowIfCancellationRequested();
            this.read.EnsureValid();
            this.EnsureIdentity();
        }

        public void Complete()
        {
            try
            {
                this.EnsureValid();
            }
            finally
            {
                this.Dispose();
            }

            this.cancellationToken.ThrowIfCancellationRequested();
            this.EnsureIdentity();
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            try
            {
                this.read.Dispose();
            }
            finally
            {
                this.userService.UserChanged -= this.OnUserChanged;
            }
        }

        private void OnUserChanged(object sender, EventArgs args) => this.userChanged = true;

        private void EnsureIdentity()
        {
            if (this.userChanged || !ReferenceEquals(this.userService.User, this.user))
            {
                throw new InvalidOperationException("The authenticated user changed during the protected read. Discard this attempt and resolve identity in a new read scope.");
            }
        }
    }
}
