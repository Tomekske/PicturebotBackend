using Api.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Data;

public class ApplicationDBContext : DbContext
{
    public ApplicationDBContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
    {
        
    }
    
    public DbSet<Picture> Pictures { get; set; }
}