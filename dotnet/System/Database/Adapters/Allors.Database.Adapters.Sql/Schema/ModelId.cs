// <copyright file="ModelId.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql
{
    using System;

    /// <summary>
    /// The id of a class or of a relation type as the adapters write it in a name: its sixteen bytes in network
    /// byte order, in the base32hex alphabet of RFC 4648, section 7, in lower case and without padding. The 128
    /// bits take 26 characters, and two ids that differ give two names that differ, also for a server that
    /// compares names without case.
    /// </summary>
    public static class ModelId
    {
        private const string Alphabet = "0123456789abcdefghijklmnopqrstuv";

        private const int ByteCount = 16;

        // 26 characters of five bits hold the 128 bits: the last one holds the last three, followed by two zero bits.
        private const int CharacterCount = 26;

        public static string Encode(Guid id)
        {
            Span<byte> bytes = stackalloc byte[ByteCount];
            id.TryWriteBytes(bytes, true, out _);

            Span<char> characters = stackalloc char[CharacterCount];
            var buffer = 0;
            var bits = 0;
            var index = 0;
            foreach (var value in bytes)
            {
                buffer = (buffer << 8) | value;
                bits += 8;
                while (bits >= 5)
                {
                    characters[index++] = Alphabet[(buffer >> (bits - 5)) & 31];
                    bits -= 5;
                }

                buffer &= (1 << bits) - 1;
            }

            characters[index] = Alphabet[(buffer << (5 - bits)) & 31];

            return new string(characters);
        }
    }
}
