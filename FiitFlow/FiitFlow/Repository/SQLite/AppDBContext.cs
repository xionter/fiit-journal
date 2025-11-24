using FiitFlow.Domain;
using Microsoft.EntityFrameworkCore;

namespace FiitFlow.Repository.Sqlite;

public class AppDbContext : DbContext
{
    public DbSet<GroupEntity> Groups => Set<GroupEntity>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<Points> Points => Set<Points>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    
}