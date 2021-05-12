namespace DataAccess
{
    public class SingleQueryResponse<T> : Response
    {
        public T Record { get; set; }
    }
}