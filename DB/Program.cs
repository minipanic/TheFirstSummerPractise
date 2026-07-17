using System;
using System.Data;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

// ==========================================
// 1. СЛОЙ МОДЕЛЕЙ И КОНТЕКСТА ДЛЯ EF CORE
// ==========================================
public class Developer
{
    [Key]
    public int DeveloperID { get; set; }
    public string Name { get; set; }
    public string Country { get; set; }
}

public class GameStoreContext : DbContext
{
    private readonly string _connectionString;
    public DbSet<Developer> Developers { get; set; }

    public GameStoreContext(string connectionString) => _connectionString = connectionString;
    protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlServer(_connectionString);
}

// ==========================================
// 2. ОСНОВНАЯ ПРОГРАММА И МЕНЮ
// ==========================================
class Program
{
    static void Main()
    {
        var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        string connectionString = config.GetConnectionString("DefaultConnection");

        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== УПРАВЛЕНИЕ РАЗРАБОТЧИКАМИ ИГР ===");
            Console.WriteLine("1. Выполнить CRUD через ADO.NET");
            Console.WriteLine("2. Выполнить CRUD через Entity Framework Core");
            Console.WriteLine("0. Выход");
            Console.Write("\nВыберите режим работы: ");

            string mode = Console.ReadLine();
            if (mode == "0") break;
            if (mode != "1" && mode != "2") continue;

            RunCrudMenu(connectionString, mode == "1");
        }
    }

    static void RunCrudMenu(string connString, bool useAdoNet)
    {
        string techName = useAdoNet ? "ADO.NET" : "EF Core";
        while (true)
        {
            Console.Clear();
            Console.WriteLine($"--- РЕЖИМ: {techName} ---");
            Console.WriteLine("1. Просмотр списка (READ)");
            Console.WriteLine("2. Добавить запись (CREATE)");
            Console.WriteLine("3. Изменить запись (UPDATE)");
            Console.WriteLine("4. Удалить запись (DELETE)");
            Console.WriteLine("0. Вернуться в главное меню");
            Console.Write("\nВыберите операцию: ");

            string choice = Console.ReadLine();
            if (choice == "0") break;

            Console.Clear();
            switch (choice)
            {
                case "1":
                    if (useAdoNet) AdoRead(connString); else EfRead(connString);
                    break;
                case "2":
                    Console.Write("Введите название студии: "); string name = Console.ReadLine();
                    Console.Write("Введите страну: "); string country = Console.ReadLine();
                    if (useAdoNet) AdoCreate(connString, name, country); else EfCreate(connString, name, country);
                    break;
                case "3":
                    Console.Write("Введите ID для изменения: "); if (!int.TryParse(Console.ReadLine(), out int uId)) break;
                    Console.Write("Введите новое название: "); string uName = Console.ReadLine();
                    Console.Write("Введите новую страну: "); string uCountry = Console.ReadLine();
                    if (useAdoNet) AdoUpdate(connString, uId, uName, uCountry); else EfUpdate(connString, uId, uName, uCountry);
                    break;
                case "4":
                    Console.Write("Введите ID для удаления: "); if (!int.TryParse(Console.ReadLine(), out int dId)) break;
                    if (useAdoNet) AdoDelete(connString, dId); else EfDelete(connString, dId);
                    break;
            }
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }
    }

    // ==========================================
    // 3. РЕАЛИЗАЦИЯ МЕТОДОВ ADO.NET
    // ==========================================
    static void AdoCreate(string connString, string name, string country)
    {
        using var conn = new SqlConnection(connString);
        string sql = "INSERT INTO Developers (Name, Country) OUTPUT INSERTED.DeveloperID VALUES (@Name, @Country)";
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Name", name);
        cmd.Parameters.AddWithValue("@Country", country);
        conn.Open();
        int newId = (int)cmd.ExecuteScalar();
        Console.WriteLine($"[ADO.NET] Запись успешно добавлена. Новый ID: {newId}");
    }

    static void AdoRead(string connString)
    {
        using var conn = new SqlConnection(connString);
        string sql = "SELECT DeveloperID, Name, Country FROM Developers";
        using var cmd = new SqlCommand(sql, conn);
        conn.Open();
        using var reader = cmd.ExecuteReader();
        Console.WriteLine("ID\t| Название\t\t| Страна");
        Console.WriteLine("------------------------------------------");
        while (reader.Read())
        {
            Console.WriteLine($"{reader["DeveloperID"]}\t| {reader["Name"]}\t\t| {reader["Country"]}");
        }
    }

    static void AdoUpdate(string connString, int id, string name, string country)
    {
        using var conn = new SqlConnection(connString);
        string sql = "UPDATE Developers SET Name = @Name, Country = @Country WHERE DeveloperID = @Id";
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.Parameters.AddWithValue("@Name", name);
        cmd.Parameters.AddWithValue("@Country", country);
        conn.Open();
        int rows = cmd.ExecuteNonQuery();
        Console.WriteLine(rows > 0 ? "[ADO.NET] Запись успешно обновлена." : "[ADO.NET] Запись с таким ID не найдена.");
    }

    static void AdoDelete(string connString, int id)
    {
        using var conn = new SqlConnection(connString);
        string sql = "DELETE FROM Developers WHERE DeveloperID = @Id";
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        conn.Open();
        int rows = cmd.ExecuteNonQuery();
        Console.WriteLine(rows > 0 ? "[ADO.NET] Запись успешно удалена." : "[ADO.NET] Запись с таким ID не найдена.");
    }

    // ==========================================
    // 4. РЕАЛИЗАЦИЯ МЕТОДОВ ENTITY FRAMEWORK
    // ==========================================
    static void EfCreate(string connString, string name, string country)
    {
        using var db = new GameStoreContext(connString);
        var dev = new Developer { Name = name, Country = country };
        db.Developers.Add(dev);
        db.SaveChanges();
        Console.WriteLine($"[EF Core] Запись успешно добавлена. Новый ID: {dev.DeveloperID}");
    }

    static void EfRead(string connString)
    {
        using var db = new GameStoreContext(connString);
        var list = db.Developers.ToList();
        Console.WriteLine("ID\t| Название\t\t| Страна");
        Console.WriteLine("------------------------------------------");
        foreach (var d in list)
        {
            Console.WriteLine($"{d.DeveloperID}\t| {d.Name}\t\t| {d.Country}");
        }
    }

    static void EfUpdate(string connString, int id, string name, string country)
    {
        using var db = new GameStoreContext(connString);
        var dev = db.Developers.FirstOrDefault(d => d.DeveloperID == id);
        if (dev != null)
        {
            dev.Name = name;
            dev.Country = country;
            db.SaveChanges();
            Console.WriteLine("[EF Core] Запись успешно обновлена.");
        }
        else
        {
            Console.WriteLine("[EF Core] Запись с таким ID не найдена.");
        }
    }

    static void EfDelete(string connString, int id)
    {
        using var db = new GameStoreContext(connString);
        var dev = db.Developers.FirstOrDefault(d => d.DeveloperID == id);
        if (dev != null)
        {
            db.Developers.Remove(dev);
            db.SaveChanges();
            Console.WriteLine("[EF Core] Запись успешно удалена.");
        }
        else
        {
            Console.WriteLine("[EF Core] Запись с таким ID не найдена.");
        }
    }
}
