using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AoC2022.days
{
    internal class Day05
    {
        public static Regex rxMoveStatement = new Regex("move (?<amount>\\d+) from (?<source>\\d+) to (?<target>\\d+)",
            RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public static void Solve()
        {
            Console.WriteLine("*** 5th December ***");
            Console.WriteLine();

            /*
            var lines = File.ReadAllLines("data\\d5-test.txt");
            var stacks = new List<Stack<char>>
            {
                new Stack<char>(new[] { 'Z', 'N' }),
                new Stack<char>(new[] { 'M', 'C', 'D' }),
                new Stack<char>(new[] { 'P' }),
            };
            */
            
            var lines = File.ReadAllLines("data\\d5.txt");
            var stacks = new List<Stack<char>>
            {
                new Stack<char>(new[] { 'W', 'M', 'L', 'F' }),
                new Stack<char>(new[] { 'B', 'Z', 'V', 'M', 'F' }),
                new Stack<char>(new[] { 'H', 'V', 'R', 'S', 'L', 'Q' }),
                new Stack<char>(new[] { 'F', 'S', 'V', 'Q', 'P', 'M', 'T', 'J' }),
                new Stack<char>(new[] { 'L', 'S', 'W' }),
                new Stack<char>(new[] { 'F', 'V', 'P', 'M', 'R', 'J', 'W' }),
                new Stack<char>(new[] { 'J', 'Q', 'C', 'P' ,'N', 'R', 'F' }),
                new Stack<char>(new[] { 'V', 'H', 'P', 'S', 'Z', 'W', 'R', 'B' }),
                new Stack<char>(new[] { 'B', 'M', 'J', 'C', 'G', 'H', 'Z', 'W' }),
            };

            foreach (var line in lines)
            {
                var statement = rxMoveStatement.Match(line);
                if (statement.Success)
                {
                    var amount = int.Parse(statement.Groups["amount"].Value);
                    var source = int.Parse(statement.Groups["source"].Value) - 1;
                    var target = int.Parse(statement.Groups["target"].Value) - 1;

                    var tempStack = new Stack<char>();
                    for(var i=0; i<amount; i++)
                    {
                        var crate = stacks[source].Pop();
                        tempStack.Push(crate);
                    }
                    foreach(var crate in tempStack)
                    {
                        stacks[target].Push(crate);
                    }
                } else
                {
                    Debug.Assert(false);
                }
            }

            var finalStackTops = new StringBuilder();
            foreach(var s in stacks)
                finalStackTops.Append(s.Peek());

            Console.WriteLine($"Final stack arragement: {finalStackTops}");
        }
    }
}
