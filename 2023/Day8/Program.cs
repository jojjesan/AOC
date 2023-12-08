using System.Collections;
using System.Collections.Immutable;
using System.Data;
using System.Reflection.Metadata;
using System.Xml;

namespace Day8
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
                data = await client.GetStringAsync("https://adventofcode.com/2023/day/8/input");
            }
            else
            {
                Console.WriteLine("## RUNNING TEST DATA");
                data = testData2.Replace("\r", "");
            }

            // Parse indata
            var lines = data.Split("\n").Where(x => x != "").ToArray();
            var instructions = lines[0].ToCharArray();
            Func<string, string> clean = x => x.Replace(",", "").Replace("(", "").Replace(")", "");
            var nodes = lines.Skip(1).Select(x => x.Split(" "))
                .ToDictionary(x => x[0], y => new Node { Left = clean(y[2]), Right = clean(y[3]) } );

            //Console.WriteLine($"Directions: {string.Join(", ", instructions)}");
            //Console.WriteLine(string.Join(", ", nodes.Keys
            //    .Select(x => $"{x}->{nodes[x].Left}/{nodes[x].Right}")).ToArray());

            //Part1(instructions, nodes);

            Part2b(instructions, nodes);

        }

        static void Part2(char[] instructions, Dictionary<string, Node> nodes)
        {
            var keys = nodes.Where(x => x.Key.EndsWith("A")).Select(x => x.Key).ToArray();
            Console.WriteLine($"Start keys: {string.Join(", ", keys)}");
            var ix = 0;
            long sum = 0;
            var finished = false;
            while (!finished)
            {
                var instruction = instructions[ix];
                if (instruction == 'L')
                {
                    for (var i = 0; i < keys.Length; i++)
                        keys[i] = nodes[keys[i]].Left;
                }
                else
                {
                    for (var i = 0; i < keys.Length; i++)
                        keys[i] = nodes[keys[i]].Right;
                }

                ix = (ix + 1) % instructions.Length;
                if ((sum & 0xfffff) == 0)
                {
                    Console.Write($"\rSum: {sum: ### ### ### ### ###}");
                }
                sum++;

                finished = true;
                for (var i = 0; i < keys.Length; i++)
                {
                    if (keys[i][2] != 'Z')
                    {
                        finished = false;
                        break;
                    }
                }
            }

            Console.WriteLine($"\nPart 2: {sum}");
        }
        static void Part2b(char[] instructions, Dictionary<string, Node> nodes)
        {
            var nodesArr = nodes.Keys.Select(x => new Node2 { Key = x }).ToArray();
            var keys = new List<int>();
            for (var j = 0; j < nodesArr.Length; j++)
            {
                var node = nodes[nodesArr[j].Key];
                nodesArr[j].Left = Array.IndexOf(nodesArr, nodesArr.First(x => x.Key == node.Left));
                nodesArr[j].Right = Array.IndexOf(nodesArr, nodesArr.First(x => x.Key == node.Right));
                if (nodesArr[j].Key.EndsWith("A"))
                    keys.Add(j);
            }

            var keysArr = keys.ToArray();

            Console.WriteLine($"Start keys: {string.Join(", ", keysArr)}");
            var ix = 0;
            long sum = 0;
            var finished = false;
            var zArr = new long[keysArr.Length];
            var zIntervalArr = new long[keysArr.Length];

            while (!finished)
            {
                var instruction = instructions[ix];
                if (instruction == 'L')
                {
                    for (var i = 0; i < keysArr.Length; i++)
                        keysArr[i] = nodesArr[keysArr[i]].Left;
                }
                else
                {
                    for (var i = 0; i < keysArr.Length; i++)
                        keysArr[i] = nodesArr[keysArr[i]].Right;
                }

                ix = (ix + 1) % instructions.Length;
                if ((sum & 0xfffff) == 0)
                {
                    Console.Write($"\rSum: {sum: ### ### ### ### ###}");
                }
                sum++;

                finished = true;
                for (var i = 0; i < keysArr.Length; i++)
                {
                    if (nodesArr[keysArr[i]].Key[2] != 'Z')
                    {
                        finished = false;
                    }
                    else
                    {
                        if (sum - zArr[i] != zIntervalArr[i])
                            Console.Write($"\n{sum}: Key {i} is at Z. Interval {sum - zArr[i]}");
                        zIntervalArr[i] = sum - zArr[i];
                        zArr[i] = sum;
                    }
                }
                if (sum > 100000) break;
            }

            Console.WriteLine();

            //var currZvalues = new long[keysArr.Length];
            //int writeIx = 0;
            //while (true)
            //{
            //    long minPos = long.MaxValue;
            //    var minKey = -1;
            //    for (var i = 0; i < currZvalues.Length; i++)
            //    {
            //        if (currZvalues[i] < minPos)
            //        {
            //            minPos = currZvalues[i];
            //            minKey = i;
            //        }
            //    }
            //    currZvalues[minKey] += zIntervalArr[minKey];
            //    if (writeIx > 1000000)
            //    {
            //        Console.Write($"\rSum: {currZvalues[minKey]: ### ### ### ### ###}");
            //        writeIx = 0;
            //    }
            //    writeIx++; 

            //    if (currZvalues.All(x => x == currZvalues[0])) break;
            //}

            Console.WriteLine();

            var commonFactors = new List<long>(GetFactors(zIntervalArr[0]));
            for (var k = 0; k < zIntervalArr.Length; k++)
            {
                var factors = GetFactors(zIntervalArr[k]);
                commonFactors = commonFactors.Intersect(factors).ToList();
                Console.WriteLine($"Z interval({k}): {zIntervalArr[k]} Factors: {string.Join(", ", factors)}");
            }

            Console.WriteLine($"Common factors: {string.Join(", ", commonFactors)}");

            Console.WriteLine($"\nPart 2: {zIntervalArr.Aggregate((x, y) => x * y /commonFactors.Aggregate((x, y) => x * y))}");
        }

        static long[] GetFactors(long num)
        {
            var prime = int.MaxValue;
            while (prime > 0)
            {
                prime = (int)IsPrime(num);
                if (prime > 0)
                {
                    num /= prime;
                    var tmp = GetFactors(num).ToList();
                    tmp.Add(prime);
                    return tmp.ToArray();
                }
            }

            return new[] {num};
        }

        public static long IsPrime(long number)
        {
            if (number <= 1) return number;
            if (number == 2) return 2;
            if (number % 2 == 0) return -1;

            var boundary = (int)Math.Floor(Math.Sqrt(number));

            for (int i = 3; i <= boundary; i += 2)
                if (number % i == 0)
                    return i;

            return -1;
        }

        private static void Part1(char[] instructions, Dictionary<string , Node> nodes)
        {
            var key = "AAA";
            var ix = 0;
            var sum = 0;
            while (key != "ZZZ")
            {
                if (instructions[ix] == 'L')
                {
                    key = nodes[key].Left;
                }
                else
                {
                    key = nodes[key].Right;
                }
                ix = (ix + 1) % instructions.Length;
                sum++;
            }

            Console.WriteLine($"Part 1: {sum}");
        }

        class Node
        {
            public string Left { get; set; }
            public string Right { get; set; }
        }

        class Node2
        {
            public string Key { get; set; }
            public int Left { get; set; }
            public int Right { get; set; }
        }

        private static string testData = @"LLR

AAA = (BBB, BBB)
BBB = (AAA, ZZZ)
ZZZ = (ZZZ, ZZZ)";

        private static string testData2 = @"LR

11A = (11B, XXX)
11B = (XXX, 11Z)
11Z = (11B, XXX)
22A = (22B, XXX)
22B = (22C, 22C)
22C = (22Z, 22Z)
22Z = (22B, 22B)
XXX = (XXX, XXX)";
    }

}