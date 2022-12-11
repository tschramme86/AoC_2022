using System;
using System.Diagnostics;

namespace AoC2022.days
{
	internal class Day11
	{
        class Monkey
        {
            public Monkey(int idx, IEnumerable<long> startItems, Func<long, long> op, int divByTest, int monkeyIdxWhenTrue, int monkeyIdxWhenFalse)
            {
                this.Idx = idx;
                this.Items = startItems.ToList();
                this.Operation = op;
                this.DivisionTest = divByTest;
                this.MonkeyPassWhenTrue = monkeyIdxWhenTrue;
                this.MonkeyPassWhenFalse = monkeyIdxWhenFalse;
            }

            public int Idx { get; private set; }

            public List<long> Items { get; private set; }

            public Func<long, long> Operation { get; private set; }

            public int DivisionTest { get; private set; }

            public int MonkeyPassWhenTrue { get; private set; }

            public int MonkeyPassWhenFalse { get; private set; }

            public long ItemInspections { get; set; }
        }

		public static void Solve()
		{
            Console.WriteLine("*** 11th December ***");
            Console.WriteLine();

            Debug.Assert(SimulateMonkeys(GetTestMonkeys(), 20, true) == 10605);
            SimulateMonkeys(GetMonkeys(), 20, true);

            Debug.Assert(SimulateMonkeys(GetTestMonkeys(), 10000, false) == 2713310158L);
            SimulateMonkeys(GetMonkeys(), 10000, false);
        }

        static long SimulateMonkeys(Monkey[] monkeys, int rounds, bool manageWorryness)
        {
            Console.WriteLine($"Simulating set with {monkeys.Length} monkeys for {rounds} rounds...");
            var mList = monkeys.OrderBy(m => m.Idx);
            var mDict = monkeys.ToDictionary(x => x.Idx);

            var factor = mList.Aggregate(1, (c, m) => c * m.DivisionTest);

            for (var r=0; r<rounds; r++)
            {
                foreach(var m in mList)
                {
                    foreach(var item in m.Items)
                    {
                        m.ItemInspections++;

                        var newWorryLevel = m.Operation(item);
                        if (manageWorryness)
                            newWorryLevel = (long)Math.Floor(newWorryLevel / 3.0);
                        else
                            newWorryLevel = newWorryLevel % factor;

                        if (newWorryLevel % m.DivisionTest == 0)
                            mDict[m.MonkeyPassWhenTrue].Items.Add(newWorryLevel);
                        else
                            mDict[m.MonkeyPassWhenFalse].Items.Add(newWorryLevel);
                    }
                    m.Items.Clear();
                }
                if (r == 19) Debugger.Break();
            }

            var orderedInspectionList = mList.OrderByDescending(x => x.ItemInspections).ToList();
            var inspectionKpi = orderedInspectionList[0].ItemInspections * orderedInspectionList[1].ItemInspections;

            Console.WriteLine($"Inspection KPI (top 2 monkeys) is {inspectionKpi}");
            return inspectionKpi;
        }

        static Monkey[] GetTestMonkeys()
        {
            return new[]
            {
                new Monkey(0, new[] { 79L, 98 }, x => x * 19, 23, 2, 3),
                new Monkey(1, new[] { 54L, 65, 75, 74 }, x => x + 6, 19, 2, 0),
                new Monkey(2, new[] { 79L, 60, 97 }, x => x * x, 13, 1, 3),
                new Monkey(3, new[] { 74L }, x => x + 3, 17, 0, 1),
            };
        }

        static Monkey[] GetMonkeys()
        {
            return new[]
            {
                new Monkey(0, new[] { 80L }, x => x * 5, 2, 4, 3),
                new Monkey(1, new[] { 75L, 83, 74 }, x => x + 7, 7, 5, 6),
                new Monkey(2, new[] { 86L, 67, 61, 96, 52, 63, 73 }, x => x + 5, 3, 7, 0),
                new Monkey(3, new[] { 85L, 83, 55, 85, 57, 70, 85, 52 }, x => x + 8, 17, 1, 5),
                new Monkey(4, new[] { 67L, 75, 91, 72, 89 }, x => x + 4, 11, 3, 1),
                new Monkey(5, new[] { 66L, 64, 68, 92, 68, 77 }, x => x * 2, 19, 6, 2),
                new Monkey(6, new[] { 97L, 94, 79, 88 }, x => x * x, 5, 2, 7),
                new Monkey(7, new[] { 77L, 85 }, x => x + 6, 13, 4, 0),
            };
        }
    }
}

