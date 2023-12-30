using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Dynamic;
using System.Numerics;
using System.Text.RegularExpressions;

namespace Day16
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
                var day = 16;
                data = await client.GetStringAsync($"https://adventofcode.com/2023/day/{day}/input");
                File.WriteAllText($"C:\\Source\\Aoc\\2023\\data{day}.txt", data);
            }
            else
            {
                Console.WriteLine("## RUNNING TEST DATA");
                data = testData.Substring(2).Replace("\r", "");
            }

            var grid = data.Split("\n").Select(x => x.ToArray()).Where(x => x.Length > 0).ToArray();
            var sum = 0;

            var energized = new Dictionary<string, int>();
            var visited = new Dictionary<string, bool>();
            //PrintGrid(grid, energized);
            var stack = new Stack<Beam>();
            stack.Push(new Beam(new Vector(0, -1), new Vector(0, 1)));
            while (stack.Any())
            {
                var currBeam = stack.Pop();
                var newBeams = TraceBeam(currBeam, grid, energized);
                newBeams.Where(x => !visited.ContainsKey(GetKey(x)))
                    .ToList().ForEach(x => stack.Push(x));
                newBeams.ForEach(x => visited[GetKey(x)] = true);
            }
            //PrintGrid(grid, energized);

            Console.WriteLine($"Part 1: {energized.Count(x => x.Value > 0)}");

            var results = new List<int>();

            for (var row = 0; row < grid.Length; row++)
            {
                results.Add(CountEnergized(grid, new Beam(new Vector(row, -1), new Vector(0, 1))));
                results.Add(CountEnergized(grid, new Beam(new Vector(row, grid[0].Length), new Vector(0, -1))));
            }

            for (var col = 0; col < grid[0].Length; col++)
            {
                results.Add(CountEnergized(grid, new Beam(new Vector(-1, col), new Vector(1, 0))));
                results.Add(CountEnergized(grid, new Beam(new Vector(grid.Length, col), new Vector(-1, 0))));
            }

            Console.WriteLine($"Part 2: {results.Max()}");

        }

        static int CountEnergized(char[][] grid, Beam start)
        {
            var energized = new Dictionary<string, int>();
            var visited = new Dictionary<string, bool>();
            //PrintGrid(grid, energized);
            var stack = new Stack<Beam>();
            stack.Push(start);
            while (stack.Any())
            {
                var currBeam = stack.Pop();
                var newBeams = TraceBeam(currBeam, grid, energized);
                newBeams.Where(x => !visited.ContainsKey(GetKey(x)))
                    .ToList().ForEach(x => stack.Push(x));
                newBeams.ForEach(x => visited[GetKey(x)] = true);
            }

            return energized.Count(x => x.Value > 0);
        }

        static string GetKey(Beam beam)
        {
            return $"{beam.Position.ToString()}|{beam.Direction.ToString()}";
        }

        static List<Beam> TraceBeam(Beam beam, char[][] grid, Dictionary<string, int> energized)
        {
            var newBeams = new List<Beam>();
            var currPos = new Vector(beam.Position);
            var currDir = new Vector(beam.Direction);
            var visited = new Dictionary<string, bool>();

            while (true)
            {
                currPos.Add(currDir);
                if (visited.ContainsKey(GetKey(new Beam(currPos, currDir))))
                    break;
                visited[GetKey(new Beam(currPos, currDir))] = true;
                if (!currPos.IsWithinBounds(grid)) 
                    break;

                switch (grid[currPos.Row][currPos.Col])
                {
                    case '-':
                        if (currDir.Row != 0)
                        {
                            currDir = Directions["E"];
                            var splitPos = new Vector(currPos);
                            //splitPos.Add(Directions["W"]);
                            newBeams.Add(new Beam(splitPos, new Vector(Directions["W"])));
                        }
                        break;
                    case '/':
                        if (currDir.Row != 0)
                        {
                            currDir = new Vector(0, -currDir.Row);
                        }
                        else
                        {
                            currDir = new Vector(-currDir.Col, 0);
                        }
                        break;
                    case '\\':
                        if (currDir.Row != 0)
                        {
                            currDir = new Vector(0, currDir.Row);
                        }
                        else
                        {
                            currDir = new Vector(currDir.Col, 0);
                        }
                        break;
                    case '|':
                        if (currDir.Col != 0)
                        {
                            currDir = Directions["S"];
                            var splitPos = new Vector(currPos);
                            //splitPos.Add(Directions["N"]);
                            newBeams.Add(new Beam(splitPos, new Vector(Directions["N"])));
                        }
                        break;
                    case '.':
                        // Always same direction
                        break;
                }
                //Console.WriteLine($"{grid[currPos.Row][currPos.Col]} Pos: {currPos.Row}, {currPos.Col} Dir: {currDir.Row}, {currDir.Col}");
                IncreaseDict(energized, currPos, 1);
                //Console.Clear();
                //PrintGrid(grid, currPos);
                //Console.ReadLine();
            }

            return newBeams;
        }

        static void IncreaseDict(Dictionary<string, int> dict, Vector key, int value)
        {
            if (dict.ContainsKey(key.ToString()))
            {
                dict[key.ToString()] += value;
            }
            else
            {
                dict[key.ToString()] = value;
            }
        }

        static void PrintGrid(char[][] grid, Vector curr)
        {
            if (!curr.IsWithinBounds(grid)) return;
            Console.WriteLine($"Curr: {grid[curr.Row][curr.Col]}");
            for (var row = 0; row < grid.Length; row++)
            {
                for (var col = 0; col < grid[0].Length; col++)
                {
                    var tmp =new Vector(row, col);
                    if (curr.Equals(tmp))
                    {
                        Console.Write("*");
                    }
                    else
                    {
                        Console.Write(grid[row][col]);
                    }
                }
                Console.WriteLine();
            }
        }

        static void PrintGrid(char[][] grid, Dictionary<string, int> energized)
        {
            for (var row = 0; row < grid.Length; row++)
            {
                for (var col = 0; col < grid[0].Length; col++)
                {
                    var key = new Vector(row, col).ToString();
                    if (energized.ContainsKey(key))
                    {
                        Console.Write(Math.Min(energized[key], 9));
                    }
                    else
                    {
                        Console.Write(grid[row][col]);
                    }
                }
                Console.WriteLine();
            }
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

        public class Beam
        {
            public Beam(Vector position, Vector direction)
            {
                Position = position;
                Direction = direction;
            }

            public Vector Position { get; set; }
            public Vector Direction { get; set; }
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

            public override string ToString()
            {
                return $"({Row},{Col})";
            }

            public bool Equals(Vector v)
            {
                return v.Row == Row && v.Col == Col;
            }
        }

        private static string testData2 =
@"";

        private static string testData =
@"
.|...\....
|.-.\.....
.....|-...
........|.
..........
.........\
..../.\\..
.-.-/..|..
.|....-|.\
..//.|....";
    }
}