// <copyright file="MetaFingerprintTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors
{
    using System.Linq;
    using System.Text.RegularExpressions;
    using Xunit;

    /// <summary>
    /// The fingerprint of a meta population is the same on every side that computes it from
    /// the same tags, whatever their order, and differs for any other set of tags.
    /// </summary>
    public class MetaFingerprintTests
    {
        [Fact]
        public void IsSixteenLowercaseHexCharacters()
        {
            var fingerprint = MetaFingerprint.Compute(new[] { "a", "b" });

            Assert.Matches(new Regex("^[0-9a-f]{16}$"), fingerprint);
            Assert.Matches(new Regex("^[0-9a-f]{16}$"), MetaFingerprint.Compute(Enumerable.Empty<string>()));
        }

        [Fact]
        public void IsTheSameForTheSameTagsInAnyOrder()
        {
            var tags = new[] { "h6OmCbWhOECwdDoByBy9og", "0gIHQyvgrUWbIrgzHcdaPw", "1", "7" };

            var fingerprint = MetaFingerprint.Compute(tags);

            Assert.Equal(fingerprint, MetaFingerprint.Compute(tags.Reverse()));
            Assert.Equal(fingerprint, MetaFingerprint.Compute(tags.OrderBy(v => v)));
            Assert.Equal(fingerprint, MetaFingerprint.Compute(tags.ToList()));
        }

        [Fact]
        public void DiffersWhenATagIsAddedRemovedOrChanged()
        {
            var tags = new[] { "a", "b", "c" };
            var fingerprint = MetaFingerprint.Compute(tags);

            Assert.NotEqual(fingerprint, MetaFingerprint.Compute(new[] { "a", "b" }));
            Assert.NotEqual(fingerprint, MetaFingerprint.Compute(new[] { "a", "b", "c", "d" }));
            Assert.NotEqual(fingerprint, MetaFingerprint.Compute(new[] { "a", "b", "x" }));
            Assert.NotEqual(fingerprint, MetaFingerprint.Compute(new[] { "ab", "c" }));
        }

        [Fact]
        public void IsAKnownValueSoThatEveryImplementationAgrees()
        {
            // FNV-1a, 64 bit, over the UTF-8 bytes of the sorted tags, each followed by a line
            // feed; the TypeScript connection computes the same.
            Assert.Equal("cbf29ce484222325", MetaFingerprint.Compute(Enumerable.Empty<string>()));
            Assert.Equal("089bdc07b544e7b2", MetaFingerprint.Compute(new[] { "a" }));
            Assert.Equal("78ed6781f136a14e", MetaFingerprint.Compute(new[] { "b", "a" }));
        }
    }
}
