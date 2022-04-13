using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerExemplu
{
    public class Utilizator
    {
        public string nume { get; set; }
        [Key]
        public string email { get; set; }
        public string password { get; set; }

        public string app { get; set; }
    }
    public class UserDbContext : DbContext
    {
        public DbSet<Utilizator> Useri { get; set; }
    }
}
