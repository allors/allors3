// <copyright file="AssemblyInfo.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Diagnostics;
using Allors.Database.Domain;
using static DebuggerDisplayConstants;

// General
[assembly: DebuggerDisplay("[Key={Key}, Value={Value}]", Target = typeof(KeyValuePair<,>))]

// Allors
[assembly: DebuggerDisplay("{DebuggerDisplay}" + id, Target = typeof(AccessControl))]
[assembly: DebuggerDisplay("{DebuggerDisplay}" + id, Target = typeof(SecurityToken))]
