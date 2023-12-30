using System;
using System.Collections;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using static Day19.Program;

namespace Day19
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
                var day = 19;
                data = await client.GetStringAsync($"https://adventofcode.com/2023/day/{day}/input");
                File.WriteAllText($"C:\\Source\\Aoc\\2023\\data{day}.txt", data);
            }
            else
            {
                Console.WriteLine("## RUNNING TEST DATA");
                data = testData.Substring(2).Replace("\r", "");
            }

            var records = data.Split("\n\n");
            var workflows = records[0].Split("\n").Select(x => x.Split("{"))
                .Select(y => new { Name = y[0], Rules = y[1].Replace("}", "").Split(",").Select(ParseRule) })
                .ToDictionary(k => k.Name, v => v.Rules.ToArray());
            var items = records[1].Split("\n").Where(x => x != "")
                .Select(x => Regex.Replace(x, "[{}]", ""))
                .Select(x => x.Split(",").Select(y => y.Split("=")).ToDictionary(k => k[0], v => int.Parse(v[1]))).ToList();

            var sum = 0;
            foreach (var item in items)
            {
                var res = TraverseWorkflows("in", item, workflows);
                //Console.WriteLine($"{item["x"]},{item["m"]},{item["a"]},{item["s"]} => {res}");
                if (res == "A")
                    sum += item.Sum(x => x.Value);
            }

            Console.WriteLine($"Part 1: {sum}");

            var startNode = new Node()
            {
                Intervals = new Dictionary<string, Tuple<long, long>>()
                {
                    {"x", new Tuple<long, long>(1, 4000)},
                    {"m", new Tuple<long, long>(1, 4000)},
                    {"a", new Tuple<long, long>(1, 4000)},
                    {"s", new Tuple<long, long>(1, 4000)}
                }
            };

            var part2 = TraverseWorkflows2("in", startNode, workflows);
            Console.WriteLine($"Part 2: {part2}");
        }

        private static string TraverseWorkflows(string currWorkflowName, Dictionary<string, int> item,
            Dictionary<string, Step[]> workflows)
        {
            foreach (var step in workflows[currWorkflowName])
            {
                var val = step.Variable != "" ? item[step.Variable] : 0;
                if (step.Function(val))
                {
                    if (step.Next == "A" || step.Next == "R")
                        return step.Next;
                    else
                        return TraverseWorkflows(step.Next, item, workflows);
                }
            }
            throw new Exception("Last step should always be true. Something went wrong!");
        }

        private static long TraverseWorkflows2(string currWorkflowName, Node currNode,
            Dictionary<string, Step[]> workflows)
        {
            var currIntervals = new Dictionary<string, Tuple<long, long>>(currNode.Intervals);
            if (currWorkflowName == "A")
            {
                var sumIntvs = currIntervals
                    .Select(x => (x.Value.Item2 - x.Value.Item1 + 1))
                    .Select(x => x < 0 ? 0 : x)
                    .Aggregate((p, c) => p * c);
                //Console.WriteLine($"{string.Join("; ", currIntervals.Select(x => $"{x.Key}:{x.Value.Item1}-{x.Value.Item2}").ToArray())}");
                //Console.WriteLine($"{string.Join("; ", currNode.Steps.Select(x => x.ToString()))}");
                //Console.WriteLine();
                return sumIntvs;
            }
            else if (currWorkflowName == "R")
            {
                return 0;
            }

            long sum = 0;
            var steps = new List<Step>(currNode.Steps);

            foreach (var step in workflows[currWorkflowName])
            {
                steps.Add(step);

                if (step.Variable == "")
                {
                    sum += TraverseWorkflows2(step.Next, 
                        new Node() { Intervals = new Dictionary<string, Tuple<long, long>>(currIntervals), 
                            Steps = new List<Step>(steps) }, 
                        workflows);
                }
                else
                {
                    var splitIntervals = SplitInterval(currIntervals[step.Variable], step.Operator, step.Value);

                    var nextInterval = step.Operator == "<" ? splitIntervals[0] : splitIntervals[1];
                    if (nextInterval.Item1 == 0) continue;
                    
                    var nextNode = new Node() { Intervals = new Dictionary<string, Tuple<long, long>>(currIntervals),
                        Steps = new List<Step>(steps)};
                    nextNode.Intervals[step.Variable] = nextInterval;
                    sum += TraverseWorkflows2(step.Next, nextNode, workflows);

                    var restInterval = step.Operator == "<" ? splitIntervals[1] : splitIntervals[0];
                    if (restInterval.Item1 == 0) return sum;

                    currIntervals[step.Variable] = restInterval;
                }

            }
            return sum;
        }

        static Tuple<long, long>[] SplitInterval(Tuple<long, long> interval, string op, int value)
        {
            var offset = op == "<" ? -1 : 0;
            var res = new List<Tuple<long, long>>()
            {
                new Tuple<long, long>(interval.Item1, value + offset),
                new Tuple<long, long>(value + offset + 1, interval.Item2)
            };
            return res.Select(x => Limit(x, interval)).ToArray();
        }

        static Tuple<long, long> Limit(Tuple<long, long> src, Tuple<long, long> limit)
        {
            var res = new Tuple<long, long>(
                Math.Max(src.Item1, limit.Item1),
                Math.Min(src.Item2, limit.Item2)
            );
            if (res.Item1 > limit.Item2 || res.Item2 < limit.Item1 || res.Item1 > res.Item2)
                return new Tuple<long, long>(0, -1);
            return res;
        }

        static Step ParseRule(string rule)
        {
            var parts = rule.Split(":");
            var next = parts.Last();
            if (parts.Length  == 2)
            {
                var m = Regex.Match(parts[0], @"(\w+)([<>])(\d+)");
                var val = int.Parse(m.Groups[3].Value);
                return new Step()
                {
                    Value = val,
                    Operator = m.Groups[2].Value,
                    RawText = rule,
                    Variable = m.Groups[1].Value,
                    Next = next,
                    Function = (m.Groups[2].Value == "<") ? x => x < val : x => x > val
                };
            }
            return new Step()
            {
                Value = 0,
                Operator = "",
                RawText = rule,
                Variable = "",
                Next = next,
                Function = x => true
            };
        }

        static Dictionary<string, Vector> Directions = new Dictionary<string, Vector>()
        {
            { "U", new Vector(-1, 0)},
            { "D", new Vector(1, 0)},
            { "R", new Vector(0, 1)},
            { "L", new Vector(0, -1)}
        };

        public class Node
        {
            public Dictionary<string, Tuple<long, long>> Intervals { get; set; }
            public List<Step> Steps { get; set; }
            public Node()
            {
                Steps = new List<Step>();
                Intervals = new Dictionary<string, Tuple<long, long>>();
            }
        }

        public class Step
        {
            public string RawText { get; set; }
            public string Variable { get; set; }
            public string Next { get; set; }
            public int Value { get; set; }
            public string Operator { get; set; }
            public Func<int, bool> Function { get; set; }
            public override string ToString()
            {
                return RawText;
            }
        }

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
px{a<2006:qkq,m>2090:A,rfg}
pv{a>1716:R,A}
lnx{m>1548:A,A}
rfg{s<537:gd,x>2440:R,A}
qs{s>3448:A,lnx}
qkq{x<1416:A,crn}
crn{x>2662:A,R}
in{s<1351:px,qqz}
qqz{s>2770:qs,m<1801:hdj,R}
gd{a>3333:R,R}
hdj{m>838:A,pv}

{x=787,m=2655,a=1222,s=2876}
{x=1679,m=44,a=2067,s=496}
{x=2036,m=264,a=79,s=2244}
{x=2461,m=1339,a=466,s=291}
{x=2127,m=1623,a=2188,s=1013}";
    }
}