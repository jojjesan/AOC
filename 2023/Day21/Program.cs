using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Day21
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
                var day = 21;
                data = await client.GetStringAsync($"https://adventofcode.com/2023/day/{day}/input");
                File.WriteAllText($"C:\\Source\\Aoc\\2023\\data{day}.txt", data);
            }
            else
            {
                Console.WriteLine("## RUNNING TEST DATA");
                data = testData.Substring(2).Replace("\r", "");
            }

            //Part2(data, 64); // 3773
            //Part2(data, 65); // 3874
            //Part2(data, 196); // 34549
            //Part2(data, 327); // 95798
            Console.WriteLine($"Part2: {SolvePart2()}");
            Part1(data);
        }

        static long SolvePart2()
        {
            // Find coefficients for the quadratic equation from three points
            var plotCounts = new List<int>() { 3874, 34549, 95798 };
            var steps = 26501365;
            var gridSize = 131;
            long n = steps / gridSize;

            long b0 = plotCounts[0];
            long b1 = plotCounts[1] - plotCounts[0];
            long b2 = plotCounts[2] - plotCounts[1];

            // Caclculate value for given n
            return b0 + b1 * n + (n * (n - 1)/2)*(b2-b1);

        }

        // Use this one only to get three valid points
        static int Part2(string data, int steps)
        {
            var grid = data.Split("\n").Select(x => x.ToArray()).Where(x => x.Length > 0).ToArray();
            var rows = grid.Length;
            var cols = grid[0].Length;
            var start = grid.Select((x, i) => new { Row = i, Col = Array.IndexOf(x, 'S') }).Where(x => x.Col != -1)
                .Select(x => new Cube(x.Row, x.Col, 0)).First();
            grid[start.Row][start.Col] = '.';
            var stack = new Stack<Cube>();
            var sum = 0;
            stack.Push(start);
            var goals = new Dictionary<string, bool>();
            var oddStates = new Dictionary<string, int>();
            var evenStates = new Dictionary<string, int>();
            var ix = 0;
            while (stack.Any())
            {
                var pos = stack.Pop();
                SetState(oddStates, evenStates, pos);
                foreach (var d in Directions.Values)
                {
                    var newPos = new Cube(pos, pos.Size + 1).Add(d) as Cube;
                    var newPosCurrState = GetState(oddStates, evenStates, newPos);
                    if (grid[((newPos.Row % rows) + rows) % rows][((newPos.Col % cols) + cols) % cols] == '.')
                    {
                        if (newPos.Size == steps)
                        {
                            goals[newPos.Key()] = true;
                            SetState(oddStates, evenStates, newPos);
                        }
                        else if (newPosCurrState == -1 || newPos.Size < newPosCurrState)
                        {
                            stack.Push(newPos);
                        }

                        if ((steps - newPos.Size) % 2 == 0)
                            goals[newPos.Key()] = true;
                    }
                }
                ix++;
                if (ix % 100000 == 0)
                {
                    Console.Write($"\rq:{stack.Count()} g:{goals.Count()} v:{evenStates.Count()}");
                }
            }

            Console.WriteLine();
            Console.WriteLine($"Part2: {goals.Count()}");
            return goals.Count();
        }

        static public void SetState(Dictionary<string, int> oddStates, Dictionary<string, int> evenStates, Cube pos)
        {
            if (pos.Size % 2 == 1)
                oddStates[pos.Key()] = pos.Size;
            else
                evenStates[pos.Key()] = pos.Size;
        }

        static public int GetState(Dictionary<string, int> oddStates, Dictionary<string, int> evenStates, Cube pos)
        {
            if (pos.Size % 2 == 1)
            {
                if (oddStates.ContainsKey(pos.Key()))
                    return oddStates[pos.Key()];
            }
            else
            {
                if (evenStates.ContainsKey(pos.Key()))
                    return evenStates[pos.Key()];
            }
            return -1;
        }

        static Dictionary<string, int> Part1(string data)
        {
            var grid = data.Split("\n").Select(x => x.ToArray()).Where(x => x.Length > 0).ToArray();
            var start = grid.Select((x, i) => new { Row = i, Col = Array.IndexOf(x, 'S') }).Where(x => x.Col != -1)
                .Select(x => new Cube(x.Row, x.Col, 0)).First();
            grid[start.Row][start.Col] = '.';
            var stack = new Stack<Cube>();
            var sum = 0;
            stack.Push(start);
            var shortest = new Dictionary<string, int>();
            var ix = 0;
            while (stack.Any())
            {
                var pos = stack.Pop();
                if (shortest.ContainsKey(pos.Key()))
                    shortest[pos.Key()] = Math.Min(shortest[pos.Key()], pos.Size);
                else
                    shortest[pos.Key()] = pos.Size;

                foreach (var d in Directions.Values)
                {
                    var newPos = new Cube(pos, pos.Size + 1).Add(d) as Cube;
                    if (newPos.IsWithinBounds(grid) && grid[newPos.Row][newPos.Col] == '.')
                    {
                        if (!shortest.ContainsKey(newPos.Key()) || shortest[newPos.Key()] > newPos.Size)
                        stack.Push(newPos);
                    }
                }
                ix++;
                if (ix % 100000 == 0)
                {
                    Console.Write($"\rq:{stack.Count()} v:{shortest.Count()}");
                }
            }

            // https://github.com/villuna/aoc23/wiki/A-Geometric-solution-to-advent-of-code-2023,-day-21
            Console.WriteLine();
            Console.WriteLine($"Part1 alt: {shortest.Values.Count(x => x % 2 == 0 && x <= 64)}"); // 3773
            long oddSquares = shortest.Values.Count(x => x % 2 == 1);
            Console.WriteLine($"Odd squares: {oddSquares}"); // 7637
            long evenSquares = shortest.Values.Count(x => x % 2 == 0);
            Console.WriteLine($"Even squares: {evenSquares}");  // 7650
            long oddSquaresOver65 = shortest.Values.Count(x => x % 2 == 1 && x > 65);
            Console.WriteLine($"Odd squares > 65: {oddSquaresOver65}"); // 3763
            long evenSquaresOver65 = shortest.Values.Count(x => x % 2 == 0 && x > 65);
            Console.WriteLine($"Even squares > 65: {evenSquaresOver65}");  // 3877

            long n = 202300;
            long part2 = ((n + 1) * (n + 1)) * oddSquares + (n * n) * evenSquares - (n + 1) * oddSquaresOver65 + n * evenSquaresOver65;
            Console.WriteLine($"Part2: {part2}"); // 1610054434 too low

            return shortest;
        }

        static void PrintGrid(Dictionary<string, int> grid, Vector min, Vector max)
        {
            for (var row = min.Row; row <= max.Row; row++)
            {
                for (var col = min.Col; col <= max.Col; col++)
                {
                    var tmp = new Vector(row, col);
                    if (grid.ContainsKey(tmp.ToString()) && grid[tmp.ToString()] == -1)
                    {
                        Console.Write(".");
                    }
                    else
                    {
                        Console.Write("#");
                    }
                }
                Console.WriteLine();
            }
        }

        static Dictionary<string, Vector> Directions = new Dictionary<string, Vector>()
        {
            { "U", new Vector(-1, 0)},
            { "D", new Vector(1, 0)},
            { "R", new Vector(0, 1)},
            { "L", new Vector(0, -1)}
        };

        public class Heading
        {
            public Heading(Vector direction, Vector pos, int steps, int color)
            {
                Direction = direction;
                Position = pos;
                Color = color;
                Steps = steps;
            }

            public Vector Direction { get; set; }
            public Vector Position { get; set; }
            public int Color { get; set; }
            public int Steps { get; set; }
        }

        public class Cube : Vector
        {
            public int Size { get; set; }
            public Cube(Vector pos, int size) : base(pos)
            {
                Size = size;
            }
            public Cube(int rows, int cols, int size) : base(rows, cols)
            {
                Size = size;
            }
            public override string ToString()
            {
                return $"({Row},{Col},{Size})";
            }
            public string Key()
            {
                return $"({Row},{Col})";
            }

        }

        public class Vector
        {
            public long Row { get; set; }
            public long Col { get; set; }

            public Vector(long rows, long cols)
            {
                Row = rows;
                Col = cols;
            }

            public Vector(Vector v)
            {
                Row = v.Row;
                Col = v.Col;
            }

            public long GetManhLen()
            {
                return Math.Abs(Row) + Math.Abs(Col);
            }

            public Vector Sub(Vector v)
            {
                Row -= v.Row;
                Col -= v.Col;
                return this;
            }

            public Vector Add(Vector v)
            {
                Row += v.Row;
                Col += v.Col;
                return this;
            }

            public Vector Mul(int mul)
            {
                Row *= mul;
                Col *= mul;
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
...........
.....###.#.
.###.##..#.
..#.#...#..
....#.#....
.##..S####.
.##..#...#.
.......##..
.##.#.####.
.##..##.##.
...........
";
    }
}