// <copyright file="EntraEventsValidation.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.Options;

    // Validation runs after every post-configurer, including one registered after the plug-in.
    // Keep the expected event and callback per options instance, so options reloads cannot reuse
    // another instance's callbacks and obsolete options need not stay alive.
    internal sealed class EntraEventsValidation<TOptions> : IValidateOptions<TOptions>
        where TOptions : AuthenticationSchemeOptions
    {
        private readonly string scheme;
        private readonly string callbackName;
        private readonly Func<TOptions, Delegate> callback;
        private readonly MethodInfo dispatch;
        private readonly ConditionalWeakTable<TOptions, Snapshot> snapshots = new();

        internal EntraEventsValidation(string scheme, string callbackName, Func<TOptions, Delegate> callback, MethodInfo dispatch)
        {
            this.scheme = scheme;
            this.callbackName = callbackName;
            this.callback = callback;
            this.dispatch = dispatch.GetBaseDefinition();
        }

        internal void Capture(TOptions options) => this.snapshots.Add(options, new Snapshot(options.Events, this.callback(options)));

        public ValidateOptionsResult Validate(string name, TOptions options)
        {
            if (!string.Equals(name, this.scheme, StringComparison.Ordinal))
            {
                return ValidateOptionsResult.Skip;
            }

            if (options.EventsType != null)
            {
                return ValidateOptionsResult.Fail(
                    $"The Entra scheme '{name}' takes its events from EventsType ({options.EventsType.Name}), so the plug-in's callbacks would not run. " +
                    $"Set callbacks on {typeof(TOptions).Name}.Events through Configure instead of setting EventsType.");
            }

            if (!this.snapshots.TryGetValue(options, out var snapshot) ||
                !ReferenceEquals(options.Events, snapshot.Events) || this.callback(options) != snapshot.Callback)
            {
                return ValidateOptionsResult.Fail(
                    $"The Entra scheme '{name}' has replaced Events or Events.{this.callbackName} after the plug-in installed its callback. " +
                    $"Set application callbacks through Configure<{typeof(TOptions).Name}> instead of replacing them in PostConfigure, so the plug-in can wrap them.");
            }

            // A virtual override can ignore the On... delegate even when it is still installed.
            // Walk the declared methods so an inherited override hidden by a new method is caught
            // too, while a harmless new method that does not override dispatch remains allowed.
            for (var type = options.Events.GetType(); type != this.dispatch.DeclaringType; type = type.BaseType)
            {
                foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                {
                    if (method.GetBaseDefinition() == this.dispatch)
                    {
                        return ValidateOptionsResult.Fail(
                            $"The Entra scheme '{name}' overrides Events.{this.dispatch.Name}, so the plug-in cannot ensure its callback runs. " +
                            $"Configure Events.{this.callbackName} instead of overriding {this.dispatch.Name}.");
                    }
                }
            }

            return ValidateOptionsResult.Success;
        }

        private sealed record Snapshot(object Events, Delegate Callback);
    }
}
