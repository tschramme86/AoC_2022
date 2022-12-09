using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AoC2022.days
{
    internal class Day08
    {
        public static void Solve()
        {
            Console.WriteLine("*** 8th December ***");
            Console.WriteLine();

            // testdata
            {
                var treeGrid = ReadTreeGrid("data/d8-test.txt");

                // Story part one - visibility
                var visibilityGrid = CreateVisibilityGrid(treeGrid);
                var visibleTrees = visibilityGrid.Sum(row => row.Count(c => c));
                Debug.Assert(visibleTrees == 21);

                Console.WriteLine($"Total visible trees (test): {visibleTrees}");

                // Story part two - scenic score
                var scenicGrid = CreateScenicGrid(treeGrid);
                var maxScenicScore = scenicGrid.Max(row => row.Max(x => x));
                Debug.Assert(maxScenicScore == 8);

                Console.WriteLine($"Max possible scenic score (test): {maxScenicScore}");
            }
            // real data
            {
                var treeGrid = ReadTreeGrid("data/d8.txt");
                
                // Story part one - visibility
                var visibilityGrid = CreateVisibilityGrid(treeGrid);
                var visibleTrees = visibilityGrid.Sum(row => row.Count(c => c));

                Console.WriteLine($"Total visible trees: {visibleTrees}");

                var scenicGrid = CreateScenicGrid(treeGrid);
                var maxScenicScore = scenicGrid.Max(row => row.Max(x => x));

                Console.WriteLine($"Max possible scenic score (test): {maxScenicScore}");
            }
        }

        static int[][] ReadTreeGrid(string file)
        {
            var allLines = File.ReadAllLines(file);
            var grid = new int[allLines.Length][];
            var expectedWidth = allLines[0].Length;
            for(var y=0; y<allLines.Length; y++)
            {
                Debug.Assert(allLines[y].Length == expectedWidth);

                grid[y] = new int[allLines[y].Length];
                for(var x=0; x < allLines[y].Length; x++)
                {
                    grid[y][x] = allLines[y][x] - '0';
                }
            }

            return grid;
        }

        static bool[][] CreateVisibilityGrid(int[][] treeGrid)
        {
            var visGrid = new bool[treeGrid.Length][];
            for (var y = 0; y < treeGrid.Length; y++) visGrid[y] = new bool[treeGrid[y].Length];

            var width = treeGrid[0].Length;
            var height = treeGrid.Length;

            // all edge-trees are visible
            for (var x = 0; x < width; x++)
            {
                visGrid[0][x] = true;
                visGrid[height - 1][x] = true;
            }
            for (var y = 0; y < height; y++)
            {
                visGrid[y][0] = true;
                visGrid[y][height - 1] = true;
            }

            // check for the inner trees
            for(var y=1; y<height-1; y++) 
            {
                for(var x=1; x<width - 1; x++) 
                {
                    var h = treeGrid[y][x];
                    
                    // look from tree to the left
                    var edgeVisible = true;
                    for(var cx=0; cx < x; cx++) if (treeGrid[y][cx] >= h) edgeVisible = false;

                    // look from tree to the right
                    if(!edgeVisible)
                    {
                        edgeVisible = true;
                        for (var cx = x+1; cx < width; cx++) if (treeGrid[y][cx] >= h) edgeVisible = false;
                    }

                    // look from tree to the top
                    if(!edgeVisible)
                    {
                        edgeVisible = true;
                        for (var cy = 0; cy < y; cy++) if (treeGrid[cy][x] >= h) edgeVisible = false;
                    }

                    // look from tree to the bottom
                    if(!edgeVisible)
                    {
                        edgeVisible = true;
                        for (var cy = y + 1; cy < height; cy++) if (treeGrid[cy][x] >= h) edgeVisible = false;
                    }

                    visGrid[y][x] = edgeVisible;
                }
            }

            return visGrid;
        }

        static int[][] CreateScenicGrid(int[][] treeGrid)
        {
            var width = treeGrid[0].Length;
            var height = treeGrid.Length;

            var scenGrid = new int[height][];
            for (var y = 0; y < treeGrid.Length; y++) scenGrid[y] = new int[width];

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var h = treeGrid[y][x];
                    int upScore = 0, downScore = 0, leftScore = 0, rightScore = 0;
                    for (; y - upScore > 0; upScore++) if (upScore > 0 && treeGrid[y - upScore][x] >= h) break;
                    for (; y + downScore < height - 1; downScore++) if (downScore > 0 && treeGrid[y + downScore][x] >= h) break;
                    for (; x - leftScore > 0; leftScore++) if (leftScore > 0 && treeGrid[y][x - leftScore] >= h) break;
                    for (; x + rightScore < width - 1; rightScore++) if (rightScore > 0 && treeGrid[y][x + rightScore] >= h) break;

                    scenGrid[y][x] = upScore * downScore * leftScore * rightScore;
                }
            }

            return scenGrid;
        }
    }
}
