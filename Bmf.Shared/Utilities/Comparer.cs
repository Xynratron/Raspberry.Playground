using System;
using System.Collections.Generic;

namespace Bmf.Shared.Utilities.Linq
{
    public class Comparer<T> : IComparer<T>
    {
        private readonly Func<T, T, int> _comparer;

        public Comparer(Func<T, T, int> comparer)
        {
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));
            _comparer = comparer;
        }

        public int Compare(T x, T y)
        {
            return _comparer(x, y);
        }
    }
}