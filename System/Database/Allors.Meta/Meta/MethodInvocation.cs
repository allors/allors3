// <copyright file="MethodInvocation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Meta
{
    using System;

    public partial class MethodInvocation
    {
        public MethodInvocation(MethodClass methodClass) => this.MethodClass = methodClass;

        public MethodClass MethodClass { get; }

        //[DebuggerStepThrough]
        public void Execute(Method method)
        {
            if (method.Executed)
            {
                throw new Exception("Method already executed.");
            }

            method.Executed = true;

            foreach (var action in this.MethodClass.Actions)
            {
                // TODO: Add test for deletion
                if (!method.Object.Strategy.IsDeleted && !method.StopPropagation)
                {
                    action(method.Object, method);
                }
            }
        }
    }
}
