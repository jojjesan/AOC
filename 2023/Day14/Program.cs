using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Numerics;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NumSharp;

namespace Day14
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            bool test = false;
            string data = "";
            if (!test)
            {
                string sessionKey = File.ReadAllText("C:\\Source\\Aoc\\sessionkey.txt");
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Cookie", $"session={sessionKey}");
                var day = 14;
                data = await client.GetStringAsync($"https://adventofcode.com/2023/day/{day}/input");
                File.WriteAllText($"C:\\Source\\Aoc\\2023\\data{day}.txt", data);
            }
            else
            {
                Console.WriteLine("## RUNNING TEST DATA");
                data = testData.Substring(2).Replace("\r", "");
            }

            var grid = data.Split("\n").Select(x => x.ToArray()).Where(x => x.Length > 0).ToArray();
            var grid2 = data.Split("\n").Select(x => x.ToArray()).Where(x => x.Length > 0).ToArray();

            //PrintGrid(grid);
            //while (RollStones(grid, Directions["N"]) > 0) { }
            //Console.WriteLine();
            //PrintGrid(grid);

            //Console.WriteLine($"Part 1: {ScoreStones(grid)}");

            PrintGrid(grid2);
            var states = new Dictionary<int, int>();
            var ix = 0;
            var loops = 1000000000;
            var jumped = false;
            while (ix < loops)
            {
                while (RollStones(grid2, Directions["N"]) > 0) { }
                while (RollStones(grid2, Directions["W"]) > 0) { }
                while (RollStones(grid2, Directions["S"]) > 0) { }
                while (RollStones(grid2, Directions["E"]) > 0) { }

                var state = GetGridState(grid2);
                if (states.ContainsKey(state) && !jumped)
                {
                    var jumpStep = ix - states[state];
                    if (jumpStep == 0) break;
                    var ixStep = (loops - ix) / jumpStep - 1;
                    Console.WriteLine($"ix: {ix}, interval: {jumpStep}, ix inc: {ixStep * jumpStep}");
                    ix += ixStep * jumpStep + 1;
                    jumped = true;
                }
                else
                {
                    states[state] = ix;
                    ix++;
                }
                Console.WriteLine($"score({ix}, {jumped}, {ScoreStones(grid2)}, {state}): ");
            }
            Console.WriteLine();
            PrintGrid(grid2);

            Console.WriteLine($"Part 2: {ScoreStones(grid2)}");
        }

        static int GetGridState(char[][] grid)
        {
            var state = string.Join("", grid.Select(x => string.Join("", x)));
            return state.GetHashCode();
        }

        static void PrintGrid(char[][] grid)
        {
            Console.WriteLine(string.Join("\n", grid.Select(x => string.Join("", x))));
        }

        static int RollStones(char[][] grid, Vector rollDir)
        {
            var innerDir = new Vector(1 - Math.Abs(rollDir.Row), 1 - Math.Abs(rollDir.Col));
            var outerDir = new Vector(-rollDir.Row, -rollDir.Col);
            var startPos = new Vector(
                Math.Max(0, rollDir.Row * (grid.Length-1)), 
                Math.Max(0, rollDir.Col * (grid[0].Length - 1)));

            var count = 0;
            for (var outer = new Vector(startPos); outer.IsWithinBounds(grid); outer.Add(outerDir))
            {
                for (var source = new Vector(outer); source.IsWithinBounds(grid); source.Add(innerDir))
                {
                    var target = new Vector(source);
                    target.Add(rollDir);
                    if (target.IsWithinBounds(grid) && grid[target.Row][target.Col] == '.' && grid[source.Row][source.Col] == 'O')
                    {
                        grid[target.Row][target.Col] = 'O';
                        grid[source.Row][source.Col] = '.';
                        count++;
                        Assert.AreEqual(grid[target.Row][target.Col], 'O');
                    }
                }
            }
            return count;
        }

        static int ScoreStones(char[][] grid)
        {
            var sum = 0;
            for (var row = 0; row < grid.Length; row++)
            {
                for (var col = 0; col < grid[0].Length; col++)
                {
                    if (grid[row][col] == 'O')
                    {
                        sum += grid.Length - row;
                    }
                }
            }

            return sum;
        }

        static Dictionary<string, Vector> Directions = new Dictionary<string, Vector>()
        {
            {"N", new Vector(-1, 0)},
            {"S", new Vector(1, 0)},
            { "E", new Vector(0, 1)},
            { "W", new Vector(0, -1)}
        };

        static int FindColReflection(NDArray orig, int unmatched = 0)
        {
            for (var col = 1; col < orig.Shape[1]; col++)
            {
                var len = Math.Min(col, orig.Shape[1] - col);
                var first = orig[$":,{col - len}:{col}"];
                var second = orig[$":,{col}:{col + len}"];
                var secondFlippedCols = second[":,::-1"];
                var s = (first == secondFlippedCols);
                if (s.flatten().ToArray<bool>().Where(x => !x).Count() == unmatched)
                {
                    return col;
                }
            }
            return 0;
        }

        static int FindRowReflection(NDArray orig, int unmatched = 0)
        {
            for (var row = 1; row < orig.Shape[0]; row++)
            {
                var len = Math.Min(row, orig.Shape[0] - row);
                var first = orig[$"{row - len}:{row},:"];
                var second = orig[$"{row}:{row + len},:"];
                var secondFlippedRows = second["::-1"];
                var s = (first == secondFlippedRows);
                if (s.flatten().ToArray<bool>().Where(x => !x).Count() == unmatched)
                {
                    return row;
                }
            }

            return 0;
        }

        public class Vector
        {
            public int Row { get; set; }
            public int Col { get; set; }

            public Vector(int rows, int cols)
            {
                Row = rows;
                Col = cols;
            }

            public Vector(Vector v)
            {
                Row = v.Row;
                Col = v.Col; 
            }

            public Vector Add(Vector v)
            {
                Row += v.Row;
                Col += v.Col;
                return this;
            }

            public bool IsWithinBounds(char[][] grid)
            {
                return Row >= 0 && Col >= 0 && Row < grid.Length && Col < grid[0].Length;
            }
        }

        private static string testData2 =
@"";

        private static string testData =
@"
O....#....
O.OO#....#
.....##...
OO.#O....O
.O.....O#.
O.#..O.#.#
..O..#O..O
.......O..
#....###..
#OO..#....";
    }
}