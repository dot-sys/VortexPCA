using System;
using System.Collections.Generic;

namespace Vortex.UI.ViewModels
{
    public static class ViewModelHelper
    {
        public static DateTime? ParseTimestamp(string timestamp)
        {
            if (string.IsNullOrWhiteSpace(timestamp))
                return null;

            if (DateTime.TryParse(timestamp, out var dt))
                return dt;

            return null;
        }

        public static void SortByTimestampDescending<T>(List<T> list, Func<T, string> timestampSelector)
        {
            if (list == null)
                return;

            list.Sort((a, b) =>
            {
                var dateA = ParseTimestamp(timestampSelector(a)) ?? DateTime.MinValue;
                var dateB = ParseTimestamp(timestampSelector(b)) ?? DateTime.MinValue;
                return dateB.CompareTo(dateA);
            });
        }
    }
}
