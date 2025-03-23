using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace ISAI.Lessons.EntityFramework.Reports
{
    public partial class LessonReports : DbContext
    {
        public LessonReports()
            : base("name=DefaultConnection")
        {
        }

        public virtual DbSet<LessonActivityReport> LessonActivityReports { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
