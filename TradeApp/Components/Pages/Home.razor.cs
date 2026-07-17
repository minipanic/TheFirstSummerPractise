using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using TradeApp.Data;

namespace TradeApp.Components.Pages;

public partial class Home
{
    // Данные для вывода транзакций
    private DateTime SelectedDate { get; set; } = DateTime.Today;
    private List<Transaction> DisplayedTransactions { get; set; } = default!;
    private List<ExpenseItem> ActiveItems { get; set; } = default!;
    private string FilterTitle { get; set; } = "за всё время";
    private string CurrentSortMode { get; set; } = "DateDesc";

    // Списки для CRUD справочников
    private List<Category> AllCategories { get; set; } = default!;
    private List<ExpenseItem> AllExpenseItems { get; set; } = default!;


    private int ActiveTab { get; set; } = 1;

    private void ToggleTab(int tabNumber)
    {
        if (ActiveTab == tabNumber)
        {
            ActiveTab = 0;
        }
        else
        {
            ActiveTab = tabNumber;
        }
    }
    // Модели форм управления
    private Transaction NewTx { get; set; } = new() { TransactionDate = DateTime.Today };
    private Category CategoryForm { get; set; } = new();
    private ExpenseItem ItemForm { get; set; } = new() { IsActive = true };

    // Уведомления системы
    private string? ErrorMessage { get; set; }
    private bool SuccessMessage { get; set; }

    // Свойства расчета стикера
    private decimal DayTotal { get; set; }
    private string StickerClass => DayTotal < 500m ? "bg-success text-white" : DayTotal <= 2000m ? "bg-warning text-dark" : "bg-danger text-white";
    private string StickerText => DayTotal < 500m ? "🟢 Экономно" : DayTotal <= 2000m ? "🟡 В пределах обычного" : "🔴 Затратный день";

    protected override async Task OnInitializedAsync()
    {
        await RefreshAllData();
    }

    private async Task RefreshAllData()
    {
        try
        {
            // Синхронизируем все справочники с базой
            AllCategories = await Db.Categories.OrderBy(c => c.Name).ToListAsync();
            AllExpenseItems = await Db.ExpenseItems.Include(e => e.Category).OrderBy(e => e.Name).ToListAsync();
            ActiveItems = AllExpenseItems.Where(i => i.IsActive).ToList();

            // Перечитываем транзакции
            if (FilterTitle == "за всё время") await LoadAllTransactions();
            else if (FilterTitle.StartsWith("за текущий месяц") || FilterTitle.Contains("2026")) await LoadCurrentMonthTransactions();
            else LoadCurrentDayTransactions();

            CalculateSticker();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка обновления данных СУБД: {ex.Message}";
            AllCategories ??= new();
            AllExpenseItems ??= new();
            ActiveItems ??= new();
            DisplayedTransactions ??= new();
        }
    }

    private void OnDateChanged()
    {
        CalculateSticker();
        LoadCurrentDayTransactions();
    }

    private void CalculateSticker()
    {
        DayTotal = Db.Transactions
            .Where(t => t.TransactionDate.Date == SelectedDate.Date)
            .Sum(t => t.Amount);
    }

    private async Task LoadAllTransactions()
    {
        FilterTitle = "за всё время";
        DisplayedTransactions = await Db.Transactions
            .Include(t => t.ExpenseItem).ThenInclude(e => e!.Category)
            .ToListAsync();
        ApplyTransactionSort();
    }

    private void LoadCurrentDayTransactions()
    {
        FilterTitle = $"за {SelectedDate.ToString("dd.MM.yyyy")}";
        DisplayedTransactions = Db.Transactions
            .Where(t => t.TransactionDate.Date == SelectedDate.Date)
            .Include(t => t.ExpenseItem).ThenInclude(e => e!.Category)
            .ToList();
        ApplyTransactionSort();
    }

    private async Task LoadCurrentMonthTransactions()
    {
        FilterTitle = $"за {SelectedDate.ToString("MMMM yyyy")}";
        DisplayedTransactions = await Db.Transactions
            .Where(t => t.TransactionDate.Year == SelectedDate.Year && t.TransactionDate.Month == SelectedDate.Month)
            .Include(t => t.ExpenseItem).ThenInclude(e => e!.Category)
            .ToListAsync();
        ApplyTransactionSort();
    }

    // ТЗ: Реализация сортировок, включая сортировку по категориям трат
    private void ApplyTransactionSort()
    {
        if (DisplayedTransactions == null) return;

        DisplayedTransactions = CurrentSortMode switch
        {
            "CategoryAsc" => DisplayedTransactions.OrderBy(t => t.ExpenseItem?.Category?.Name ?? "ЯЯЯ").ThenByDescending(t => t.TransactionDate).ToList(),
            "AmountDesc" => DisplayedTransactions.OrderByDescending(t => t.Amount).ToList(),
            _ => DisplayedTransactions.OrderByDescending(t => t.TransactionDate).ToList()
        };
    }

