namespace DataAccess
{
    public interface IRecordInstanceHolder
    {
        /// <summary>
        /// The object to populate the parameters from, the output parameters to and the query results to as well
        /// </summary>
        object RecordInstance { get; set; }
    }
}
