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
    using Database.Data;
    using Database.Derivations;
    using Database.Domain;
    using Database.Meta;
    using Database.Security;
    using Protocol.Direct;
    using IClass = Database.Meta.IClass;
    using Method = Workspace.Method;
    using Node = Database.Data.Node;
    using Pull = Database.Data.Pull;

    public class LocalInvokeResult
    {
        internal LocalInvokeResult(LocalWorkspace workspace)
        {
            this.Workspace = workspace;
            this.Transaction = this.Workspace.Database.CreateTransaction();

            var sessionContext = this.Transaction.Context();
            var databaseContext = this.Transaction.Database.Context();
            var metaCache = databaseContext.MetaCache;
            var user = sessionContext.User;

            this.AccessControlLists = new WorkspaceAccessControlLists(this.Workspace.Name, user);
            this.AllowedClasses = metaCache.GetWorkspaceClasses(this.Workspace.Name);
            this.M = databaseContext.M;
            this.MetaPopulation = databaseContext.MetaPopulation;
            this.Derive = () => this.Transaction.Derive(false);

            // TODO: move to separate class
            this.AccessErrorStrategies = new List<LocalStrategy>();
            this.MissingIds = new List<long>();
            this.VersionErrors = new Dictionary<LocalStrategy, IObject>();
        }

        public LocalWorkspace Workspace { get; }

        public ITransaction Transaction { get; }

        public ISet<IClass> AllowedClasses { get; }

        internal IAccessControlLists AccessControlLists { get; }

        public M M { get; set; }

        public MetaPopulation MetaPopulation { get; set; }

        public Func<IDerivationResult> Derive { get; }

        public List<LocalStrategy> AccessErrorStrategies { get; }

        public List<long> MissingIds { get; }

        public Dictionary<LocalStrategy, IObject> VersionErrors { get; }

        public IList<IDerivationResult> Validations { get; set; }

        public string ErrorMessage { get; private set; }

        public bool HasErrors => !string.IsNullOrWhiteSpace(this.ErrorMessage) || this.AccessErrorStrategies?.Count > 0 || this.MissingIds?.Count > 0 || this.VersionErrors?.Count > 0 || this.Validations?.Count > 0;

        internal void Execute(Method[] methods, CallOptions options)
        {
            var isolated = options?.Isolated ?? false;
            var continueOnError = options?.ContinueOnError ?? false;

            if (isolated)
            {
                foreach (var method in methods)
                {
                    var error = this.Invoke(method);
                    if (!error)
                    {
                        var validation = this.Derive();
                        if (validation.HasErrors)
                        {
                            error = true;
                            this.Validations.Add(validation);
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
                    error = this.Invoke(method);
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

        private bool Invoke(Method invocation)
        {
            var obj = this.Transaction.Instantiate(invocation.Object.Identity);
            if (obj == null)
            {
                this.MissingIds.Add(invocation.Object.Identity);
                return true;
            }

            var localStrategy = (LocalStrategy)invocation.Object.Strategy;

            if (this.AllowedClasses?.Contains(obj.Strategy.Class) != true)
            {
                this.AccessErrorStrategies.Add(localStrategy);
                return true;
            }

            var composite = (IComposite)obj.Strategy.Class;

            // TODO: Cache and filter for workspace
            var methodTypes = composite.MethodTypes.Where(v => v.WorkspaceNames.Length > 0);
            var methodType = methodTypes.FirstOrDefault(x => x.Id.Equals(invocation.MethodType.Id));

            if (methodType == null)
            {
                throw new Exception("Method " + invocation.MethodType + " not found.");
            }

            if (!localStrategy.DatabaseVersion.Equals(obj.Strategy.ObjectVersion))
            {
                this.VersionErrors.Add(localStrategy, obj);
                return true;
            }

            var acl = this.AccessControlLists[obj];
            if (!acl.CanExecute(methodType))
            {
                this.AccessErrorStrategies.Add(localStrategy);
                return true;
            }

            var method = obj.GetType().GetMethod(methodType.Name, new Type[] { });

            try
            {
                method.Invoke(obj, null);
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

            var validation = this.Derive();
            if (validation.HasErrors)
            {
                this.Validations.Add(validation);
                return true;
            }

            return false;
        }
    }
}
