using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace Day25
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            bool test = false;
            string data = "";

            var day = 25;
            if (!test)
            {
                string sessionKey = File.ReadAllText("C:\\Source\\Aoc\\sessionkey.txt");
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Cookie", $"session={sessionKey}");
                data = await client.GetStringAsync($"https://adventofcode.com/2023/day/{day}/input");
                File.WriteAllText($"C:\\Source\\Aoc\\2023\\data{day}.txt", data);
            }
            else
            {
                Console.WriteLine("## RUNNING TEST DATA");
                data = testData.Substring(2).Replace("\r", "");
                File.WriteAllText($"C:\\Source\\Aoc\\2023\\test{day}.txt", data);
            }
            data = data.Substring(0, data.Length - 1);

            var connsTmp = data.Split("\n").Select(x => x.Split(": ").Select(y => y.Split(" ").ToArray()).ToArray()).ToArray();

            var conns = new Dictionary<string, HashSet<string>>();
            foreach (var conn in connsTmp)
            {
                var a = conn[0][0];
                if (!conns.ContainsKey(a))
                {
                    conns[a] = new HashSet<string>();
                }
                foreach (var b in conn[1])
                {
                    if (!conns.ContainsKey(b))
                    {
                        conns[b] = new HashSet<string>();
                    }   
                    conns[b].Add(a);
                    conns[a].Add(b);
                }
            }

            var nodes1 = new HashSet<string>(conns.Keys);
            var nodes2 = new HashSet<string>();

            // Note: For some order of nodes it does not produce a solution.
            //       I tried shuffling the nodes and then it worked.

            // Move the node with the least connections to the first set
            // until only 3 connections exist between the two sets
            while (nodes1.Sum(x => conns[x].Intersect(nodes2).Count()) != 3)
            {
                var minKey = nodes1.MinBy(x => conns[x].Intersect(nodes1).Count());
                nodes1.Remove(minKey);
                nodes2.Add(minKey);
                Console.Write($"\rMoved {nodes2.Count} items to second set");
                if (nodes1.Count() == 0) throw new Exception("No solution found. Try shuffling nodes again.");
            }

            Console.WriteLine();
            Console.WriteLine($"Part 1: {nodes1.Count * nodes2.Count()}");
        }

        private static string testData =
@"
jqt: rhn xhk nvd
rsh: frs pzl lsr
xhk: hfx
cmg: qnr nvd lhk bvb
rhn: xhk bvb hfx
bvb: xhk hfx
pzl: lsr hfx nvd
qnr: nvd
ntq: jqt hfx bvb xhk
nvd: lhk
lsr: lhk
rzs: qnr cmg lsr rsh
frs: qnr lhk lsr
";
    }
}