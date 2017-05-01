using System;
using System.Collections.Generic;
using System.Linq;
using Bmf.Shared.Annotations;

namespace Bmf.Shared
{
    /// <summary>
    /// Extensions for Exception checks in functions. e.g. check if parameter is null
    /// </summary>
    public static class ExceptionHelper
    {
        public static T ThrowIfArgumentIsNull<T>(this T self)
        {
            if (self == null)
                throw new ArgumentNullException();
            return self;
        }

        public static T ThrowIfArgumentIsNull<T>(this T self, string argumentName)
        {
            if (self == null)
                throw new ArgumentNullException(argumentName);
            return self;
        }

        public static string ThrowIfArgumentIsNullOrEmpty(this string self)
        {
            if (self == null)
                throw new ArgumentNullException();
            if (string.IsNullOrEmpty(self))
                throw new ArgumentException();
            return self;
        }

        public static string ThrowIfArgumentIsNullOrEmpty(this string self, string argumentName)
        {
            if (self == null)
                throw new ArgumentNullException(argumentName);
            if (string.IsNullOrEmpty(self))
                throw new ArgumentException(argumentName);
            return self;
        }

        public static string ThrowIfArgumentIsNullOrWhitespace(this string self, string argumentName)
        {
            if (self == null)
                throw new ArgumentNullException(argumentName);
            if (string.IsNullOrWhiteSpace(self))
                throw new ArgumentException(argumentName);
            return self;
        }
    }
}
