using System.Collections.Generic;
using System.Linq;

namespace Helpers
{
    public static class Search
    {
        private static int itemsPerPage = 10;
        public static int ItemsPerPage
        {
            get { return itemsPerPage; }
        }
        public static void ExtractPage<T>(ref IEnumerable<T> allResults, int currentPage)
        {
            allResults = allResults.Skip(SkipBeforePage(currentPage)).Take(itemsPerPage);
        }
        public static void ExtractPage<T>(ref IQueryable<T> allResults, int currentPage)
        {
            if (currentPage >= 1)
                allResults = allResults.Skip(SkipBeforePage(currentPage)).Take(itemsPerPage);
        }
        public static int SkipBeforePage(int currentPage) {
            return (currentPage - 1) * itemsPerPage;
        }
        public static void ProcessPages(int totalCount, int page, out int totalPages, out int currentPage) {
            
            int total = (totalCount / itemsPerPage) + ((totalCount % itemsPerPage) > 0 ? 1 : 0);

            totalPages = total;
            if (page < 1) page = 1;
            else if (page > total) page = total;
            currentPage = page;
        }

    }
}
