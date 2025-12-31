// <copyright file="ExtentOperation.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    internal sealed class ExtentOperation : Extent
    {
        private readonly Extent firstOperand;
        private readonly ExtentOperationType operationType;
        private readonly Extent secondOperand;

        public ExtentOperation(Transaction transaction, Extent firstOperand, Extent secondOperand, ExtentOperationType operationType)
            : base(transaction)
        {
            if (!firstOperand.ObjectType.Equals(secondOperand.ObjectType))
            {
                throw new ArgumentException("Both extents in a Union, Intersect or Except must be from the same type");
            }

            this.operationType = operationType;

            this.firstOperand = firstOperand;
            this.secondOperand = secondOperand;

            firstOperand.Parent = this;
            secondOperand.Parent = this;
        }

        public override ICompositePredicate Filter => null;

        public override IComposite ObjectType => this.firstOperand.ObjectType;

        protected override void Evaluate()
        {
            if (this.Strategies == null)
            {
                var firstOperandStrategies = this.firstOperand.GetEvaluatedStrategies();
                var secondOperandStrategies = this.secondOperand.GetEvaluatedStrategies();

                // Use HashSet for O(1) lookups instead of O(n) List.Contains
                switch (this.operationType)
                {
                    case ExtentOperationType.Union:
                        var unionSet = new HashSet<Strategy>(firstOperandStrategies);
                        unionSet.UnionWith(secondOperandStrategies);
                        this.Strategies = unionSet.ToList();
                        break;

                    case ExtentOperationType.Intersect:
                        var firstSet = new HashSet<Strategy>(firstOperandStrategies);
                        this.Strategies = secondOperandStrategies
                            .Where(s => firstSet.Contains(s))
                            .ToList();
                        break;

                    case ExtentOperationType.Except:
                        var exceptSet = new HashSet<Strategy>(secondOperandStrategies);
                        this.Strategies = firstOperandStrategies
                            .Where(s => !exceptSet.Contains(s))
                            .ToList();
                        break;

                    default:
                        throw new Exception("Unknown extent operation");
                }

                if (this.Sorter != null)
                {
                    this.Strategies.Sort(this.Sorter);
                }
            }
        }
    }
}
