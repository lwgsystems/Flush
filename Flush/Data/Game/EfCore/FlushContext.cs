using Flush.Data.Game.Model;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Flush.Data.Game.EfCore
{
    /// <summary>
    /// An EF Core based data model context for the Flush game.
    /// </summary>
    public class FlushContext : DbContext
    {
        private readonly ILogger<FlushContext> _logger;
        private readonly FlushDatabaseOptions _databaseOptions;

        /// <summary>
        /// Construct a new instance of the FlushContext object.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="databaseOptions">The database options.</param>
        /// <param name="options">The dbcontext options.</param>
        public FlushContext(ILogger<FlushContext> logger,
            IOptions<FlushDatabaseOptions> databaseOptions,
            DbContextOptions<FlushContext> options)
            : base(options)
        {
            _databaseOptions = databaseOptions.Value;
            _logger = logger;
        }

        /// <summary>
        /// The games.
        /// </summary>
        public DbSet<Model.Game> Games { get; set; }

        /// <summary>
        /// The players.
        /// </summary>
        public DbSet<Player> Players { get; set; }

        /// <summary>
        /// The logs.
        /// </summary>
        public DbSet<AuditLog> AuditLogs { get; set; }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        /// <inheritdoc />
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = new SqliteConnectionStringBuilder(
                _databaseOptions.ConnectionString)
            {
                Mode = SqliteOpenMode.ReadWriteCreate,
                Password = _databaseOptions.Key
            }.ToString();
            optionsBuilder.UseSqlite(connectionString);
        }
    }
}
