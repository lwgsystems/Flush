using Flush.Data.Identity.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Flush.Data.Identity
{
    /// <summary>
    /// An EF Core based data model context for the ASP.NET Core Identity 3
    /// models.
    /// </summary>
    public class IdentityContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        private readonly IdentityDatabaseOptions _databaseOptions;

        /// <summary>
        /// Construct a new instance of the FlushContext object.
        /// </summary>
        /// <param name="databaseOptions">The database options.</param>
        /// <param name="options">The dbcontext options.</param>
        public IdentityContext(IOptions<IdentityDatabaseOptions> databaseOptions,
            DbContextOptions<IdentityContext> options)
            : base(options)
        {
            _databaseOptions = databaseOptions.Value;
        }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
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
