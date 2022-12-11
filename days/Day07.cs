using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AoC2022.days
{
    interface FileSystemObject
    {
        string Name { get; }
        IList<FileSystemObject>? Children { get; }
        long Size { get; }
    }
    class FsDir : FileSystemObject
    {
        public string Name { get; set; } = string.Empty;

        public IList<FileSystemObject> Children { get; } = new List<FileSystemObject>();

        public FsDir? Parent { get; set; }

        public long Size => this.Children.Sum(c => c.Size);
    }

    class FsFile : FileSystemObject
    {
        public string Name { get; set; } = string.Empty;

        public IList<FileSystemObject>? Children { get; } = null;

        public FsDir? Parent { get; set; }

        public long Size { get; set; }
    }

    internal class Day07
    {
        static Regex rxCmdCD = new Regex("\\$ cd (?<DirName>[a-zA-Z0-9/.]+)", RegexOptions.CultureInvariant | RegexOptions.Compiled);
        static Regex rxListingDir = new Regex("dir (?<DirName>[a-zA-Z0-9]+)", RegexOptions.CultureInvariant | RegexOptions.Compiled);
        static Regex rxListingFile = new Regex("(?<FileSize>\\d+) (?<FileName>[a-zA-Z0-9.]+)", RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public static void Solve()
        {
            Console.WriteLine("*** 7th December ***");
            Console.WriteLine();

            var root = ReadFS("data\\d7.txt");

            // story part one: find all directories smaller than 100000
            var allDirectoriesWithSize = GetAllDirectories(root).Where(x => x.Size <= 100000).ToList();
            var sumSize = allDirectoriesWithSize.Sum(x => x.Size);
            Console.WriteLine($"Total Dir Size (all dirs <= 100000): {sumSize}");

            // story part two: find smallest directory that would free up enough space for the update
            const long totalSize = 70000000;
            const long necessaryFreeSpace = 30000000;

            var currentlyFreeSpace = totalSize - root.Size;
            var additionallyRequiredSpace = necessaryFreeSpace - currentlyFreeSpace;

            var potentialDirectories = GetAllDirectories(root).Where(x => x.Size >= additionallyRequiredSpace).OrderBy(x => x.Size).ToList();
            Console.WriteLine($"Delete directory '{potentialDirectories[0].Name}' with a size of {potentialDirectories[0].Size}");
        }

        public static IEnumerable<FsDir> GetAllDirectories(FsDir current)
        {
            yield return current;
            foreach(var dir in current.Children.OfType<FsDir>()) 
            {
                foreach (var subDir in GetAllDirectories(dir))
                    yield return subDir;
            }
        }

        public static FsDir ReadFS(string fsFile)
        {
            var allLines = File.ReadAllLines(fsFile);
            var root = new FsDir();
            
            var current = root;
            var commandExecutionOk = false;
            var inListing = false;
            foreach(var line in allLines)
            {
                commandExecutionOk = false;
                Debug.Assert(current != null);

                if (inListing)
                {
                    var dirMatch = rxListingDir.Match(line);
                    var fileMatch = rxListingFile.Match(line);
                    if (dirMatch.Success)
                    {
                        current.Children.Add(new FsDir { Name = dirMatch.Groups["DirName"].Value, Parent = current });
                        continue;
                    } else if(fileMatch.Success)
                    {
                        current.Children.Add(new FsFile
                        {
                            Name = fileMatch.Groups["FileName"].Value,
                            Size = long.Parse(fileMatch.Groups["FileSize"].Value),
                            Parent = current
                        });
                        continue;
                    }
                    inListing = false;
                }
                if (rxCmdCD.IsMatch(line))
                {
                    var dirName = (rxCmdCD.Match(line).Groups["DirName"]).Value;
                    Debug.Assert(!string.IsNullOrEmpty(dirName));
                    if (dirName == "/")
                    {
                        while (current.Parent != null)
                            current = current.Parent;
                    }
                    else if (dirName == "..")
                    {
                        current = current.Parent;
                    }
                    else
                    {
                        current = current.Children.OfType<FsDir>().First(c => c.Name == dirName);
                    }
                    commandExecutionOk = true;
                }
                else if (line == "$ ls")
                {
                    inListing = true;
                    commandExecutionOk = true;
                }

                Debug.Assert(commandExecutionOk);
            }

            return root;
        }
    }
}
