using Microsoft.EntityFrameworkCore;
using TradeApp.Components;
using TradeApp.Data;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


builder.Services.AddDbContext<FinanceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();


public class Category
{
    public int CategoryID { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal MonthlyBudget { get; set; }
    public bool IsActive { get; set; } = true;
    public List<ExpenseItem> ExpenseItems { get; set; } = new();
}

public class ExpenseItem
{
    public int ExpenseItemID { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CategoryID { get; set; }
    public Category? Category { get; set; }
    public bool IsActive { get; set; } = true;
    public List<Transaction> Transactions { get; set; } = new();
}

public class Transaction
{
    public int TransactionID { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal Amount { get; set; }
    public string? Comment { get; set; }
    public int ExpenseItemID { get; set; }
    public ExpenseItem? ExpenseItem { get; set; }
}
