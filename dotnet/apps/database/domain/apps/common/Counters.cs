// <copyright file="Counters.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class Counters
    {
        public static int NextElfProefValue(ITransaction transaction, Guid counterId)
        {
            int NextElfProefValue(Counter counter)
            {
                counter.Value += 1;

                while (!IsValidElfProefNumber(counter.Value))
                {
                    counter.Value += 1;
                }

                return counter.Value;
            }

            if (transaction.Database.IsShared)
            {
                using (var outOfBandTransaction = transaction.Database.CreateTransaction())
                {
                    var outOfBandCounter = new Counters(outOfBandTransaction).Cache[counterId];
                    if (outOfBandCounter != null)
                    {
                        var value = NextElfProefValue(outOfBandCounter);
                        outOfBandTransaction.Commit();
                        return value;
                    }
                }
            }

            var transactionCounter = new Counters(transaction).Cache[counterId];
            return NextElfProefValue(transactionCounter);
        }

        public static bool IsValidElfProefNumber(int number)
        {
            var numberString = number.ToString();
            var length = numberString.Length;

            // ... the number must be validatable to the so-called 11-proof ...
            long total = 0;
            for (var i = 0; i <= numberString.Length - 1; i++)
            {
                var nummertje = Convert.ToInt32(numberString[i].ToString());
                total += nummertje * length;
                length--;
            }

            // ... not result in a 0 when dividing by 11 ...
            if (total == 0)
                return false;

            // ... and not have a modulo when dividing by 11.
            return total % 11 == 0;
        }
    }
}
