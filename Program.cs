// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");
using FYP;

namespace Lab1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var sim1 = new Warehouse(seed: 0);
            sim1.Run(TimeSpan.FromDays(60));

        }
    }
}
