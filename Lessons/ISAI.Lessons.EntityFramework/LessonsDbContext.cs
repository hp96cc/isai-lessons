using Microsoft.AspNet.Identity.EntityFramework;
using ISAI.Lessons.EntityFramework.Models;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace ISAI.Lessons.EntityFramework
{
    public class LessonsDbContext : IdentityDbContext<User>
    {

        public LessonsDbContext() : base("DefaultConnection")
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

        public DbSet<SubscriptionCode> SubscriptionCode { get; set; }

        public DbSet<SubscriptionType> SubscriptionType { get; set; }

        public DbSet<LessonGroup> LessonGroup { get; set; }

        public DbSet<Lesson> Lesson { get; set; }

        public DbSet<CustomerActivity> CustomerActivity { get; set; }

        public DbSet<CustomerDevice> CustomerDevice { get; set; }

        public DbSet<Tutorial> Tutorial { get; set; }

        public DbSet<StripeWebhookLog> StripeWebhookLog { get; set; }

        public DbSet<TutorialSubject> TutorialSubject { get; set; }

        public DbSet<TutorialSubjectGroup> TutorialSubjectGroup { get; set; }

        public DbSet<TutorialSubjectTutorUser> TutorialSubjectTutorUser { get; set; }


        public DbSet<GroupTutorial> GroupTutorial { get; set; }

        public DbSet<GroupTutorialGroup> GroupTutorialGroup { get; set; }

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