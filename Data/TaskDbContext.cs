using CustodialTaskList.Models;
using Microsoft.EntityFrameworkCore;

namespace CustodialTaskList.Data
{
    public class TaskDbContext : DbContext
    {
        public TaskDbContext(
            DbContextOptions<TaskDbContext> options)
            : base(options)
        {
        }

        public DbSet<TaskTemplate> TaskTemplates =>
            Set<TaskTemplate>();

        public DbSet<DailyTask> DailyTasks =>
            Set<DailyTask>();

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TaskTemplate>()
                .ToTable("TaskTemplates");

            modelBuilder.Entity<DailyTask>()
                .ToTable("DailyTasks");

            modelBuilder.Entity<TaskTemplate>()
                .Property(t => t.IsActive)
                .HasDefaultValue(true);

            modelBuilder.Entity<DailyTask>()
                .HasIndex(t => new
                {
                    t.TaskTemplateId,
                    t.ScheduledDate
                })
                .IsUnique();
        }
    }
}
