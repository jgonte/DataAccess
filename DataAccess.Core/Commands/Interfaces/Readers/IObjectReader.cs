namespace DataAccess
{
    /// <summary>
    /// Defines a type that populates a single strong typed object
    /// </summary>
    public interface IObjectReader<T> : ITypeReader<T>
    {
        /// <summary>
        /// The single object being populated by htis reader
        /// </summary>
        T Object { get; set; }
    }
}