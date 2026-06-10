// <copyright file="Link.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Signals.Default.Core
{
    internal sealed class Link
    {
        internal ReactiveNode Dependency;
        internal ReactiveNode Subscriber;
        internal Link NextDependency;
        internal Link PreviousDependency;
        internal Link NextSubscriber;
        internal Link PreviousSubscriber;
        internal ulong Version;
        internal bool Retained;
    }
}
