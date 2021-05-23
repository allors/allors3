// <copyright file="Time.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Configuration
{
    using System;
    using Domain;

    public class Time : ITime
    {
        public Time() => this.Shift = null;

        public TimeSpan? Shift { get; set; }

        public DateTime Now() => this.Shift.HasValue ? DateTime.UtcNow.Add(this.Shift.Value) : DateTime.UtcNow;
    }
}
