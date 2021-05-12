namespace DataAccess
{
    public class NonQueryResponse : Response
    {
        /// <summary>
        /// The affected rows of the command
        /// </summary>
        public int AffectedRows { get; set; }
    }
}