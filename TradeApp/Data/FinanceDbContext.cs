using Microsoft.EntityFrameworkCore;
namespace TradeApp.Data; 

public class FinanceDbContext : DbContext
{
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<ExpenseItem> ExpenseItems { get; set; } = null!;
    public DbSet<Transaction> Transactions { get; set; } = null!;

    public FinanceDbContext(DbContextOptions<FinanceDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExpenseItem>()
            .HasOne(e => e.Category)
            .WithMany(c => c.ExpenseItems)
            .HasForeignKey(e => e.CategoryID);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.ExpenseItem)
            .WithMany(e => e.Transactions)
            .HasForeignKey(t => t.ExpenseItemID);
    }
}
