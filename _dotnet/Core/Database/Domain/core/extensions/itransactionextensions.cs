namespace Allors.Database.Domain
{
    using System;
    using Database.Derivations;
    using Derivations;

    public static class ITransactionExtensions
    {
        public static IValidation Derive(this ITransaction transaction, bool throwExceptionOnError = true, bool continueOnError = false)
        {
            var derivationFactory = transaction.Database.Services.Get<IDerivationService>();
            var derivation = derivationFactory.CreateDerivation(transaction, continueOnError);
            var validation = derivation.Derive();
            if (throwExceptionOnError && validation.HasErrors)
            {
                throw new DerivationException(validation);
            }

            return validation;
        }

        public static DateTime Now(this ITransaction transaction)
        {
            var now = DateTime.UtcNow;

            var time = transaction.Database.Services.Get<ITime>();
            var timeShift = time.Shift;
            if (timeShift != null)
            {
                now = now.Add((TimeSpan)timeShift);
            }

            return now;
        }
    }
}
