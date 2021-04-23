using System;
using System.Collections.Generic;
using System.Text;

namespace ISAI.Lessons.Models.Models
{
    public class AppUser
    {

        [SQLite.PrimaryKey]
        public string Id { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public int MaxFileDownloads { get; set; }

    }
}
