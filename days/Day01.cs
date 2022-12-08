using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AoC2022.days
{
    internal class Day01
    {
        public static void Solve()
        {
            Console.WriteLine("*** 1st December ***");

            var lines = File.ReadAllLines("data\\d1.txt");
            var caloriesPerElf = new List<int> { 0 };

            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line))
                {
                    caloriesPerElf.Insert(0, 0);
                    continue;
                }
                caloriesPerElf[0] += int.Parse(line);
            }

            caloriesPerElf = caloriesPerElf.OrderByDescending(x => x).ToList();

            Console.WriteLine($"Number of elfs: {caloriesPerElf.Count}");
            Console.WriteLine($"Top elf calories: {caloriesPerElf[0]}");
            Console.WriteLine($"Top 3 elfs calories: {caloriesPerElf.Take(3).Sum()}");
        }
    }
}
