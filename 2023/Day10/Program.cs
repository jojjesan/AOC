using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using System.Security.Cryptography;
using System.Xml;

namespace Day10
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
                data = await client.GetStringAsync("https://adventofcode.com/2023/day/10/input");
            }
            else
            {
                Console.WriteLine("## RUNNING TEST DATA");
                data = testData2.Replace("\r", "");
            }

            // Parse indata
            var lines = data.Split("\n").Where(x => x != "").ToArray();
            var connPoints = lines.Select(x => x.ToCharArray().Select(GetConnectingPoints).ToList()).ToList();

            var start = new Point() { Row = 0, Col = 0 };
            var ix = 0;
            foreach (var connPointRow in connPoints)
            {
                //WriteColored(string.Join("", connPointRow), "XX");
                //Console.WriteLine();
                if (connPointRow.Contains("NSEW")) {
                    start.Row = ix;
                    start.Col = connPointRow.IndexOf("NSEW");
                };
                ix++;
            }

            //Part1(start, connPoints);

            // Part 2
            Part2(start, connPoints);

        }

        static void Part2(Point start, List<List<string>> connPoints)
        {
            var forward = GetNextPoint(start, start, connPoints);
            var startDirection = GetDirection(forward.Subtract(start));
            var steps = 1;

            Console.WriteLine($"Start: {start}");
            Console.WriteLine($"Forward: {forward}");

            var map = new Dictionary<Point, char>();
            var prevForward = start;
            while (!forward.Equals(start))
            {
                AddImgToMap(map, forward, connPoints[forward.Row][forward.Col]);
                var savedForward = forward;
                forward = GetNextPoint(forward, prevForward, connPoints);
                prevForward = savedForward;
                //Console.WriteLine($"Curr forward: {forward}");
                steps++;
            }
            var endDirection = GetDirection(forward.Subtract(prevForward));
            AddImgToMap(map, forward, endDirection.Substring(1, 1) + startDirection.Substring(0, 1));

            var maxMap = new Point() { Row = connPoints.Count() * 3, Col = connPoints[0].Count() * 3 };
            var max = new Point() { Row = connPoints.Count(), Col = connPoints[0].Count() };

            PrintMap(map, maxMap.Row, maxMap.Col);
            Console.WriteLine();

            FillOuter(map, maxMap, 'o');
            FillEmpty(map, maxMap, 'i');
            PrintMap(map, maxMap.Row, maxMap.Col);

            //Console.WriteLine($"Part 2: {map[new Point() { Row=0, Col=0}]}");
            Console.WriteLine($"Part 2 mapped: {map.Values.Count()}");
            Console.WriteLine($"Part 2 #: {maxMap.Row * maxMap.Row}");
            Console.WriteLine($"{max} {maxMap}");
            Console.WriteLine($"Part 2 result: {CountFullSquares(map, 'i', max)}");
        }

        static int CountFullSquares(Dictionary<Point, char> map, char ch, Point max)
        {
            var count = 0;
            for (var r = 0; r < max.Row; r++)
            {
                for (var c = 0; c < max.Col; c++)
                {
                    var found = true;
                    for (var dr = 0; dr < 3; dr++)
                    {
                        for (var dc = 0; dc < 3; dc++)
                        {
                            if (map[new Point(r * 3 + dr, c * 3 + dc)] != ch)
                            {
                                found = false;
                            }
                        }
                    }

                    if (found) count++;
                }
            }
            return count;
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

        static void FillOuter(Dictionary<Point, char> map, Point max,char ch)
        {
            for (var r = 0; r < max.Row; r++)
            {
                FillAdjacent(map, new Point(r, 0), max, ch);
                FillAdjacent(map, new Point(r, max.Col - 1), max, ch);
            }
            for (var c = 0; c < max.Col; c++)
            {
                FillAdjacent(map, new Point(0, c), max, ch);
                FillAdjacent(map, new Point(max.Row - 1, c), max, ch);
            }
        }

        static void FillAdjacent(Dictionary<Point, char> map, Point pos, Point max, char ch)
        {
            var directions = new Point[]
            {
                new Point(0, 1), new Point(0, -1), new Point(1, 0), new Point(-1, 0)
                //new Point(1, 1), new Point(1, -1), new Point(-1, 1), new Point(-1, 1)
            };
            if (!IsWithinBounds(pos, max) || map.ContainsKey(pos))
                return;

            var stack = new Stack<Point>();
            stack.Push(pos);

            while (stack.Any())
            {
                var p = stack.Pop();
                map[p] = ch;
                foreach (var direction in directions)
                {
                    var newPos = p.Add(direction);
                    if (!map.ContainsKey(newPos) && IsWithinBounds(newPos, max))
                        stack.Push(newPos);
                }
            }
        }

        static bool IsWithinBounds(Point pos, Point max)
        {
            return pos.Row >= 0 && pos.Col >= 0 && pos.Row < max.Row && pos.Col < max.Col;
        }

        static void AddImgToMap(Dictionary<Point, char> map, Point pos, string connType)
        {
            var img = GetImg(connType);
            for (var r = 0; r < img.Length; r++)
            {
                for (var c = 0; c < img[r].Length; c++)
                {
                    if (img[r][c] != '.')
                        map[new Point(pos.Row*3 + r, pos.Col*3 + c)] = img[r][c];
                }
            }
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
                        Console.Write(".");
                    }
                }
                Console.WriteLine();
            }
        }

        static void Part1(Point start, List<List<string>> connPoints)
        {
            var forward = GetNextPoint(start, start, connPoints);
            var reverse = GetNextPoint(start, start, connPoints, true);
            var steps = 1;

            Console.WriteLine($"Start: {start}");
            Console.WriteLine($"Forward: {forward}");
            Console.WriteLine($"Reverse: {reverse}");

            var prevForward = start;
            var prevReverse = start;
            while (!forward.Equals(reverse))
            {
                var savedForward = forward;
                var savedReverse = reverse;
                forward = GetNextPoint(forward, prevForward, connPoints);
                reverse = GetNextPoint(reverse, prevReverse, connPoints);
                prevForward = savedForward;
                prevReverse = savedReverse;
                Console.WriteLine($"Curr forward: {forward}");
                Console.WriteLine($"Curr reverse: {reverse}");
                steps++;
            }

            Console.WriteLine($"Part 1: {steps}");
        }

        static Point GetNextPoint(Point current, Point previous, List<List<string>> connPoints, bool reverse = false)
        {
            var directions = new Point[] {
                new Point() { Row = 0, Col = 1},
                new Point() { Row = 0, Col = -1},
                new Point() { Row = 1, Col = 0},
                new Point() { Row = -1, Col = 0}
            };
            if (reverse)
            {
                directions = directions.Reverse().ToArray();
            }

            foreach (var direction in directions)
            {
                if (current.Add(direction).Equals(previous)) continue;
                if (IsConnected(current, current.Add(direction), connPoints)) 
                    return current.Add(direction);
            }

            throw new Exception("No next point found");
        }

        static bool IsConnected(Point from, Point to, List<List<string>> connPoints)
        {
            if (to.Row < 0 || to.Row >= connPoints.Count()) return false;
            if (to.Col < 0 || to.Col >= connPoints[to.Row].Count()) return false;

            var fromConn = connPoints[from.Row][from.Col];
            var toConn = connPoints[to.Row][to.Col];
            var direction = GetDirection(to.Subtract(from));

            return fromConn.Contains(direction[0]) && toConn.Contains(direction[1]);
        }

        static string GetDirection(Point direction)
        {
            switch (direction.Row, direction.Col)
            {
                case (0, 1):
                    return "EW";
                case (0, -1):
                    return "WE";
                case (1, 0):
                    return "SN";
                case (-1, 0):
                    return "NS";
                default:
                    throw new Exception("Unknown direction");
            }
        }

        static string GetConnectingPoints(char input)
        {
            switch (input)
            {
                case '|':
                    return "NS";
                case '-':
                    return "EW";
                case 'L':
                    return "NE";
                case 'J':
                    return "NW";
                case '7':
                    return "SW";
                case 'F':
                    return "SE";
                case 'S':
                    return "NSEW";
                case '.':
                    return "..";
                default:
                    throw new Exception("Unknown input");
            }
        }

        static char[][] GetImg(string connType)
        {
            switch (connType)
            {
                case "NS":
                    return new char[][] { 
                        new char[] { '.', '|', '.'},
                        new char[] { '.', '|', '.'},
                        new char[] { '.', '|', '.'}
                    };
                case "EW":
                    return new char[][] {
                        new char[] { '.', '.', '.'},
                        new char[] { '-', '-', '-'},
                        new char[] { '.', '.', '.'}
                    };
                case "NE":
                    return new char[][] {
                        new char[] { '.', '|', '.'},
                        new char[] { '.', '*', '-'},
                        new char[] { '.', '.', '.'}
                    };
                case "NW":
                    return new char[][] {
                        new char[] { '.', '|', '.'},
                        new char[] { '-', '*', '.'},
                        new char[] { '.', '.', '.'}
                    };
                case "SW":
                    return new char[][] {
                        new char[] { '.', '.', '.'},
                        new char[] { '-', '*', '.'},
                        new char[] { '.', '|', '.'}
                    };
                case "SE":
                    return new char[][] {
                        new char[] { '.', '.', '.'},
                        new char[] { '.', '*', '-'},
                        new char[] { '.', '|', '.'}
                    };
                default:
                    throw new Exception("Unknown input");
            }
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
@"FF7FSF7F7F7F7F7F---7
L|LJ||||||||||||F--J
FL-7LJLJ||||||LJL-77
F--JF--7||LJLJ7F7FJ-
L---JF-JLJ.||-FJLJJ7
|F|F-JF---7F7-L7L|7|
|FFJF7L7F-JF7|JL---7
7-L-JL7||F7|L7F-7F7|
L.L7LFJ|||||FJL7||LJ
L7JLJL-JLJLJL--JLJ.L";

        private static string testData = 
@"7-F7-
.FJ|7
SJLL7
|F--J
LJ.LJ";
    }
}