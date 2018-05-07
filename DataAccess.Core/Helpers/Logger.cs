using System.Diagnostics;

namespace DataAccess
{
    public static class Logger
    {
        /// <summary>
        /// Logs the persistence process
        /// </summary>
        public static TraceSource DataAccess = new TraceSource(GetTraceSourceName("DataAccess"));

        private static string GetTraceSourceName(string category)
        {
            return string.Format("{0}.{1}", typeof(Logger).FullName, category);
        }
    }
}
