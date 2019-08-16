// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DerivationLogFormatter.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Allors.Domain.Logging
{
    public static class DerivationLogFormatter
    {
        public const int TabWith = 20;

        public static string FormatStartedGeneration(int generation)
        {
            return $"{"Generation",-TabWith} #{generation}";
        }

        public static string FormatStartedPreparation(int preparationRun)
        {
            return $"{"Preparation",-TabWith} #{preparationRun}";
        }

        public static string FormatPreDeriving(Object derivable)
        {
            return $"{"PreDerive",-TabWith} " + FormatDerivable(derivable);
        }

        public static string FormatPreDerived(Object derivable)
        {
            return $"{"PreDerive�",-TabWith} " + FormatDerivable(derivable);
        }

        public static string FormatAddedDerivable(Object derivable)
        {
            return $"{"Add",-TabWith} " + FormatDerivable(derivable);
        }

        public static string FormatAddedDependency(Object dependent, Object dependee)
        {
            return $"{"Dependency",-TabWith} " + FormatDerivable(dependent) + " <- " + FormatDerivable(dependee);
        }

        public static string FormatAddedError(IDerivationError derivationError)
        {
            return $"Error {derivationError}";
        }

        public static string FormatCycle(Object root, Object derivable)
        {
            return $"Cycle root: {root}, object: {derivable}";
        }

        public static string FormatCycleDetected(Object dependent, Object dependee)
        {
            return $"Cycle detected between dependent: {FormatDerivable(dependent)}, dependee: {FormatDerivable(dependee)}";
        }

        public static string FormatDeriving(Object derivable)
        {
            return $"{"Derive",-TabWith} " + FormatDerivable(derivable);
        }

        public static string FormatDerived(Object derivable)
        {
            return $"{"Derive�",-TabWith} " + FormatDerivable(derivable);
        }

        public static string FormatPostDeriving(Object derivable)
        {
            return $"{"PostDerive",-TabWith} " + FormatDerivable(derivable);
        }

        public static string FormatPostDerived(Object derivable)
        {
            return $"{"PostDerive�",-TabWith} " + FormatDerivable(derivable);
        }

        private static object FormatDerivable(Object derivable)
        {
            var info = $"{derivable.Strategy.Class.Name}: {derivable}";
            return $"[{info,80}]";
        }
    }
}