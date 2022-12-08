using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AoC2022.days
{
    internal class Day04
    {
        public static void Solve()
        {
            Console.WriteLine("*** 4th December ***");
            Console.WriteLine();

            var lines = File.ReadAllLines("data\\d4.txt");
            var numberOfOverlappings = 0;
            var numberOfPartlyOverlappings = 0;

            foreach (var line in lines)
            {
                var pair = line.Split(',');

                var e1FromTo = pair[0].Split('-');
                var e1From = int.Parse(e1FromTo[0]);
                var e1To = int.Parse(e1FromTo[1]);

                var e2FromTo = pair[1].Split('-');
                var e2From = int.Parse(e2FromTo[0]);
                var e2To = int.Parse(e2FromTo[1]);

                Debug.Assert(e1From <= e1To && e2From <= e2To);
                
                // story part one: total overlapping assignments
                if(
                    (e1From <= e2From && e1To >= e2To) || 
                    (e2From <= e1From && e2To >= e1To))
                {
                    Console.WriteLine($" - Overlapping assingment: {line}");
                    numberOfOverlappings++;
                }

                // story part two: partly overlapping assignments
                if(
                    (e1From <= e2From && e2From <= e1To) ||
                    (e1From <= e2To && e2To <= e1To) ||
                    (e2From <= e1From && e1From <= e2To) ||
                    (e2From <= e1To && e1To <= e2To))
                {
                    Console.WriteLine($" - Partly overlapping assingment: {line}");
                    numberOfPartlyOverlappings++;
                }
            }

            Console.WriteLine($"Total overlapping assignments: {numberOfOverlappings}");
            Console.WriteLine($"Partly overlapping assignments: {numberOfPartlyOverlappings}");
        }
    }
}
