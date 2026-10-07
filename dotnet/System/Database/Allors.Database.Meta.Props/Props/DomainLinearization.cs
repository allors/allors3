// <copyright file="DomainLinearization.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Meta
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The order of the domains, for the hooks of domains that do not extend each other: a domain
    /// before the domains it extends; branches in the id order of their top domain, each branch kept
    /// whole. It is the C3 linearization over the direct superdomains sorted by id, after dropping a
    /// direct superdomain that another direct superdomain already extends. Names and the declared
    /// order of the superdomains play no part, so the order survives a rename and a reordering of
    /// [Extends], and a domain is ordered the same in every population that holds it. The ids compare
    /// as <see cref="Guid.CompareTo(Guid)"/> does: as written, character by character, ignoring case,
    /// which DomainOrderTests pins.
    /// </summary>
    public static class DomainLinearization
    {
        /// <summary>
        /// Returns the domain followed by its superdomains, nearest first.
        /// Throws when the domains have a cycle, or when two superdomains order their superdomains in opposite ways.
        /// </summary>
        public static IDomain[] Linearize(IDomain domain) => Linearize(domain, new List<IDomain>(), new Dictionary<IDomain, IDomain[]>());

        /// <summary>
        /// Returns every domain of a population in order, most derived first: the linearization of the
        /// one domain that no other domain extends. Throws when there are several such domains.
        /// </summary>
        public static IDomain[] Sort(IEnumerable<IDomain> domains)
        {
            var all = domains.ToArray();
            var linearizationByDomain = new Dictionary<IDomain, IDomain[]>();

            foreach (var domain in all)
            {
                Linearize(domain, new List<IDomain>(), linearizationByDomain);
            }

            var extended = new HashSet<IDomain>(all.SelectMany(v => v.DirectSuperdomains));
            var leaves = all.Where(v => !extended.Contains(v)).ToArray();

            if (leaves.Length > 1)
            {
                throw new Exception(
                    $"A population has one domain that no other domain extends, but here no domain extends {Names(leaves)}. " +
                    "Let one of them extend the other, or add a domain that extends both.");
            }

            return leaves.Length == 0 ? Array.Empty<IDomain>() : linearizationByDomain[leaves[0]];
        }

        private static IDomain[] Linearize(IDomain domain, List<IDomain> path, Dictionary<IDomain, IDomain[]> linearizationByDomain)
        {
            if (linearizationByDomain.TryGetValue(domain, out var linearization))
            {
                return linearization;
            }

            var index = path.IndexOf(domain);
            if (index >= 0)
            {
                var cycle = path.Skip(index).Append(domain).ToArray();
                var steps = cycle.Take(cycle.Length - 1).Select((v, i) => $"{v.Name} extends {cycle[i + 1].Name}");
                throw new Exception($"The domains have a cycle: {Join(steps)}. Remove one of these [Extends].");
            }

            path.Add(domain);
            var directSuperdomains = domain.DirectSuperdomains.Distinct().ToArray();
            var linearizationBySuperdomain = directSuperdomains.ToDictionary(v => v, v => Linearize(v, path, linearizationByDomain));
            path.RemoveAt(path.Count - 1);

            // A direct superdomain that another direct superdomain already extends adds nothing.
            var superdomains = directSuperdomains
                .Where(v => !directSuperdomains.Any(w => !w.Equals(v) && linearizationBySuperdomain[w].Skip(1).Contains(v)))
                .OrderBy(v => v.Id)
                .ToArray();

            var sequences = superdomains
                .Select(v => (Owner: v, Remaining: linearizationBySuperdomain[v].ToList()))
                .Append((Owner: domain, Remaining: superdomains.ToList()))
                .ToList();

            linearization = new[] { domain }.Concat(Merge(domain, sequences)).ToArray();
            linearizationByDomain[domain] = linearization;
            return linearization;
        }

        // The C3 merge: take the first head that is in no tail, until nothing remains.
        private static List<IDomain> Merge(IDomain domain, List<(IDomain Owner, List<IDomain> Remaining)> sequences)
        {
            var merged = new List<IDomain>();

            while (true)
            {
                sequences.RemoveAll(v => v.Remaining.Count == 0);
                if (sequences.Count == 0)
                {
                    return merged;
                }

                var head = sequences
                    .Select(v => v.Remaining[0])
                    .FirstOrDefault(candidate => !sequences.Any(v => v.Remaining.Skip(1).Contains(candidate)));

                if (head == null)
                {
                    throw new Exception(
                        $"{domain.Name} cannot order its superdomains: {Join(Contradiction(sequences))}. " +
                        "Change the structure of the domains so that these orders agree, for example by letting one of them extend the other.");
                }

                merged.Add(head);
                foreach (var (_, remaining) in sequences)
                {
                    if (remaining[0].Equals(head))
                    {
                        remaining.RemoveAt(0);
                    }
                }
            }
        }

        // Follows the blocked heads around until one repeats: each step names the superdomain whose order blocks the previous head.
        private static IEnumerable<string> Contradiction(List<(IDomain Owner, List<IDomain> Remaining)> sequences)
        {
            var seen = new List<IDomain>();
            var head = sequences[0].Remaining[0];

            while (!seen.Contains(head))
            {
                seen.Add(head);
                var blocking = sequences.First(v => v.Remaining.Skip(1).Contains(head));
                yield return $"{blocking.Owner.Name} orders {blocking.Remaining[0].Name} before {head.Name}";
                head = blocking.Remaining[0];
            }
        }

        private static string Names(IEnumerable<IDomain> domains) => Join(domains.Select(v => v.Name));

        private static string Join(IEnumerable<string> parts)
        {
            var array = parts.ToArray();
            return array.Length <= 1 ? string.Concat(array) : string.Join(", ", array.Take(array.Length - 1)) + " and " + array[array.Length - 1];
        }
    }
}
