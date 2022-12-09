using System;
using System.Diagnostics;

namespace AoC2022.days
{
	public class Day09
	{
        struct vec2
        {
            public vec2() { }
            public vec2(int px, int py) { this.x = px; this.y = py; }

            public int x;
            public int y;

            public static vec2 operator +(vec2 first, vec2 second)
            {
                return new vec2(first.x + second.x, first.y + second.y);
            }
            public static vec2 operator -(vec2 first, vec2 second)
            {
                return new vec2(first.x - second.x, first.y - second.y);
            }
        };

		public static void Solve()
		{
            Console.WriteLine("*** 9th December ***");
            Console.WriteLine();

            Debug.Assert(SimulateRope("data/d9-test.txt", 2) == 13);
            SimulateRope("data/d9.txt", 2);

            Debug.Assert(SimulateRope("data/d9-test.txt", 10) == 1);
            Debug.Assert(SimulateRope("data/d9-test2.txt", 10) == 36);
            SimulateRope("data/d9.txt", 10);
        }

        static int SimulateRope(string ropeActionsFile, int knots)
        {
            var commands = new Dictionary<string, vec2>
            {
                {"R", new vec2(1, 0) },
                {"L", new vec2(-1, 0) },
                {"U", new vec2(0, 1) },
                {"D", new vec2(0, -1) }
            };


            var rope = new vec2[knots];
            var tailTouched = new HashSet<vec2>() { rope[0] };
            var head = knots - 1;

            var lines = File.ReadAllLines(ropeActionsFile);
            Console.WriteLine($"Simulating a {knots}-knot-rope via file {ropeActionsFile} with {lines.Length} actions");

            foreach(var line in lines)
            {
                var cmdSplit = line.Split(' ');
                Debug.Assert(cmdSplit.Length == 2);

                var cmd = commands[cmdSplit[0]];
                var repititions = int.Parse(cmdSplit[1]);

                for(var i=0; i<repititions; i++)
                {
                    rope[head] = rope[head] + cmd;
                    for(var k=knots - 2; k >= 0; k--)
                    {
                        var dist = rope[k + 1] - rope[k];

                        // need to move the tail?
                        if (Math.Abs(dist.x) > 1 || Math.Abs(dist.y) > 1)
                        {
                            rope[k] = rope[k] + new vec2(Math.Sign(dist.x), Math.Sign(dist.y));
                        }
                    }

                    tailTouched.Add(rope[0]);
                }
            }

            var touchedLocationCount = tailTouched.Count;
            Console.WriteLine($"The tail of the rope touched {touchedLocationCount} positions.");
            Console.WriteLine();
            return touchedLocationCount;
        }
    }
}

