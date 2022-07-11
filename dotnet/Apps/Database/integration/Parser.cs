// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Parser.cs" company="Allors bvba">
//   Copyright 2002-2012 Allors bvba.
// 
// Dual Licensed under
//   a) the General Public Licence v3 (GPL)
//   b) the Allors License
// 
// The GPL License is included in the file gpl.txt.
// The Allors License is an addendum to your contract.
// 
// Allors Applications is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// For more information visit http://www.allors.com/legal
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Linq.Expressions;

namespace Allors.Integration
{
    using Microsoft.Extensions.Logging;

    public static class Parser
    {
        public static Func<Expression<Func<T, string>>, int?> ToInt<T>(this T @object, ILogger logger)
        {
            return new ToIntFunc<T>(@object, logger).Call;
        }

        public static Func<Expression<Func<T, string>>, decimal?> ToDecimal<T>(this T @object, ILogger logger)
        {
            return new ToDecimalFunc<T>(@object, logger).Call;
        }

        private class ToIntFunc<T>
        {
            public T Object { get; }

            public ILogger Logger { get; }

            public ToIntFunc(T @object, ILogger logger)
            {
                this.Object = @object;
                this.Logger = logger;
            }

            public int? Call(Expression<Func<T, string>> lambda)
            {
                var func = lambda.Compile();
                var value = func(this.Object);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (int.TryParse(value, out var result))
                    {
                        return result;
                    }

                    this.Logger.LogError($"Could not parse value: {value} for {lambda} on {this.Object}");
                    throw new Exception(typeof(T).Name);
                }

                return null;
            }
        }

        private class ToDecimalFunc<T>
        {
            public T Object { get; }

            public ILogger Logger { get; }

            public ToDecimalFunc(T @object, ILogger logger)
            {
                this.Object = @object;
                this.Logger = logger;
            }

            public decimal? Call(Expression<Func<T, string>> lambda)
            {
                var func = lambda.Compile();
                var value = func(this.Object);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (decimal.TryParse(value, out var result))
                    {
                        return result;
                    }

                    this.Logger.LogError($"Could not parse value: {value} for {lambda} on {this.Object}");
                    throw new Exception(typeof(T).Name);
                }

                return null;
            }
        }

    }
}
