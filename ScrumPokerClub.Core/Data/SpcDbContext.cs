using Microsoft.EntityFrameworkCore;
using ScrumPokerClub.Data.Entities;
using System;
using System.IO;

namespace ScrumPokerClub.Data
{
    /// <summary>
    /// 
    /// </summary>
    public class SpcDbContext : DbContext
    {
        /// <summary>
        /// 
        /// </summary>
        public DbSet<Profile> Profiles { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DbSet<Session> Sessions { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string DbPath { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public SpcDbContext()
        {
            var root = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(root);
            DbPath = $"{path}{Path.DirectorySeparatorChar}spc.db";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContextOptionsBuilder"></param>
        protected override void OnConfiguring(DbContextOptionsBuilder dbContextOptionsBuilder)
        {
            dbContextOptionsBuilder.UseSqlite($"DataSource={DbPath}");
        }
    }
}
