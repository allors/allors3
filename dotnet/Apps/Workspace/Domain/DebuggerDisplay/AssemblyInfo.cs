// <copyright file="AssemblyInfo.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Diagnostics;
using Allors.Workspace.Domain;
using static DebuggerDisplayConstants;

// General
[assembly: DebuggerDisplay("[Key={Key}, Value={Value}]", Target = typeof(KeyValuePair<,>))]

// Allors
[assembly: DebuggerDisplay(name, Target = typeof(Organisation))]
[assembly: DebuggerDisplay("{FirstName}" + id, Target = typeof(Person))]
