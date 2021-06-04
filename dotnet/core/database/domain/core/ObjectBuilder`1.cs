// <copyright file="ObjectBuilder`1.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public abstract class ObjectBuilder<T> : IObjectBuilder where T : Object
    {
        private bool built;
        private Exception exception;

        protected ObjectBuilder(ITransaction transaction) => this.Transaction = transaction;

        ~ObjectBuilder()
        {
            if (this.exception == null && !this.built)
            {
                throw new Exception(this + " was not built.");
            }
        }

        public ITransaction Transaction { get; }

        public void Dispose() => this.Build();

        public override string ToString() => "Builder for " + typeof(T).Name;

        public IObject DefaultBuild() => this.Build();

        public virtual T Build()
        {
            GC.SuppressFinalize(this);

            try
            {
                var instance = this.Transaction.Create<T>();
                this.OnBuild(instance);

                instance.OnBuild(x => x.WithBuilder(this));
                instance.OnPostBuild();

                this.built = true;

                return instance;
            }
            catch (Exception e)
            {
                this.exception = e;
                throw;
            }
        }

        protected abstract void OnBuild(T instance);
    }
}