    // ==========================================
    // 🛠 БИЗНЕС-ЛОГИКА И CRUD ОПЕРАЦИИ ТРАНЗАКЦИЙ
    // ==========================================
    private async Task HandleAddTransaction()
    {
        ResetAlerts();

        if (NewTx.Amount <= 0) { ErrorMessage = "Сумма должна быть положительной!"; return; }
        if (NewTx.ExpenseItemID == 0) { ErrorMessage = "Выберите статью!"; return; }

        decimal currentDaySum = await Db.Transactions
            .Where(t => t.TransactionDate.Date == NewTx.TransactionDate.Date)
            .SumAsync(t => t.Amount);

        if (currentDaySum + NewTx.Amount > 1000000m)
        {
            ErrorMessage = "Технический предел! Сумма за день превысит 1 000 000 рублей.";
            return;
        }

        try
        {
            var tx = new Transaction
            {
                TransactionDate = NewTx.TransactionDate.Date,
                Amount = NewTx.Amount,
                Comment = NewTx.Comment,
                ExpenseItemID = NewTx.ExpenseItemID
            };

            Db.Transactions.Add(tx);
            await Db.SaveChangesAsync();
            SuccessMessage = true;
            NewTx = new Transaction { TransactionDate = SelectedDate };
            await RefreshAllData();
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
    }

    // ==========================================
    // 🛠 CRUD УПРАВЛЕНИЕ КАТЕГОРИЯМИ
    // ==========================================
    private async Task SaveCategory()
    {
        ResetAlerts();
        if (string.IsNullOrWhiteSpace(CategoryForm.Name)) { ErrorMessage = "Укажите имя категории!"; return; }

        try
        {
            if (CategoryForm.CategoryID == 0) Db.Categories.Add(CategoryForm); // Create
            else Db.Categories.Update(CategoryForm); // Update

            await Db.SaveChangesAsync();
            SuccessMessage = true;
            CategoryForm = new Category();
            await RefreshAllData();
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
    }

    private void EditCategory(Category c) => CategoryForm = new Category { CategoryID = c.CategoryID, Name = c.Name, MonthlyBudget = c.MonthlyBudget, IsActive = c.IsActive };
    private void CancelCategoryEdit() => CategoryForm = new Category();
    
    private async Task DeleteCategory(int id)
    {
        ResetAlerts();
        try
        {
            var hasDependencies = await Db.ExpenseItems.AnyAsync(e => e.CategoryID == id);
            if (hasDependencies) { ErrorMessage = "Нельзя удалить категорию, к которой привязаны статьи расходов! Сначала удалите статьи."; return; }

            var cat = await Db.Categories.FindAsync(id);
            if (cat != null) { Db.Categories.Remove(cat); await Db.SaveChangesAsync(); SuccessMessage = true; }
            await RefreshAllData();
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
    }

    // ==========================================
    // 🛠 CRUD УПРАВЛЕНИЕ СТАТЬЯМИ РАСХОДОВ
    // ==========================================
    private async Task SaveExpenseItem()
    {
        ResetAlerts();
        if (string.IsNullOrWhiteSpace(ItemForm.Name)) { ErrorMessage = "Укажите имя статьи!"; return; }
        if (ItemForm.CategoryID == 0) { ErrorMessage = "Выберите категорию для статьи!"; return; }

        try
        {
            if (ItemForm.ExpenseItemID == 0) Db.ExpenseItems.Add(ItemForm); // Create
            else Db.ExpenseItems.Update(ItemForm); // Update

            await Db.SaveChangesAsync();
            SuccessMessage = true;
            ItemForm = new ExpenseItem { IsActive = true };
            await RefreshAllData();
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
    }

    private void EditExpenseItem(ExpenseItem e) => ItemForm = new ExpenseItem { ExpenseItemID = e.ExpenseItemID, Name = e.Name, CategoryID = e.CategoryID, IsActive = e.IsActive };
    private void CancelItemEdit() => ItemForm = new ExpenseItem { IsActive = true };

    private async Task DeleteExpenseItem(int id)
    {
        ResetAlerts();
        try
        {
            var hasTransactions = await Db.Transactions.AnyAsync(t => t.ExpenseItemID == id);
            if (hasTransactions) { ErrorMessage = "Нельзя удалить статью, по которой уже были совершены траты (историчность данных)! Вместо удаления переключите тумблер активности в положение Выкл."; return; }

            var item = await Db.ExpenseItems.FindAsync(id);
            if (item != null) { Db.ExpenseItems.Remove(item); await Db.SaveChangesAsync(); SuccessMessage = true; }
            await RefreshAllData();
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
    }

    private void ResetAlerts() { ErrorMessage = null; SuccessMessage = false; }
}