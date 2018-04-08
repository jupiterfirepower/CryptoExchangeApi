using System;
using System.Globalization;
using System.Linq;

namespace Common.Contracts
{
    public class CultureHelper
    {
        private const string EnglishCultureName = "English";

        public static CultureInfo GetEnglishCulture()
        {
            CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.AllCultures & ~CultureTypes.NeutralCultures);
            // get culture by it's english name 
            var culture = cultures.FirstOrDefault(c => c.EnglishName.Equals(EnglishCultureName, StringComparison.InvariantCultureIgnoreCase));
            return culture;
        }
    }
}
