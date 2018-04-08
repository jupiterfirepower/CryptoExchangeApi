using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Common.Contracts
{
    public static class Extensions
    {
        public static string FormatAs(this string format, params object[] args)
        {
            return string.Format(format, args);
        }
        public static bool AlmostEquals(this decimal d1, decimal d2, int precision = 8)
        {
            decimal epsilon = (decimal)Math.Pow(10.0, -precision);
            return (Math.Abs(d1 - d2) <= epsilon);
        }

        public static decimal StdDevP(this IEnumerable<decimal> values)
        {
            decimal avg = values.Average();
            decimal stdDev = (decimal)Math.Sqrt(values.Average(v => Math.Pow((double)(v - avg), 2)));
            return stdDev;
        }

        public static string GetAssemblyQualifiedNameVersionInvariant(this Type type)
        {
            var typename = type.AssemblyQualifiedName;

            typename = Regex.Replace(typename, @", Version=\d+.\d+.\d+.\d+", string.Empty);
            typename = Regex.Replace(typename, @", Culture=\w+", string.Empty);
            typename = Regex.Replace(typename, @", PublicKeyToken=\w+", string.Empty);
            return typename;
        }
    }
}
