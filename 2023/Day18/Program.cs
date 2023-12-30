using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Day18
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
                var day = 18;
                data = await client.GetStringAsync($"https://adventofcode.com/2023/day/{day}/input");
                File.WriteAllText($"C:\\Source\\Aoc\\2023\\data{day}.txt", data);
            }
            else
            {
                Console.WriteLine("## RUNNING TEST DATA");
                data = testData.Substring(2).Replace("\r", "");
            }

            //Part1(data);

            Part2(data);
        }

        static void Part2(string data)
        {
            var tmp = data.Split("\n").Select(x => x.Split(" ")).Where(x => x.Length == 3)
                .Select(x => new Heading(new Vector(Directions[x[0]]), null, int.Parse(x[1]),
                    Convert.ToInt32(x[2].Replace("(#", "").Replace(")", ""), 16)));
            var plan = tmp.Select(x => new Heading(GetDir(x.Color % 16), null, x.Color / 16, 1));

            var grid = new List<Heading>();
            var currPos = new Vector(0, 0);
            var min = new Vector(0, 0);
            var max = new Vector(0, 0);
            long outline = 0;
            foreach (var heading in plan)
            {
                var move = new Vector(heading.Direction).Mul(heading.Steps);
                grid.Add(new Heading(heading.Direction, new Vector(currPos), heading.Steps, 0));
                currPos.Add(move);
                min.Row = Math.Min(min.Row, currPos.Row);
                min.Col = Math.Min(min.Col, currPos.Col);
                max.Row = Math.Max(max.Row, currPos.Row);
                max.Col = Math.Max(max.Col, currPos.Col);
                outline += heading.Steps;
            }

            Console.WriteLine($"Min/Max: {min}/{max}");

            // Shoelace formula
            double area = 0;
            for (var i = 1; i <= grid.Count(); i++)
                area += grid[i-1].Position.Row * grid[i % grid.Count()].Position.Col 
                    - grid[i%grid.Count()].Position.Row * grid[i-1].Position.Col;
            area /= 2;

            Console.WriteLine($"Area: {area}, Outline: {outline}");
            // Pick's theorem
            Console.WriteLine($"Part 2: {Convert.ToInt64(Math.Abs(area) - 0.5 * outline + 1) + outline}");
            // Without Pick's.
            // - The area does not include right and bottom outline.
            // - The "missing" right + bottom outline includes three "corners". So it is one more than half the outline
            Console.WriteLine($"Part 2, jojje: {Convert.ToInt64(Math.Abs(area) + 0.5 * outline + 1)}");
        }

        static Vector GetDir(int dir)
        {
            switch (dir)
            {
                case 0:
                    return Directions["R"];
                case 1:
                    return Directions["D"];
                case 2:
                    return Directions["L"];
                case 3:
                    return Directions["U"];
            }
            throw new Exception("Invalid dir");
        }

        static void Part1(string data)
        {
            var plan = data.Split("\n").Select(x => x.Split(" ")).Where(x => x.Length == 3)
                .Select(x => new Heading((new Vector(Directions[x[0]])), null, int.Parse(x[1]),
                    Convert.ToInt32(x[2].Replace("(#", "0x").Replace(")", ""), 16)));

            var grid = new Dictionary<string, int>();
            var pos = new Vector(0, 0);
            var min = new Vector(0, 0);
            var max = new Vector(0, 0);
            foreach (var heading in plan)
            {
                for (var i = 0; i < heading.Steps; i++)
                {
                    pos.Add(heading.Direction);
                    min.Row = Math.Min(min.Row, pos.Row);
                    min.Col = Math.Min(min.Col, pos.Col);
                    max.Row = Math.Max(max.Row, pos.Row);
                    max.Col = Math.Max(max.Col, pos.Col);
                    grid[pos.ToString()] = heading.Color;
                }
            }

            FillGrid(grid, min, max);

            //PrintGrid(grid, min, max);

            Console.WriteLine($"Part 1: {(max.Row - min.Row + 1) * (max.Col - min.Col + 1) - grid.Values.Count(x => x == -1)}");

        }

        static void FillGrid(Dictionary<string, int> grid, Vector min, Vector max)
        {
            for (var row = min.Row; row <= max.Row; row++)
            {
                FindNext(grid, min, max, new Vector(row, min.Col));
                FindNext(grid, min, max, new Vector(row, max.Col));
            }
            for (var col = min.Col; col <= max.Col; col++)
            {
                FindNext(grid, min, max, new Vector(min.Row, col));
                FindNext(grid, min, max, new Vector(max.Row, col));
            }
        }

        static void FindNext(Dictionary<string, int> grid, Vector min, Vector max, Vector curr)
        {
            var stack = new Stack<Vector>();
            stack.Push(new Vector(curr));

            while (stack.Any())
            {
                var next = stack.Pop();
                if (!next.IsWithinBounds(min, max))
                    continue;
                if (grid.ContainsKey(next.ToString()))
                    continue;
                grid[next.ToString()] = -1;
                foreach (var dir in Directions)
                {
                    stack.Push(new Vector(next).Add(dir.Value));
                }
            }
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

        public class Cube: Vector
        {
            public int Size { get; set; }
            public Cube(Vector pos, int size) : base(pos)
            {
                Size = size;
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

            public bool IsWithinBounds(Vector min, Vector max)
            {
                return Row >= min.Row && Col >= min.Col && Row <= max.Row && Col <= max.Col;
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
R 6 (#70c710)
D 5 (#0dc571)
L 2 (#5713f0)
D 2 (#d2c081)
R 2 (#59c680)
D 2 (#411b91)
L 5 (#8ceee2)
U 2 (#caa173)
L 1 (#1b58a2)
U 2 (#caa171)
R 2 (#7807d2)
U 3 (#a77fa3)
L 2 (#015232)
U 2 (#7a21e3)";
    }
}