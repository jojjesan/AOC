using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace Day15
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
                var day = 15;
                data = await client.GetStringAsync($"https://adventofcode.com/2023/day/{day}/input");
                File.WriteAllText($"C:\\Source\\Aoc\\2023\\data{day}.txt", data);
            }
            else
            {
                Console.WriteLine("## RUNNING TEST DATA");
                data = testData.Substring(2).Replace("\r", "");
            }

            var steps = data.Replace("\n", "").Split(",").ToArray();
            var sum = 0;
            foreach (var step in steps)
            {
                var value = Hash(step);
                Console.WriteLine($"{step} {value}");
                sum += value;
            }

            Console.WriteLine($"Part 1: {sum}");

            // Part 2

            // Split step into label action and value
            var stepList = steps.Select(x => {
                var parts = Regex.Match(x, "([a-z]*)(-|=)([0-9]*)");
                return new LensRule
                {
                    Lbl = parts.Groups[1].Value,
                    Op = parts.Groups[2].Value,
                    Val = ParseInt(parts.Groups[3].Value)
                };
            });

            // Create a list of 256 lists
            var buckets = new List<LensRule>[256];
            buckets = buckets.Select(x => new List<LensRule>()).ToArray();

            // Loop through all steps:
            //   Add last or replace if action is =
            //   Remove item with label if action is -
            foreach (var rule in stepList)
            {
                var bucket = Hash(rule.Lbl);
                if (rule.Op == "=")
                {
                    var existingRule = buckets[bucket].FirstOrDefault(x => x.Lbl == rule.Lbl);
                    if (existingRule != null)
                    {
                        existingRule.Val = rule.Val;
                    }
                    else
                    {
                        buckets[bucket].Add(rule);
                    }
                }
                else if (rule.Op == "-")
                {
                    var existingRule = buckets[bucket].FirstOrDefault(x => x.Lbl == rule.Lbl);
                    if (existingRule != null)
                    {
                        buckets[bucket].Remove(existingRule);
                    }
                }
            }

            // Calculate focal strength of each bucket and sum up for all buckets
            var power = 0;
            for (var bucketIx = 0; bucketIx < buckets.Length; bucketIx++)
            {
                for (var posIx = 0; posIx < buckets[bucketIx].Count(); posIx++)
                {
                    power += buckets[bucketIx][posIx].Val * (posIx + 1) * (bucketIx + 1);
                }
            }

            Console.WriteLine($"Part 2: {power}");
        }

        static int ParseInt(string input)
        {
            if (int.TryParse(input, out var result))
            {
                return result;
            }
            return -1;
        }

        static int Hash(string input)
        {
            var value = 0;
            foreach (var asciiByte in Encoding.ASCII.GetBytes(input))
            {
                value += asciiByte;
                value *= 17;
                value = value % 256;
            }
            return value;
        }

        class LensRule
        {
            public string Lbl { get; set; }
            public string Op { get; set; }
            public int Val { get; set; }
        }

        private static string testData2 =@"
";

        private static string testData = @"
rn=1,cm-,qp=3,cm=2,qp-,pc=4,ot=9,ab=5,pc-,pc=6,ot=7";
    }
}