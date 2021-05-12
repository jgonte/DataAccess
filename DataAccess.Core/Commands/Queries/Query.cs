using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace DataAccess
{
    /// <summary>
    /// Represents a database command that returns results from the data reader.
    /// Not to be confused with a "pure" query that does not update the database
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Query<T> : Command, 
        ITypeReader<T>
    {
        Action<DbDataReader, T> ITypeReader<T>.OnRecordRead { get; set; }

        PropertyMap ITypeReader<T>.PropertyMap { get; set; }

        TypeMap ITypeReader<T>.TypeMap { get; set; }

        protected override int OnExecute(DbCommand command)
        {
            using (DbDataReader reader = command.ExecuteReader())
            {
                return Read(reader);
            }
        }

        protected override async Task<int> OnExecuteAsync(DbCommand command)
        {
            using (DbDataReader reader = await command.ExecuteReaderAsync())
            {
                return Read(reader);
            }
        }

        public abstract int Read(DbDataReader reader);

        #region Factory methods

        public static SingleQuery<T> Single() => new SingleQuery<T>();

        public static CollectionQuery<T> Collection() => new CollectionQuery<T>(); 

        #endregion
    }
}
