using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AoC2022.days
{
    internal class Day06
    {
        public static void Solve()
        {
            const int packageMarkerLength = 4;
            const int messageMarkerLength = 14;

            Console.WriteLine("*** 6th December ***");
            Console.WriteLine();

            Debug.Assert(GetFirstMarker("mjqjpqmgbljsphdztnvjfqwrcgsmlb", packageMarkerLength) == 7);
            Debug.Assert(GetFirstMarker("bvwbjplbgvbhsrlpgdmjqwftvncz", packageMarkerLength) == 5);
            Debug.Assert(GetFirstMarker("nppdvjthqldpwncqszvftbrmjlhg", packageMarkerLength) == 6);
            Debug.Assert(GetFirstMarker("nznrnfrfntjfmvfwmzdfjlvtqnbhcprsg", packageMarkerLength) == 10);
            Debug.Assert(GetFirstMarker("zcfzfwzzqfrljwzlrfnpqdbhtmscgvjw", packageMarkerLength) == 11);

            Debug.Assert(GetFirstMarker("mjqjpqmgbljsphdztnvjfqwrcgsmlb", messageMarkerLength) == 19);
            Debug.Assert(GetFirstMarker("bvwbjplbgvbhsrlpgdmjqwftvncz", messageMarkerLength) == 23);
            Debug.Assert(GetFirstMarker("nppdvjthqldpwncqszvftbrmjlhg", messageMarkerLength) == 23);
            Debug.Assert(GetFirstMarker("nznrnfrfntjfmvfwmzdfjlvtqnbhcprsg", messageMarkerLength) == 29);
            Debug.Assert(GetFirstMarker("zcfzfwzzqfrljwzlrfnpqdbhtmscgvjw", messageMarkerLength) == 26);

            var lines = File.ReadAllLines("data\\d6.txt");
            Debug.Assert(lines != null && lines.Length > 0);
            var firstPacketMarkerPos = GetFirstMarker(lines[0], packageMarkerLength);
            var firstMessageMarkerPos = GetFirstMarker(lines[0], messageMarkerLength);

            Console.WriteLine($"First Package Marker: {firstPacketMarkerPos}");
            Console.WriteLine($"First Message Marker: {firstMessageMarkerPos}");
        }

        private static int GetFirstMarker(string input, int markerLength)
        {
            Debug.Assert(input.Length > markerLength);
            for(var i=markerLength - 1; i<input.Length; i++)
            {
                var markerTest = new HashSet<char>(input.Substring(i - markerLength + 1, markerLength));
                if (markerTest.Count == markerLength) return i + 1;
            }
            return -1;
        }
    }
}
