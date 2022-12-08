using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AoC2022.days
{
    internal class Day03
    {
        public static void Solve() 
        {
            Console.WriteLine("*** 3rd December ***");

            var lines = File.ReadAllLines("data\\d3.txt");
            var sumPriorities = 0;

            // story part one
            foreach (var line in lines)
            {
                Debug.Assert(line.Length % 2 == 0);
                var firstCompartment = line.Substring(0, line.Length / 2);
                var secondCompartment = line.Substring(line.Length / 2);

                var appearsInBoth = new List<int>();
                for (var c = 'a'; c <= 'z'; c++)
                    if (firstCompartment.Contains(c) && secondCompartment.Contains(c))
                        appearsInBoth.Add(c - 'a' + 1);
                for (var c = 'A'; c <= 'Z'; c++)
                    if (firstCompartment.Contains(c) && secondCompartment.Contains(c))
                        appearsInBoth.Add(c - 'A' + 27);

                Debug.Assert(appearsInBoth.Count == 1);
                sumPriorities += appearsInBoth[0];
            }

            Console.WriteLine($"Sum of Priorities (each rucksack): {sumPriorities}");

            // story part two
            sumPriorities = 0;
            foreach(var groupOfLines in lines.Chunk(3))
            {
                var appearsInAll = new List<int>();
                for (var c = 'a'; c <= 'z'; c++)
                    if (groupOfLines.All(line => line.Contains(c)))
                        appearsInAll.Add(c - 'a' + 1);
                for (var c = 'A'; c <= 'Z'; c++)
                    if (groupOfLines.All(line => line.Contains(c)))
                        appearsInAll.Add(c - 'A' + 27);

                Debug.Assert(appearsInAll.Count == 1);
                sumPriorities += appearsInAll[0];
            }

            Console.WriteLine($"Sum of Priorities (group of 3): {sumPriorities}");
        }
    }
}
