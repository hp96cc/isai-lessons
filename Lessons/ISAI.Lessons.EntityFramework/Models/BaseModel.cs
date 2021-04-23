using Newtonsoft.Json;
using ISAI.Lessons.EntityFramework.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using ISAI.Lessons.Models.Interfaces;

namespace ISAI.Lessons.EntityFramework.Models
{
    public abstract class BaseModel : IBaseInterface
    {
        protected BaseModel()
        {
            DateCreated = DateTime.UtcNow;
            DateModified = DateTime.UtcNow;
        }

        public int Id { get; set; }

        [JsonIgnore]
        public bool Deleted { get; set; }

        [JsonIgnore]
        public DateTimeOffset DateCreated { get; set; }

        [JsonIgnore]
        public DateTimeOffset DateModified { get; set; }

        [JsonIgnore]
        public string ModifiedUserId { get; set; }

        [ForeignKey("ModifiedUserId")]
        public User ModifiedUser { get; set; }

        [JsonIgnore]
        public string CreatedUserId { get; set; }

        [ForeignKey("CreatedUserId")]
        public User CreatedUser { get; set; }


    }
}
