using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AoC2022.days
{
    internal class Day21
    {
        class Yell
        {
            public string Name { get; set; } = string.Empty;
            public long? Value { get; set; }
            public string LeftMonkey { get; set; } = string.Empty;
            public string RightMonkey { get; set; } = string.Empty;
            public char Operator { get; set; }
        }

        public static void Solve()
        {
            Console.WriteLine("*** 21th December ***");
            Console.WriteLine();

            Debug.Assert(WhatDoesRootYell("data/d21-test.txt") == 152);
            WhatDoesRootYell("data/d21.txt");

            Debug.Assert(WhatDoIHaveToYell("data/d21-test.txt") == 301);
            WhatDoIHaveToYell("data/d21.txt");
        }

        static long WhatDoesRootYell(string inputFile)
        {
            var monkeys = ReadInput(inputFile);
            Console.WriteLine($"Simulating monkey file {inputFile} ({monkeys.Count} monkeys)...");

            var solution = MonkeySolve(monkeys["root"], monkeys);
            Console.WriteLine($"Monkey 'root' yells {solution}!");
            Console.WriteLine();

            return solution;
        }

        static long WhatDoIHaveToYell(string inputFile)
        {
            var monkeys = ReadInput(inputFile);
            Console.WriteLine($"Simulating monkey file {inputFile} ({monkeys.Count} monkeys) including me...");

            var baseMonkey = monkeys["root"];
            const string searchForName = "humn";

            // which part needs to be solved
            long expectedValue;
            if (IsMonkeyInTree(monkeys[baseMonkey.LeftMonkey], searchForName, monkeys))
            {
                expectedValue = MonkeySolve(monkeys[baseMonkey.RightMonkey], monkeys);
                baseMonkey = monkeys[baseMonkey.LeftMonkey];
            }
            else
            {
                expectedValue = MonkeySolve(monkeys[baseMonkey.LeftMonkey], monkeys);
                baseMonkey = monkeys[baseMonkey.RightMonkey];
            }

            var solution = 0L;
            while (true)
            {
                var isInLeftTree = IsMonkeyInTree(monkeys[baseMonkey.LeftMonkey], searchForName, monkeys);
                var isInRightTree = IsMonkeyInTree(monkeys[baseMonkey.RightMonkey], searchForName, monkeys);
                Debug.Assert(isInLeftTree != isInRightTree);

                if (isInLeftTree)
                {
                    // I'm in the left part of the equation, calculate the right part
                    var rightValue = MonkeySolve(monkeys[baseMonkey.RightMonkey], monkeys);

                    // now check what should be on the left side
                    var expectedLeftValue = baseMonkey.Operator switch
                    {
                        '+' => expectedValue - rightValue,
                        '-' => expectedValue + rightValue,
                        '*' => expectedValue / rightValue,
                        '/' => expectedValue * rightValue,
                        _ => throw new InvalidOperationException()
                    };

                    // is it me?
                    if(monkeys[baseMonkey.LeftMonkey].Name == searchForName)
                    {
                        solution = expectedLeftValue;
                        break;
                    }
                    expectedValue = expectedLeftValue;
                    baseMonkey = monkeys[baseMonkey.LeftMonkey];
                } else {
                    // I'm in the right part of the equation, calculate the left part
                    var leftValue = MonkeySolve(monkeys[baseMonkey.LeftMonkey], monkeys);

                    // now check what should be on the right side
                    var expectedRightValue = baseMonkey.Operator switch
                    {
                        '+' => expectedValue - leftValue,
                        '-' => leftValue - expectedValue,
                        '*' => expectedValue / leftValue,
                        '/' => leftValue / expectedValue,
                        _ => throw new InvalidOperationException()
                    };

                    // is it me?
                    if (monkeys[baseMonkey.RightMonkey].Name == searchForName)
                    {
                        solution = expectedRightValue;
                        break;
                    }
                    expectedValue = expectedRightValue;
                    baseMonkey = monkeys[baseMonkey.RightMonkey];
                }
            }

            Console.WriteLine($"I have to yell {solution}!");
            Console.WriteLine();

            return solution;
        }

        static long MonkeySolve(Yell baseMonkey, Dictionary<string, Yell> allMonkeys)
        {
            if (baseMonkey.Value.HasValue) return baseMonkey.Value.Value;

            // build search tree
            var monkeys = new Dictionary<string, Yell>();
            var search = new Queue<Yell>();
            search.Enqueue(baseMonkey);
            while(search.Count > 0)
            {
                var monkey = search.Dequeue();
                monkeys.Add(monkey.Name, monkey);

                if (!string.IsNullOrEmpty(monkey.LeftMonkey)) search.Enqueue(allMonkeys[monkey.LeftMonkey]);
                if (!string.IsNullOrEmpty(monkey.RightMonkey)) search.Enqueue(allMonkeys[monkey.RightMonkey]);
            }

            // solve the tree
            long? solution = null;
            while (true)
            {
                foreach (var monkey in monkeys.Values)
                {
                    if (!monkey.Value.HasValue)
                    {
                        var o1 = monkeys[monkey.LeftMonkey];
                        var o2 = monkeys[monkey.RightMonkey];
                        if (o1.Value.HasValue && o2.Value.HasValue)
                        {
                            monkey.Value = monkey.Operator switch
                            {
                                '+' => o1.Value + o2.Value,
                                '-' => o1.Value - o2.Value,
                                '/' => o1.Value / o2.Value,
                                '*' => o1.Value * o2.Value,
                                _ => throw new InvalidOperationException()
                            };
                        }
                    }

                    if (monkey.Name == baseMonkey.Name && monkey.Value.HasValue)
                    {
                        solution = monkey.Value;
                        break;
                    }
                }
                if (solution.HasValue) break;
            }

            return solution ?? -1;
        }

        static bool IsMonkeyInTree(Yell startMonkey, string search, Dictionary<string, Yell> allMonkeys)
        {
            if (startMonkey.Name == search) return true;
            if (startMonkey.Value.HasValue) return false;

            return IsMonkeyInTree(allMonkeys[startMonkey.LeftMonkey], search, allMonkeys) ||
                IsMonkeyInTree(allMonkeys[startMonkey.RightMonkey], search, allMonkeys);
        }

        static Dictionary<string, Yell> ReadInput(string inputFile)
        {
            var monkeys = new Dictionary<string, Yell>();
            foreach (var line in File.ReadAllLines(inputFile))
            {
                var parts = line.Split(' ');
                var m = new Yell { Name = parts[0].Substring(0, parts[0].Length - 1) };
                if (parts.Length == 2)
                {
                    m.Value = int.Parse(parts[1]);
                }
                else
                {
                    m.LeftMonkey = parts[1];
                    m.Operator = parts[2][0];
                    m.RightMonkey = parts[3];
                }
                monkeys[m.Name] = m;
            }

            return monkeys;
        }
    }
}
