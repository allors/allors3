// <copyright file="LocalPullResult.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database;
    using Database.Derivations;
    using Database.Domain;
    using Database.Meta;
    using Database.Security;
    using Method = Allors.Workspace.Method;

    public class Invoke : Result
    {
        internal Invoke(Session session, Workspace workspace) : base(session)
        {
            this.Workspace = workspace;
            this.Transaction = this.Workspace.DatabaseConnection.Database.CreateTransaction();

            var sessionContext = this.Transaction.Context();
            var databaseContext = this.Transaction.Database.Context();
            var metaCache = databaseContext.MetaCache;
            var user = sessionContext.User;

            this.AccessControlLists = new WorkspaceAccessControlLists(this.Workspace.DatabaseConnection.Configuration.Name, user);
            this.AllowedClasses = metaCache.GetWorkspaceClasses(this.Workspace.DatabaseConnection.Configuration.Name);
            this.Derive = () => this.Transaction.Derive(false);
        }

        private Workspace Workspace { get; }

        private ITransaction Transaction { get; }

        private ISet<IClass> AllowedClasses { get; }

        private IAccessControlLists AccessControlLists { get; }

        private Func<IDerivationResult> Derive { get; }

        internal void Execute(Method[] methods, InvokeOptions options)
        {
            var isolated = options?.Isolated ?? false;
            var continueOnError = options?.ContinueOnError ?? false;

            if (isolated)
            {
                foreach (var method in methods)
                {
                    var error = this.Execute(method);
                    if (!error)
                    {
                        var derivationResult = this.Derive();
                        if (derivationResult.HasErrors)
                        {
                            error = true;
                            this.AddDerivationErrors(derivationResult.Errors);
                        }
                    }

                    if (error)
                    {
                        this.Transaction.Rollback();
                        if (!continueOnError)
                        {
                            break;
                        }
                    }
                    else
                    {
                        this.Transaction.Commit();
                    }
                }
            }
            else
            {
                var error = false;
                foreach (var method in methods)
                {
                    error = this.Execute(method);
                    if (error)
                    {
                        break;
                    }
                }

                if (error)
                {
                    this.Transaction.Rollback();
                }
                else
                {
                    this.Transaction.Commit();
                }
            }

            if (!this.HasErrors)
            {
                this.Transaction.Commit();
            }
        }

        private bool Execute(Method invocation)
        {
            var obj = this.Transaction.Instantiate(invocation.Object.Id);
            if (obj == null)
            {
                this.AddMissingId(invocation.Object.Id);
                return true;
            }

            var localStrategy = (Strategy)invocation.Object.Strategy;

            if (this.AllowedClasses?.Contains(obj.Strategy.Class) != true)
            {
                this.AddAccessError(localStrategy);
                return true;
            }

            var composite = (IComposite)obj.Strategy.Class;

            // TODO: Cache and filter for workspace
            var methodTypes = composite.MethodTypes.Where(v => v.WorkspaceNames.Length > 0);
            var methodType = methodTypes.FirstOrDefault(x => x.Tag.Equals(invocation.MethodType.Tag));

            if (methodType == null)
            {
                throw new Exception("Method " + invocation.MethodType + " not found.");
            }

            if (!localStrategy.DatabaseOriginState.Version.Equals(obj.Strategy.ObjectVersion))
            {
                this.AddVersionError(localStrategy.Id);
                return true;
            }

            var acl = this.AccessControlLists[obj];
            if (!acl.CanExecute(methodType))
            {
                this.AddAccessError(localStrategy);
                return true;
            }

            var method = obj.GetType().GetMethod(methodType.Name, new Type[] { });

            try
            {
                _ = method.Invoke(obj, null);
            }
            catch (Exception e)
            {
                var innerException = e;
                while (innerException.InnerException != null)
                {
                    innerException = innerException.InnerException;
                }

                this.ErrorMessage = innerException.Message;
                return true;
            }

            var derivationResult = this.Derive();
            if (!derivationResult.HasErrors)
            {
                return false;
            }

            this.AddDerivationErrors(derivationResult.Errors);
            return true;
        }
    }
}
