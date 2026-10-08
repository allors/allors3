// <copyright file="RecordChangedEventArgs.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Connection
{
    using System;

    /// <summary>
    /// A record the connection held has been replaced by a newer one.
    /// </summary>
    public sealed class RecordChangedEventArgs : EventArgs
    {
        public RecordChangedEventArgs(IRecord record) => this.Record = record;

        public long Id => this.Record.Id;

        /// <summary>
        /// The record as the connection holds it now.
        /// </summary>
        public IRecord Record { get; }
    }
}
