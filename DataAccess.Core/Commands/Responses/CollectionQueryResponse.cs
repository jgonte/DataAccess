using System.Collections.Generic;

namespace DataAccess
{
    public class CollectionQueryResponse<T> : Response
    {
        /// <summary>
        /// The count of the number of records available (such for paging total)
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// The records retreived
        /// </summary>
        public IList<T> Records { get; set; }
    }
}