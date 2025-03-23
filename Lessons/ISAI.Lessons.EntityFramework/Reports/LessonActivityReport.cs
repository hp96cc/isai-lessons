namespace ISAI.Lessons.EntityFramework.Reports
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("LessonActivityReport")]
    public partial class LessonActivityReport
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CustomerActivityId { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LessonId { get; set; }

        public string Name { get; set; }

        public string Expr1 { get; set; }

        [Key]
        [Column(Order = 2)]
        public DateTimeOffset StartDateTime { get; set; }

        public string CustomerName { get; set; }

        public string CustomerEmail { get; set; }

        public string CustomerDeviceName { get; set; }

        public string SubscriptionName { get; set; }

        public string SubscriptionCode { get; set; }

        public string SubscriptionCodeIssuedTo { get; set; }
    }
}
