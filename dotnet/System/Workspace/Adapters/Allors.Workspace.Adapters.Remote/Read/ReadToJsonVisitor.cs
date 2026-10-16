// <copyright file="ReadToJsonVisitor.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Collections;
    using System.Linq;
    using Allors.Protocol.Json;
    using Data;
    using Protocol.Json;

    internal sealed class ReadToJsonVisitor : ToJsonVisitor
    {
        private readonly IUnitConvert units;
        private readonly bool committedArguments;
        internal ReadToJsonVisitor(IUnitConvert units, bool committedArguments = false) : base(units) { this.units = units; this.committedArguments = committedArguments; }

        public override void VisitPull(Pull visited)
        {
            if (visited.ObjectId <= 0 || visited.Object?.Id <= 0)
            { throw new ArgumentException("Scoped Pull requires positive committed object IDs. Push new objects before reading them."); }
            base.VisitPull(visited);
            this.Pull.a = visited.Arguments?.ToDictionary(v => v.Key, v => this.Argument(v.Value));
            if (visited.Results != null && visited.Results.Length == 0)
            { this.Pull.r = Array.Empty<Allors.Protocol.Json.Data.Result>(); }
        }

        public override void VisitInstanceOf(Instanceof visited)
        {
            base.VisitInstanceOf(visited);
            this.CurrentPredicate.p = visited.Parameter;
        }

        public override void VisitExists(Exists visited)
        {
            if (visited.Parameter != null)
            { throw new NotSupportedException("Scoped reads do not admit parameter-form Exists. Supply its property directly."); }
            base.VisitExists(visited);
        }

        private object Argument(object value)
        {
            if (value is long id) { return id; }
            if (value is IObject obj)
            {
                if (this.committedArguments && obj.Id <= 0) throw new ArgumentException("PushRead query arguments must reference committed objects. Read a newly saved object after its ID is assigned.");
                return obj.Id;
            }
            if (value is Meta.IObjectType type) { return type.Tag; }
            if (value is IEnumerable values && !(value is string) && !(value is byte[]))
            { return values.Cast<object>().Select(this.Argument).ToArray(); }
            return this.units.ToJson(value);
        }
    }
}
