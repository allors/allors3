// <copyright file="TrackingContext.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Signals.Default.Core
{
    using System.Threading;

    internal static class TrackingContext
    {
        private static readonly AsyncLocal<ComputationNode> ActiveSubscriberStorage = new AsyncLocal<ComputationNode>();
        private static readonly AsyncLocal<EffectScopeNode> ActiveScopeStorage = new AsyncLocal<EffectScopeNode>();

        internal static ComputationNode ActiveSubscriber
        {
            get => ActiveSubscriberStorage.Value;
            set => ActiveSubscriberStorage.Value = value;
        }

        internal static EffectScopeNode ActiveScope
        {
            get => ActiveScopeStorage.Value;
            set => ActiveScopeStorage.Value = value;
        }
    }
}
