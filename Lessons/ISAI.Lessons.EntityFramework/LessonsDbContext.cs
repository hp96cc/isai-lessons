using Microsoft.AspNet.Identity.EntityFramework;
using ISAI.Lessons.EntityFramework.Models;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace ISAI.Lessons.EntityFramework
{
    public class LessonsDbContext : IdentityDbContext<User>
    {

        public LessonsDbContext() : base("PrintContext")
        {
            Configuration.LazyLoadingEnabled = false;
        }

        public static LessonsDbContext Create()
        {
            return new LessonsDbContext();
        }

        public DbSet<App> App { get; set; }

        public DbSet<Customer> Customer { get; set; }

        public DbSet<Subscription> Subscription { get; set; }

        public DbSet<LessonGroup> LessonGroup { get; set; }

        public DbSet<Lesson> Lesson { get; set; }

        public DbSet<CustomerActivity> CustomerActivity { get; set; }

        public DbSet<CustomerDevice> CustomerDevice { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();

            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<IdentityRole>().ToTable("Role");
            modelBuilder.Entity<IdentityUserRole>().ToTable("UserRole");
            modelBuilder.Entity<IdentityUserClaim>().ToTable("UserClaim");
            modelBuilder.Entity<IdentityUserLogin>().ToTable("UserLogin");

            modelBuilder.Conventions.Remove<DecimalPropertyConvention>();
            modelBuilder.Conventions.Add(new DecimalPropertyConvention(14, 4));
        }
    }
}