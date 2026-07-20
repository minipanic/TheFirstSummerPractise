using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TradeApp.Data;

namespace TradeApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FinanceController : ControllerBase
{
    private readonly FinanceDbContext _db;

    public FinanceController(FinanceDbContext db) => _db = db;

    // 1. GET-запрос: Получить список всех транзакций из базы для Postman
     [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions()
    {
        var transactions = await _db.Transactions
            .Include(t => t.ExpenseItem)
            .ThenInclude(e => e!.Category)
            .Select(t => new {
                t.TransactionID,
                t.TransactionDate,
                t.Amount,
                t.Comment,
                ExpenseItem = new {
                    t.ExpenseItem!.ExpenseItemID,
                    t.ExpenseItem.Name,
                    Category = new {
                        t.ExpenseItem.Category!.CategoryID,
                        t.ExpenseItem.Category.Name
                    }
                }
            })
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
            
        return Ok(transactions);
    }

    // 2. POST-запрос: Создать новую транзакцию с проверкой ограничений ТЗ из Postman
    [HttpPost("transaction")]
    public async Task<IActionResult> CreateTransaction([FromBody] TransactionDto dto)
    {
        if (dto.Amount <= 0) return BadRequest("Сумма должна быть больше нуля.");

        // Проверяем лимит в 1 000 000 рублей в день (наша старая логика)
        decimal currentDaySum = await _db.Transactions
            .Where(t => t.TransactionDate.Date == dto.TransactionDate.Date)
            .SumAsync(t => t.Amount);

        if (currentDaySum + dto.Amount > 1000000m)
        {
            return BadRequest("Превышено техническое ограничение! Нельзя вводить более 1 000 000 рублей в день.");
        }

        var tx = new Transaction
        {
            TransactionDate = dto.TransactionDate.Date,
            Amount = dto.Amount,
            Comment = dto.Comment,
            ExpenseItemID = dto.ExpenseItemID
        };

        _db.Transactions.Add(tx);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTransactions), new { id = tx.TransactionID }, tx);
    }
}

// Вспомогательный класс-модель для красивого приема JSON из Postman
public class TransactionDto
{
    public DateTime TransactionDate { get; set; }
    public decimal Amount { get; set; }
    public string? Comment { get; set; }
    public int ExpenseItemID { get; set; }
}