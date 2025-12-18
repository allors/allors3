// <copyright file="TransactionChanges.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory
{
    using System.Collections.Generic;

    /// <summary>
    /// Collected changes from a transaction for atomic commit.
    /// </summary>
    internal sealed class TransactionChanges
    {
        internal TransactionChanges()
        {
            this.NewObjects = new List<CommittedObject>();
            this.ModifiedObjects = new List<CommittedObject>();
            this.DeletedObjectIds = new HashSet<long>();
        }

        internal List<CommittedObject> NewObjects { get; }

        internal List<CommittedObject> ModifiedObjects { get; }

        internal HashSet<long> DeletedObjectIds { get; }
    }
}
