namespace DataAccess
{
    /// <summary>
    /// Defines a type that populates a single strong typed record
    /// </summary>
    public interface IObjectReader<T> : ITypeReader<T>, IRecordInstanceHolder
    {
    }
}