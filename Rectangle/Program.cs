using System;



public class Rectangle
{
    private double _x;
    private double _y;
    private double _height;
    private double _width;

    public double X
    {
        get => _x;
        set => _x = value;
    }

    public double Y
    {
        get => _y;
        set => _y = value;
    }

    public double Width
    {
        get => _width;
        set
        {
            if (value < 0)
                throw new ArgumentException("Ширина должна быть положительной!");
            _width = value;
        }
    }

    public double Height
    {
        get => _height;
        set
        {
            if (value < 0)
                throw new ArgumentException("Высота должна быть положительной!");
            _height = value;
        }
    }

    public double Perimeter => 2 * (_width + _height);
    public double Area => _width * _height;

    public Rectangle (double x, double y, double width, double height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
}



class Program
{
    static void Main()
    {
        try
        {
            Console.WriteLine("Создаем прямоугольник со сторонами 5 и 10.");
            Rectangle rect = new Rectangle(0, 0, 5, 10);
            
            Console.WriteLine($"Периметр: {rect.Perimeter}");
            Console.WriteLine($"Площадь: {rect.Area}\n");

            Console.WriteLine("Устанавливаем ширину равным 20.");
            rect.Width = 20;
            Console.WriteLine($"Новая площадь: {rect.Area}\n");

            Console.WriteLine("Попытка установить отрицательную высоту:");
            rect.Height = -5; 
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }
}
