namespace Allors.Collections
{
    using System;
    using System.Collections.Generic;

    public class EmptyEnumerator<T> : IEnumerator<T>
    {
        public static readonly EmptyEnumerator<T> Instance = new EmptyEnumerator<T>();

        public bool MoveNext() => false;

        public void Reset() { }

        T IEnumerator<T>.Current => throw new NotSupportedException("EmptyEnumerator has no elements.");
        public object Current => throw new NotSupportedException("EmptyEnumerator has no elements.");

        public void Dispose() { }
    }
}
