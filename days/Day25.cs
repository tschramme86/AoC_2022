using Google.OrTools.LinearSolver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AoC2022.days
{
    internal class Day25
    {
        public static void Solve()
        {
            Console.WriteLine("*** 25th December ***");
            Console.WriteLine();

            Debug.Assert(SolveSNAFU("data/d25-test.txt") == "2=-1=0");
            SolveSNAFU("data/d25.txt");
        }

        static string SolveSNAFU(string inputFile)
        {
            Console.WriteLine($"Reading + converting SNAFU numbers from '{inputFile}'...");
            var decNumbers = new List<long>();
            var snafuLines = File.ReadAllLines(inputFile);
            foreach(var snafuLine in snafuLines )
            {
                decNumbers.Add(SnafuToDec(snafuLine));
            }

            var sum = decNumbers.Sum();
            var snafuSum = DecToSnafu(sum);

            var check = SnafuToDec(snafuSum);
            Debug.Assert(check == sum);

            Console.WriteLine($"Sum (dec) = {sum}, Sum (SNAFU) = {snafuSum}");
            Console.WriteLine();

            return snafuSum;
        }

        static long SnafuToDec(string snafuNumber)
        {
            var n = 0L;
            for(var i=0; i<snafuNumber.Length; i++)
            {
                var p = (long)Math.Pow(5, i);
                var d = snafuNumber[snafuNumber.Length - i - 1] switch
                {
                    '2' => 2,
                    '1' => 1,
                    '0' => 0,
                    '-' => -1,
                    '=' => -2,
                    _ => throw new InvalidDataException()
                };
                n += d * p;
            }
            return n;
        }

        static string DecToSnafu(long decimalNumber)
        {
            var solver = Solver.CreateSolver("SCIP");
            if (solver == null) throw new Exception("Could not create MIP solver");

            var powers = new Dictionary<int, Variable>();
            for(var i=0; i<20; i++)
            {
                powers[i] = solver.MakeIntVar(-2, 2, "p_" + i);
            }

            var constr = solver.MakeConstraint(decimalNumber, decimalNumber, "isValue");
            for (var i = 0; i < powers.Count; i++)
            {
                constr.SetCoefficient(powers[i], Math.Pow(5, i));
            }

            var obj = solver.Objective();
            obj.SetMinimization();
            for (var i = 0; i < powers.Count; i++)
            {
                obj.SetCoefficient(powers[i], Math.Pow(5, i));
            }

            var sol = solver.Solve();
            if(sol == Solver.ResultStatus.OPTIMAL)
            {
                var s = new StringBuilder();
                for (var i = 0; i < powers.Count; i++)
                {
                    var c = (int)powers[i].SolutionValue() switch
                    {
                        2 => "2",
                        1 => "1",
                        0 => "0",
                        -1 => "-",
                        -2 => "=",
                        _ => throw new InvalidDataException()
                    };
                    s.Append(c);
                }

                var snafuNumber = new string(s.ToString().Reverse().ToArray());
                var firstNot0 = 0;
                for(var i=0; i<snafuNumber.Length;i++)
                {
                    if (snafuNumber[i] != '0')
                    {
                        firstNot0 = i;
                        break;
                    }
                }
                return snafuNumber.Substring(firstNot0);
            }

            return "";
        }
    }
}
