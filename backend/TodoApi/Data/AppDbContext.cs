using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<UserItem> Users => Set<UserItem>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
}