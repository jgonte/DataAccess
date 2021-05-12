namespace DataAccess
{
    /// <summary>
    /// Defines a type that populates a single strong typed record
    /// </summary>
    public interface IObjectReader<T> : ITypeReader<T>
    {
        /// <summary>
        /// The single record being read by this reader
        /// </summary>
        T Record { get; set; }
    }
}