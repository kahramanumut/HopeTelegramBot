using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class BotDbContext : DbContext
{
    public BotDbContext(DbContextOptions<BotDbContext> options)
            : base(options)
    {

    }

    public DbSet<User> User { get; set; }
    public DbSet<Question> Question { get; set; }
    public DbSet<UserStepTemp> UserStepTemp { get; set; }

}

//In order to add Migration
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<BotDbContext>
{
    public BotDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<BotDbContext>();
        builder.UseSqlServer("Server=YourDb=HopeChatBotDb;Trusted_Connection=True;MultipleActiveResultSets=true",
            optionsBuilder => optionsBuilder.MigrationsAssembly(typeof(BotDbContext).GetTypeInfo().Assembly.GetName().Name));

        return new BotDbContext(builder.Options);
    }
}