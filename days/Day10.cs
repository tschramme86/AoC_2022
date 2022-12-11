using System;
using System.Diagnostics;

namespace AoC2022.days
{
	internal class Day10
	{
		public static void Solve()
		{
            Console.WriteLine("*** 10th December ***");
            Console.WriteLine();

            Debug.Assert(SimulateCpu("data/d10-test.txt") == 13140);
            SimulateCpu("data/d10.txt");
        }

        static int SimulateCpu(string instructionsFile)
        {
            var lines = File.ReadAllLines(instructionsFile);
            Console.WriteLine($"Simulating cpu file {instructionsFile} with {lines.Length} instructions");

            var cycleValues = new List<Tuple<int, int>>();

            var x = 1;
            foreach(var line in lines)
            {
                var instrOp = line.Split(' ');
                switch(instrOp[0])
                {
                    case "noop":
                        cycleValues.Add(Tuple.Create(x, x));
                        break;

                    case "addx":
                        Debug.Assert(instrOp.Length == 2);
                        var newX = x + int.Parse(instrOp[1]);
                        cycleValues.Add(Tuple.Create(x, x));
                        cycleValues.Add(Tuple.Create(x, newX));
                        x = newX;
                        break;
                }
            }

            var signalSum = 0;
            for(var c=20; c<=220; c+= 40)
            {
                signalSum += cycleValues[c - 1].Item1 * c;
            }

            Console.WriteLine($"The sum of signal strenghtes is {signalSum} in total.");
            Console.WriteLine();

            for (var sx = 0; sx < cycleValues.Count; sx += 40)
            {
                for(var pixel = 0; pixel < 40; pixel++)
                {
                    var spriteStart = cycleValues[sx + pixel].Item1 - 1;
                    var spriteEnd = cycleValues[sx + pixel].Item1 + 1;
                    if (spriteStart <= pixel && pixel <= spriteEnd)
                        Console.Write("#");
                    else
                        Console.Write(".");
                }
                Console.WriteLine();
            }
            Console.WriteLine();

            return signalSum;
        }
    }
}

