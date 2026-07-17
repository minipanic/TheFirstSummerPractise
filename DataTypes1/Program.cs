public class Class1
{
    public string HardPrecent(double initial_deposit, int years, double interest_rate)
    {
        string ans = "";
        for (int i = 0; i <= years; i++)
        {
            initial_deposit *= interest_rate * 0.01 + 1;
            initial_deposit = Math.Round(initial_deposit, 2, MidpointRounding.AwayFromZero);
            ans = $"{ans}Год {i} : {initial_deposit}.\n"; 
        }
        return ans; 
    }
    public static void Main()
    {
        Class1 program = new Class1();
        Console.WriteLine(program.HardPrecent(1000, 5, 10));
    }
}

