// <copyright file="ExtentEnumerator.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql
{
    using System.Collections;
    using System.Collections.Generic;

    internal class ExtentEnumerator : IEnumerator
    {
        private readonly IEnumerator enumerator;

        private readonly Transaction transaction;

        internal ExtentEnumerator(Transaction transaction, IEnumerable<Reference> references)
        {
            this.transaction = transaction;
            this.enumerator = references.GetEnumerator();
        }

        public object Current
        {
            get
            {
                this.transaction.EnsureReadable();
                var reference = (Reference)this.enumerator.Current;
                if (reference != null)
                {
                    return reference.Strategy.GetObject();
                }

                return null;
            }
        }

        public bool MoveNext()
        {
            this.transaction.EnsureReadable();
            return this.enumerator.MoveNext();
        }

        public void Reset()
        {
            this.transaction.EnsureReadable();
            this.enumerator.Reset();
        }
    }
}
