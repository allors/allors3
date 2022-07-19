// <copyright file="AssemblyInfo.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Diagnostics;
using Allors.Workspace.Domain;
using static DebuggerDisplayConstants;

// General
[assembly: DebuggerDisplay("[Key={Key}, Value={Value}]", Target = typeof(KeyValuePair<,>))]

// Allors
[assembly: DebuggerDisplay("{Name}" + id + session, Target = typeof(Organisation))]
[assembly: DebuggerDisplay("{FirstName}" + id + session, Target = typeof(Person))]

[assembly: DebuggerDisplay("{Name}" + id + session, Target = typeof(C1))]
[assembly: DebuggerDisplay("{Name}" + id + session, Target = typeof(C2))]
