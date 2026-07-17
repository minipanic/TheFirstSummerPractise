using System;
using System.Text;
public class Class2()
{
    public string Diamante(int N)
    {
        if (N % 2 != 1)
            return "Введено не целое значение!";
        int HalfedN = N / 2;
        int OutSpace, InSpace;
        StringBuilder sb = new StringBuilder(100);
        for (int i = 0; i < N; i++)
        {
            OutSpace = Math.Abs(HalfedN - i);
            InSpace = (HalfedN - OutSpace) * 2 - 1;
            if (i == 0 || i == N-1)
            {
                sb.Append(' ', HalfedN);
                sb.Append('X');
            }
            else
            {
                sb.Append(' ', OutSpace);
                sb.Append('X');
                
                sb.Append(' ', InSpace);
                sb.Append('X');
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }
    public static void Main()
    {
        Class2 program = new Class2();
        Console.WriteLine(program.Diamante(5));
    }
}