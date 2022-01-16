
namespace LineTen.Analytics.Data.Options
{
    /// <summary>
    /// Represents strongly typed environment configuration
    /// </summary>
    public class DbOptions
    {
        /// <summary>
        /// The server address
        /// </summary>
        public string DbServer { get; set; }
        /// <summary>
        /// The username
        /// </summary>
        public string DbUsername { get; set; }
        /// <summary>
        /// The password
        /// </summary>
        public string DbPassword { get; set; }
        /// <summary>
        /// The database name
        /// </summary>
        public string DbDatabase { get; set; }
    }
}
