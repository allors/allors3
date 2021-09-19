// <copyright file="ObjectsBase.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Linq;
    using Meta;

    public abstract partial class ObjectsBase<T> : IObjects where T : IObject
    {
        protected ObjectsBase(ITransaction transaction)
        {
            this.Transaction = transaction;
            this.M = this.Transaction.Database.Services.Get<MetaPopulation>();
        }

        public MetaPopulation M { get; }

        public abstract Composite ObjectType { get; }

        public ITransaction Transaction { get; private set; }

        public Extent<T> Extent() => this.Transaction.Extent<T>();

        public T FindBy(IRoleType roleType, object parameter)
        {
            if (parameter == null)
            {
                return default;
            }

            var extent = this.Transaction.Extent(this.ObjectType);
            extent.Filter.AddEquals(roleType, parameter);
            return (T)extent.FirstOrDefault();
        }

        protected virtual void CorePrepare(Setup setup) => setup.Add(this);

        protected virtual void CoreSetup(Setup setup)
        {
        }

        protected virtual void CorePrepare(Security security) => security.Add(this);

        protected virtual void CoreSecure(Security config)
        {
        }
    }
}
