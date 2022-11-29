namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using Database.Derivations;
    using Derivations;

    public static class ITransactionExtensions
    {
        public static T Build<T>(this ITransaction @this, Action<T> builder) where T : IObject
        {
            var newObject = @this.Create<T>();

            builder?.Invoke(newObject);

            ((Object)newObject).OnPostBuild();

            return newObject;
        }

        public static T Build<T>(this ITransaction @this, params Action<T>[] builders) where T : IObject
        {
            var newObject = @this.Create<T>();

            if (builders != null)
            {
                foreach (var builder in builders)
                {
                    builder?.Invoke(newObject);
                }
            }

            ((Object)newObject).OnPostBuild();

            return newObject;
        }

        public static T Build<T>(this ITransaction @this, IEnumerable<Action<T>> builders, Action<T> extraBuilder) where T : IObject
        {
            var newObject = @this.Create<T>();

            if (builders != null)
            {
                foreach (var builder in builders)
                {
                    builder?.Invoke(newObject);
                }
            }

            extraBuilder?.Invoke(newObject);

            ((Object)newObject).OnPostBuild();

            return newObject;
        }

        public static T Build<T>(this ITransaction @this, IEnumerable<Action<T>> builders, params Action<T>[] extraBuilders) where T : IObject
        {
            var newObject = @this.Create<T>();

            if (builders != null)
            {
                foreach (var builder in builders)
                {
                    builder?.Invoke(newObject);
                }
            }

            foreach (var extraBuilder in extraBuilders)
            {
                extraBuilder?.Invoke(newObject);
            }

            ((Object)newObject).OnPostBuild();

            return newObject;
        }


        public static IValidation Derive(this ITransaction @this, bool throwExceptionOnError = true, bool continueOnError = false)
        {
            var derivationFactory = @this.Database.Services.Get<IDerivationService>();
            var derivation = derivationFactory.CreateDerivation(@this, continueOnError);
            var validation = derivation.Derive();
            if (throwExceptionOnError && validation.HasErrors)
            {
                throw new DerivationException(validation);
            }

            return validation;
        }

        public static DateTime Now(this ITransaction @this)
        {
            var now = DateTime.UtcNow;

            var time = @this.Database.Services.Get<ITime>();
            var timeShift = time.Shift;
            if (timeShift != null)
            {
                now = now.Add((TimeSpan)timeShift);
            }

            return now;
        }
    }
}
