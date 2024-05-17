using Microsoft.EntityFrameworkCore;

namespace MasterDetailsCore.Models
{
    public class StudentDbContext:DbContext
    {
        public StudentDbContext(DbContextOptions<StudentDbContext> options)
        : base(options)
        {
        }

        public virtual DbSet<Course> Courses { get; set; }

        public virtual DbSet<Module> Modules { get; set; }

        public virtual DbSet<Student> Students { get; set; }
    }
}
