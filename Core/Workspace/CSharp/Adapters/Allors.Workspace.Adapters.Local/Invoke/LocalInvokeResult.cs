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
    using IDerivationError = Workspace.IDerivationError;
    using IObject = Workspace.IObject;
    using Method = Workspace.Method;
    using Node = Database.Data.Node;
    using Pull = Database.Data.Pull;

    public class LocalInvokeResult : ICallResult
    {
        private List<LocalStrategy> accessErrorStrategies;
        private List<long> databaseMissingIds;
        private List<LocalStrategy> databaseVersionErrors;
        private IList<IDerivationResult> validations;

        private readonly LocalSession session;

        internal LocalInvokeResult(LocalSession session, LocalWorkspace workspace)
        {
            this.session = session;
            this.Workspace = workspace;
            this.Transaction = this.Workspace.Database.CreateTransaction();

            var sessionContext = this.Transaction.Context();
            var databaseContext = this.Transaction.Database.Context();
            var metaCache = databaseContext.MetaCache;
            var user = sessionContext.User;

            this.AccessControlLists = new WorkspaceAccessControlLists(this.Workspace.Name, user);
            this.AllowedClasses = metaCache.GetWorkspaceClasses(this.Workspace.Name);
            this.MetaPopulation = databaseContext.MetaPopulation;
            this.Derive = () => this.Transaction.Derive(false);

            // TODO: move to separate class
            this.accessErrorStrategies = new List<LocalStrategy>();
            this.databaseMissingIds = new List<long>();
            this.databaseVersionErrors = new List<LocalStrategy>();
        }

        private LocalWorkspace Workspace { get; }

        private ITransaction Transaction { get; }

        private ISet<IClass> AllowedClasses { get; }

        private IAccessControlLists AccessControlLists { get; }

        private MetaPopulation MetaPopulation { get; }

        private Func<IDerivationResult> Derive { get; }

        public string ErrorMessage { get; private set; }

        public IEnumerable<IObject> VersionErrors => this.databaseVersionErrors?.Select(v => v.Object);

        public IEnumerable<IObject> AccessErrors => this.accessErrorStrategies?.Select(v => v.Object);

        public IEnumerable<IObject> MissingErrors => this.session.Get<IObject>(this.databaseMissingIds);

        public IEnumerable<IDerivationError> DerivationErrors => this.validations?.Select(v => (IDerivationError)new LocalDerivationError(this.session, v)).ToArray();

        public bool HasErrors => !string.IsNullOrWhiteSpace(this.ErrorMessage) || this.accessErrorStrategies?.Count > 0 || this.databaseMissingIds?.Count > 0 || this.databaseVersionErrors?.Count > 0 || this.validations?.Count > 0;

        internal void Execute(Method[] methods, InvokeOptions options)
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
                            this.validations.Add(validation);
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
            var obj = this.Transaction.Instantiate(invocation.Object.Id);
            if (obj == null)
            {
                this.databaseMissingIds.Add(invocation.Object.Id);
                return true;
            }

            var localStrategy = (LocalStrategy)invocation.Object.Strategy;

            if (this.AllowedClasses?.Contains(obj.Strategy.Class) != true)
            {
                this.accessErrorStrategies.Add(localStrategy);
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
                this.databaseVersionErrors.Add(localStrategy);
                return true;
            }

            var acl = this.AccessControlLists[obj];
            if (!acl.CanExecute(methodType))
            {
                this.accessErrorStrategies.Add(localStrategy);
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
                this.validations.Add(validation);
                return true;
            }

            return false;
        }
    }
}
