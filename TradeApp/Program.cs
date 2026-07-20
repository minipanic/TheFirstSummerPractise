using Microsoft.EntityFrameworkCore;
using TradeApp.Components;
using TradeApp.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Включаем поддержку обычных контроллеров API и генератора Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Добавление служб Blazor (ваш старый код)
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Регистрация нашей базы данных (ваш старый код)
builder.Services.AddDbContext<FinanceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// 2. Включаем сам Swagger в режиме разработки (строго перед app.Run())
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // Создает интерактивную страницу со списком запросов
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();

// Включаем маршрутизацию для контроллеров API
app.MapControllers(); 

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();



builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


builder.Services.AddDbContext<FinanceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));



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
