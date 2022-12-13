using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AoC2022.days
{
    internal class Day13
    {
        static Regex rxPackageTokens = new Regex("(?<ListStart>\\[)|(?<ListEnd>\\])|(?<Value>\\d+)",
            RegexOptions.CultureInvariant | RegexOptions.Compiled);

        abstract class PListItem { 
            public PList? Parent { get; set; }
        }

        [DebuggerDisplay("Items = {Items.Count}")]
        class PList : PListItem 
        {
            public List<PListItem> Items { get; } = new List<PListItem>();

            public bool IsDivider { get; set; }
        }

        [DebuggerDisplay("Value = {Value}")]
        class PValue : PListItem
        {
            public int Value { get; set; }
        }

        class PListComparer : IComparer<PList>
        {
            public int Compare(PList? x, PList? y)
            {
                return CompareLists(y!, x!);
            }
        }

        public static void Solve()
        {
            Console.WriteLine("*** 13th December ***");
            Console.WriteLine();

            Debug.Assert(ComparePackageList("data/d13-test.txt") == 13);
            ComparePackageList("data/d13.txt");

            Debug.Assert(FindDecoderKey("data/d13-test.txt") == 140);
            FindDecoderKey("data/d13.txt");
        }

        static int ComparePackageList(string packageListFile)
        {
            var lines = File.ReadLines(packageListFile);
            var leftPackages = new List<string>();
            var rightPackages = new List<string>();

            foreach (var line in lines)
            {
                if(string.IsNullOrEmpty(line)) continue;
                if (leftPackages.Count == rightPackages.Count)
                    leftPackages.Add(line);
                else 
                    rightPackages.Add(line);
            }

            Debug.Assert(leftPackages.Count == rightPackages.Count);
            Console.WriteLine($"Checking '{packageListFile}' with {leftPackages.Count} package-pairs...");

            var correctOrderPackages = new List<int>();
            var incorrectOrderPackages = new List<int>();
            for(var p=0; p<leftPackages.Count; p++)
            {
                var leftPackage = ParseList(leftPackages[p]);
                var rightPackage = ParseList(rightPackages[p]);

                var cmp = CompareLists(leftPackage, rightPackage);
                if(cmp < 0) incorrectOrderPackages.Add(p);
                if(cmp > 0) correctOrderPackages.Add(p);
                if (cmp == 0) Debug.Assert(false, "Cannot make decision for these lists");
            }

            var indicesSum = correctOrderPackages.Sum(x => x + 1);
            Console.WriteLine($"Found {correctOrderPackages.Count} packages in correct order, indices sum = {indicesSum}");
            Console.WriteLine();
            return indicesSum;
        }

        static int FindDecoderKey(string packageListFile)
        {
            var lines = File.ReadLines(packageListFile);

            // read all packages
            var packages = new List<PList>();
            foreach (var line in lines)
            {
                if (!string.IsNullOrEmpty(line))
                    packages.Add(ParseList(line));
            }

            Console.WriteLine($"Ordering '{packageListFile}' with {packages.Count} packages...");

            var div1 = ParseList("[[2]]");
            div1.IsDivider = true;
            packages.Add(div1);

            var div2 = ParseList("[[6]]");
            div2.IsDivider = true;
            packages.Add(div2);

            packages.Sort(new PListComparer());

            var idx1 = packages.IndexOf(div1) + 1;
            var idx2 = packages.IndexOf(div2) + 1;
            var decoderKey = idx1 * idx2;

            Console.WriteLine($"Decoder key for this package list is {decoderKey}");
            Console.WriteLine();

            return decoderKey;
        }

        static int CompareLists(PList left, PList right)
        {
            // compare all list items
            for(var i=0; i<Math.Max(left.Items.Count, right.Items.Count); i++)
            {
                // left list run out of items first
                if (i == left.Items.Count)
                    return 1; // correct order
                
                // right list run out of items first
                if (i == right.Items.Count)
                    return -1; // incorrect order

                // both items are values, compare them directly
                if (left.Items[i] is PValue lV && right.Items[i] is PValue rV)
                {
                    if (lV.Value < rV.Value)
                        return 1;
                    else if (lV.Value > rV.Value)
                        return -1;
                }
                // both items are lists, start new list comparison
                else if (left.Items[i] is PList lL && right.Items[i] is PList rL)
                {
                    var compareSubList = CompareLists(lL, rL);
                    if(compareSubList != 0) return compareSubList; // there is a decision, return it
                }
                // left item is a list + right item is a value, so wrap right as list and start again
                else if (left.Items[i] is PList lL1 && right.Items[i] is PValue rV1)
                {
                    var wrappedList = new PList();
                    wrappedList.Items.Add(rV1);

                    var compareSubList = CompareLists(lL1, wrappedList);
                    if (compareSubList != 0) return compareSubList; // there is a decision, return it
                }
                // left item is a value + right is a list, so wrap left as list and start again
                else if (left.Items[i] is PValue lV1 && right.Items[i] is PList rL1)
                {
                    var wrappedList = new PList();
                    wrappedList.Items.Add(lV1);

                    var compareSubList = CompareLists(wrappedList, rL1);
                    if (compareSubList != 0) return compareSubList; // there is a decision, return it
                }
            }

            return 0;
        }

        static PList ParseList(string package)
        {
            var tk = rxPackageTokens.Match(package);
            var root = new PList();
            var cList = root;
            while(tk.Success)
            {
                if(IsListStart(tk))
                {
                    var sublist = new PList { Parent = cList };
                    cList.Items.Add(sublist);
                    cList = sublist;
                } else if(IsListEnd(tk))
                {
                    cList = cList.Parent!;
                } else if(IsValue(tk, out var v))
                {
                    var item = new PValue { Parent = cList, Value = v };
                    cList.Items.Add(item);
                }

                tk = tk.NextMatch();
            }

            var rootList = root.Items.OfType<PList>().Single();
            rootList.Parent = null;
            return rootList;
        }

        static bool IsValue(Match m, out int value)
        {
            if(m.Success && m.Groups["Value"].Success) 
            {
                value = int.Parse(m.Groups["Value"].Value);
                return true;
            }

            value = 0; return false;
        }
        static bool IsListStart(Match m)
        {
            return m.Success && m.Groups["ListStart"].Success;
        }
        static bool IsListEnd(Match m)
        {
            return m.Success && m.Groups["ListEnd"].Success;
        }
    }
}
