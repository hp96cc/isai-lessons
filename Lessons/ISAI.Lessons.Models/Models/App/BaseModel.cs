using ISAI.Lessons.Models.Interfaces;
using Newtonsoft.Json;
using System;

namespace ISAI.Lessons.Models.Models.App
{
    public abstract class BaseModel : IBaseInterface
    {
        protected BaseModel()
        {
            DateCreated = DateTime.UtcNow;
            DateModified = DateTime.UtcNow;
        }

        [SQLite.PrimaryKey]
        public int Id { get; set; }

        [JsonIgnore]
        public bool Deleted { get; set; }

        [JsonIgnore]
        public DateTimeOffset DateCreated { get; set; }

        [JsonIgnore]
        public DateTimeOffset DateModified { get; set; }

        [JsonIgnore]
        public string ModifiedUserId { get; set; }

        [JsonIgnore]
        public string CreatedUserId { get; set; }




    }
}
