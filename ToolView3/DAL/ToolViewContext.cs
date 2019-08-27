using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using ToolView3.Models;

namespace ToolView3.DAL {
    public class ToolViewContext : DbContext {
        public ToolViewContext() : base("ToolViewContext") {
            Database.SetInitializer<ToolViewContext>(new CreateDatabaseIfNotExists<ToolViewContext>());
        }

        public DbSet<Host> Hosts { get; set; }
        public DbSet<Vulnerability> Vulnerabilities { get; set; }
        public DbSet<AppendixData> Appendix { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder) {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}
