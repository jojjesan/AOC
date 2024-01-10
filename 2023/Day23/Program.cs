using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using static Day23.Program;

namespace Day23
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
                var day = 23;
                data = await client.GetStringAsync($"https://adventofcode.com/2023/day/{day}/input");
                File.WriteAllText($"C:\\Source\\Aoc\\2023\\data{day}.txt", data);
            }
            else
            {
                Console.WriteLine("## RUNNING TEST DATA");
                data = testData.Substring(2).Replace("\r", "");
            }

            //Part(1, data);
            Part(2, data.Replace("<", ".").Replace(">", ".").Replace("^", ".").Replace("v", "."));
        }

        static void Part(int part, string data)
        {
            var (startJ, goalJ) = BuildGraph(data);

            var res = new List<Explorer>();
            var stack = new Stack<Explorer>();
            stack.Push(new Explorer { PositionJ = startJ, Steps = 0, Visited = new HashSet<(int, int)>(new (int, int)[] { startJ.Pos }) });
            while (stack.Any())
            {
                var currExp = stack.Pop();
                foreach (var currPath in currExp.PositionJ.Paths.Values)
                {
                    if (currExp.Visited.Contains(currPath.EndJ.Pos))
                        continue;
                    var nextExp = new Explorer(currExp);
                    nextExp.PositionJ = currPath.EndJ;
                    nextExp.Steps += currPath.Length;
                    nextExp.Visited.Add(currPath.EndJ.Pos);
                    if (nextExp.PositionJ.Pos.Equals(goalJ.Pos))
                    {
                        res.Add(nextExp);
                    }
                    else
                    {
                        stack.Push(nextExp);
                    }
                }
            }

            //Console.WriteLine($"{string.Join(", ", res.ToArray().Select(x => $"{x.Steps}"))}");
            Console.WriteLine($"Part{part}: {res.Max(x => x.Steps)}");
        }

        static (Junction, Junction) BuildGraph(string data)
        {
            var map = data.Split("\n").Where(x => x.Length > 0)
                .Select(x => x.ToArray()).ToArray();
            var startCol = map[0].Select((x, i) => new { x, i }).First(y => y.x == '.').i;
            var endCol = map[map.Length - 1].Select((x, i) => new { x, i }).First(y => y.x == '.').i;

            var visited = new HashSet<string>();

            var startJ = new Junction { Pos = (0, startCol) };
            var goalJ = new Junction { Pos = (map.Length - 1, endCol) };
            var currJ = startJ;

            var stack = new Stack<((int, int), (int, int), Junction)>();
            stack.Push((GetNeighbours(startJ.Pos, startJ.Pos, map, Dirs).First(), startJ.Pos, startJ));
            var junctions = new Dictionary<(int, int), Junction>();
            junctions[goalJ.Pos] = goalJ;

            while (stack.Any())
            {
                var oneWay = false;
                var fromStack = stack.Pop();
                var pos = (fromStack.Item1, fromStack.Item2);
                currJ = fromStack.Item3;
                junctions[currJ.Pos] = currJ;
                var len = 0;
                while (true)
                {
                    visited.Add(GetKey(pos.Item1));
                    if (map[pos.Item1.Item1][pos.Item1.Item2] != '.')
                        oneWay = true;

                    var next = GetNeighbours(pos.Item1, pos.Item2, map, Dirs);
                    if (!next.Any()) break;

                    len++;
                    if (next.Count() == 1)
                    {
                        pos = (next.First(), pos.Item1);
                    }
                    else if (next.Count() > 1)
                    {
                        Junction junction;
                        if (!junctions.TryGetValue(pos.Item1, out junction))
                        {
                            junction = new Junction { Pos = pos.Item1 };
                        }
                        var newPath = new Path { Length = len, StartJ = currJ, EndJ = junction };
                        if (currJ.Paths.ContainsKey(newPath.Key))
                            break;

                        currJ.Paths[newPath.Key] = newPath;
                        if (!oneWay)
                        {
                            var newBackPath = new Path { Length = len, StartJ = junction, EndJ = currJ };
                            junction.Paths[newBackPath.Key] = newBackPath;
                        }
                        next.ToList().ForEach(x => stack.Push((x, pos.Item1, junction)));
                        break;
                    }

                    if (pos.Item1.Equals(goalJ.Pos))
                    {
                        var goal = new Path { Length = len + 1, StartJ = currJ, EndJ = goalJ };
                        currJ.Paths[goal.Key] = goal;
                        var goalBack = new Path { Length = len + 1, StartJ = goalJ, EndJ = currJ };
                        goalJ.Paths[goalBack.Key] = goalBack;
                        break;
                    }
                }
            }

            return (startJ, goalJ);
        }

        static void PrintPaths(Dictionary<(int, int), Junction> junctions)
        {
            foreach (var j in junctions.Values)
            {
                Console.WriteLine($"{j.Pos.Item1},{j.Pos.Item2}: {string.Join("; ", j.Paths.Values.Select(x => $"->{x.EndJ.Pos.Item1},{x.EndJ.Pos.Item2}/{x.Length}"))}");
            }
        }

        static string GetKey((int, int) pos)
        {
            return $"{pos.Item1},{pos.Item2}";
        }

        static Dictionary<char, (int, int)[]> Dirs = new Dictionary<char, (int, int)[]>()
        {
            { '.', new (int, int)[]{ (0, -1), (0, 1), (-1, 0), (1, 0)} },
            { '>', new (int, int)[]{ (0, 1) } },
            { '<', new (int, int)[]{ (0, -1) } },
            { '^', new (int, int)[]{ (-1, 0) } },
            { 'v', new (int, int)[]{ (1, 0) } },
            { '#', new (int, int)[]{ } }
        };

        static List<(int, int)> GetNeighbours((int, int) pos, (int, int) prev, char[][] map, Dictionary<char, (int, int)[]> dirs)
        {
            var res = new List<(int, int)>();
            foreach (var d in Dirs[map[pos.Item1][pos.Item2]])
            {
                var newPos = (pos.Item1 + d.Item1, pos.Item2 + d.Item2);
                if (newPos.Item1 < 0 || newPos.Item1 >= map.Length || newPos.Item2 < 0 || newPos.Item2 >= map[0].Length)
                    continue;
                if (newPos.Equals(prev))
                    continue;
                if (map[newPos.Item1][newPos.Item2] == '#')
                    continue;
                res.Add(newPos);
            }
            return res;
        }   

        public class Explorer
        {
            public int Steps { get; set; }
            public Junction PositionJ { get; set; } = new Junction();
            public HashSet<(int, int)> Visited { get; set; } = new HashSet<(int, int)>();
            public Explorer() { }
            public Explorer(Explorer e)
            {
                Steps = e.Steps;
                PositionJ = e.PositionJ;
                Visited = new HashSet<(int, int)>(e.Visited);
            }
        }
        public class Junction
        {
            public (int, int) Pos { get; set; }
            public Dictionary<string, Path> Paths { get; set; } = new Dictionary<string, Path>();
        }

        public class Path
        {
            public int Length { get; set; }
            public Junction StartJ { get; set; }
            public Junction EndJ { get; set; }
            public string Key => $"{StartJ.Pos.Item1},{StartJ.Pos.Item2};{EndJ.Pos.Item1},{EndJ.Pos.Item2};{Length}";
        }

        public class Heading
        {
            public Heading(int id, Vector direction, Vector pos)
            {
                Direction = direction;
                Position = pos;
                Id = id;
            }

            public int Id { get; set; }
            public Vector Direction { get; set; }
            public Vector Position { get; set; }
            public List<int> SupportedBy { get; set; } = new List<int>();
            public List<int> Supports { get; set; } = new List<int>();
            public List<Vector> GetPath()
            {
                var res = new List<Vector>();
                var p = new Vector(Position);
                var end = new Vector(Position).Add(Direction);
                var d = new Vector(Direction.X > 0 ? 1 : 0, Direction.Y > 0 ? 1 : 0, Direction.Z > 0 ? 1 : 0);
                res.Add(p);
                while (!p.Equals(end))
                {
                    p = new Vector(p).Add(d);
                    res.Add(p);
                }
                return res;
            }
        }

        public class Vector
        {
            public long X { get; set; }
            public long Y { get; set; }
            public long Z { get; set; }

            public Vector(long x, long y, long z)
            {
                X = x;
                Y = y;
                Z = z;
            }

            public Vector(Vector v)
            {
                X = v.X;
                Y = v.Y;
                Z = v.Z;
            }

            public long GetManhLen()
            {
                return Math.Abs(X) + Math.Abs(Y) + Math.Abs(Z);
            }

            public Vector Sub(Vector v)
            {
                X -= v.X;
                Y -= v.Y;
                Z -= v.Z;
                return this;
            }

            public Vector Add(Vector v)
            {
                X += v.X;
                Y += v.Y;
                Z += v.Z;
                return this;
            }

            public Vector Mul(int mul)
            {
                X *= mul;
                Y *= mul;
                Z *= mul;
                return this;
            }

            public override string ToString()
            {
                return $"{X},{Y},{Z}";
            }

            public bool Equals(Vector v)
            {
                return v.X == X && v.Y == Y && v.Z == Z;
            }
        }

        private static string testData2 =
@"";

        private static string testData =
@"
#.#####################
#.......#########...###
#######.#########.#.###
###.....#.>.>.###.#.###
###v#####.#v#.###.#.###
###.>...#.#.#.....#...#
###v###.#.#.#########.#
###...#.#.#.......#...#
#####.#.#.#######.#.###
#.....#.#.#.......#...#
#.#####.#.#.#########v#
#.#...#...#...###...>.#
#.#.#v#######v###.###v#
#...#.>.#...>.>.#.###.#
#####v#.#.###v#.#.###.#
#.....#...#...#.#.#...#
#.#########.###.#.#.###
#...###...#...#...#.###
###.###.#.###v#####v###
#...#...#.#.>.>.#.>.###
#.###.###.#.###.#.#v###
#.....###...###...#...#
#####################.#
";
    }
}