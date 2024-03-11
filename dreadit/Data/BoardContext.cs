using Microsoft.EntityFrameworkCore;
using Model;

namespace Data
{
    public class BoardContext : DbContext
    {
        public DbSet<User>? Users {get; set; }
        public DbSet<Post>? Posts {get; set; }

        public string? DbPath {get; }
        public BoardContext (DbContextOptions<BoardContext> options)
            : base(options)
        {
            // Den her er tom. Men ": base(options)" sikre at constructor
            // på DbContext super-klassen bliver kaldt.
        }
    }
}