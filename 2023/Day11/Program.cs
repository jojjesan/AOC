using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Numerics;
using System.Security.Cryptography;
using System.Xml;

namespace Day11
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
                data = await client.GetStringAsync("https://adventofcode.com/2023/day/11/input");
            }
            else
            {
                Console.WriteLine("## RUNNING TEST DATA");
                data = testData.Replace("\r", "");
            }

            // Parse indata
            var lines = data.Split("\n").Where(x => x != "").ToList();
            var map = new Dictionary<Point, char>();
            FillMap(map, lines);
            //Console.WriteLine($"{string.Join("\n", lines.ToArray())}");
            //Console.WriteLine($"{string.Join("\n", lines.ToArray())}");
            //Console.WriteLine($"{string.Join("\n", lines.ToArray())}");
            PrintMap(map, lines.Count(), lines[0].Length);

            var galaxies = map.Keys.Where(x => map[x] == '#').ToList();
            var distances = galaxies.SelectMany(x => galaxies.Where(y => !y.Equals(x)).Select(y => CalcDistance(map, x, y, 2)));
            Console.WriteLine("Part 1");
            Console.WriteLine($"Galaxies: {galaxies.Count()}");
            Console.WriteLine($"Duplicator: 2");
            Console.WriteLine($"Distance: {distances.Sum()/2}");

            // Part 2
            //Part2(start, connPoints);
            var distances2 = galaxies.SelectMany(x => galaxies.Where(y => !y.Equals(x)).Select(y => CalcDistance(map, x, y, 1000000)));
            Console.WriteLine();
            Console.WriteLine("Part 2");
            Console.WriteLine($"Galaxies: {galaxies.Count()}");
            Console.WriteLine($"Duplicator: 1 000 000");
            Console.WriteLine($"Distance: {distances2.Sum() / 2}");

        }

        static long CalcDistance(Dictionary<Point, char> map, Point x, Point y, int eStep = 1)
        {
            var dist = 0;
            var diff = y.Subtract(x);
            var dirR = diff.Row > 0 ? 1 : -1;
            var dirC = diff.Col > 0 ? 1 : -1;

            for (var dr = 0; dr < Math.Abs(diff.Row); dr++)
            {
                if (map[new Point(x.Row + dirR*dr , x.Col)] == 'e')
                    dist += eStep;
                else
                    dist++;
            }
            for (var dc = 0; dc < Math.Abs(diff.Col); dc++)
            {
                if (map[new Point(x.Row, x.Col + dirC*dc)] == 'e')
                    dist += eStep;
                else
                    dist++;
            }

            return dist;
        }

        static void FillMap(Dictionary<Point, char> map, List<string> lines)
        {
            for (var r = 0; r < lines.Count(); r++)
            {
                for (var c = 0; c < lines[0].Length; c++)
                {
                    var fillCh = lines[r][c];
                    if (lines.All(x => x[c] == '.') || !lines[r].Contains("#"))
                        fillCh = 'e';
                    map[new Point(r, c)] = fillCh;
                }
            }
        }

        static void Part2(Point start, List<List<string>> connPoints)
        {
        }

        static void FillEmpty(Dictionary<Point, char> map, Point max, char ch)
        {
            for (var r = 0; r < max.Row; r++)
            {
                for (var c = 0; c < max.Col; c++)
                {
                    var p = new Point(r, c);
                    if (!map.ContainsKey(p))
                        map[p] = ch;
                }
            }
        }

        static bool IsWithinBounds(Point pos, Point max)
        {
            return pos.Row >= 0 && pos.Col >= 0 && pos.Row < max.Row && pos.Col < max.Col;
        }

        static void PrintMap(Dictionary<Point, char> map, int rowCount, int colCount)
        {
            for (var r = 0; r < rowCount; r++)
            {
                for (var c = 0; c < colCount; c++)
                {
                    var p = new Point() { Row = r, Col = c };
                    if (map.ContainsKey(p))
                    {
                        Console.Write(map[p]);
                    }
                    else
                    {
                        Console.Write("x");
                    }
                }
                Console.WriteLine();
            }
        }

        static void Part1(Point start, List<List<string>> connPoints)
        {
        }


        static void WriteColored(string text, string coloredWord, ConsoleColor color = ConsoleColor.Green)
        {
            string[] normalParts = text.Split(new string[] { coloredWord }, StringSplitOptions.None);
            for (int i = 0; i < normalParts.Length; i++)
            {
                Console.ResetColor();
                Console.Write(normalParts[i]);
                if (i != normalParts.Length - 1)
                {
                    Console.BackgroundColor = color;
                    Console.Write(coloredWord);
                }
            }
        }

        struct Point
        {
            public int Row;
            public int Col;
            public Point(int r, int c)
            {
                Row = r;
                Col = c;
            }
            public Point Add(Point other)
            {
                return new Point() { Row = Row + other.Row, Col = Col + other.Col };
            }
            public Point Subtract(Point other)
            {
                return new Point() { Row = Row - other.Row, Col = Col - other.Col };
            }
            public int ManhattanDistanceTo(Point other)
            {
                return Math.Abs(Row - other.Row) + Math.Abs(Col - other.Col);
            }
            public bool Equals(Point other)
            {
                return Row == other.Row && Col == other.Col;
            }
            public override string ToString()
            {
                return $"({Row}, {Col})";
            }
        }

        private static string testData2 =
@"";

        private static string testData =
@"...#......
.......#..
#.........
..........
......#...
.#........
.........#
..........
.......#..
#...#.....";
    }
}