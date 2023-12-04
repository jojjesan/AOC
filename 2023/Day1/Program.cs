using System;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace Day1 // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string sessionKey = File.ReadAllText("C:\\Source\\Aoc\\sessionkey.txt");
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Cookie", $"session={sessionKey}");
            var data = await client.GetStringAsync("https://adventofcode.com/2023/day/1/input");

            //var lines = data.Split("\n");
            //Part(lines, 1);
            var lines2 = data.Split("\n");
            for (int i = 0; i < lines2.Length; i++)
            {
                var tmp = lines2[i];
                //Console.WriteLine($"-> {tmp}");
                lines2[i] = MarkOneDigit(lines2[i], RegexOptions.None);
                lines2[i] = ReplaceMarkedDigits(lines2[i]);
                //Console.WriteLine($"{lines2[i]}");
                lines2[i] = MarkOneDigit(lines2[i], RegexOptions.RightToLeft);
                lines2[i] = ReplaceMarkedDigits(lines2[i]);
                //Console.WriteLine($"{lines2[i]}");
                lines2[i] = GetFirstAndLastDigit(lines2[i]);
                //Console.WriteLine($"{lines2[i]}");
                //Console.WriteLine();
            }
            Console.WriteLine(lines2.Sum(x => x != "" ? int.Parse(x) : 0));
            //Part(lines2, 2);
            //Console.WriteLine(string.Join("\n", lines2));
            // Fel 54235, 54729(too high)
        }

        static void Part(string[] lines, int part)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = Regex.Replace(lines[i], "[a-z]*", "");
                if (lines[i].Length > 0) 
                    lines[i] = lines[i].Substring(0, 1) + lines[i].Substring(lines[i].Length - 1, 1);
            }
            Console.Write($"Part {part}:");
            Console.WriteLine(lines.Sum(x => x != "" ? int.Parse(x) : 0));
            //Console.WriteLine(string.Join("\n", lines));
        }

        private static string MarkOneDigit(string line, RegexOptions options)
        {
            var r = new Regex("(nine|eight|seven|six|five|four|three|two|one)", options);
            var result = r.Replace(line, "#$0#", 1);
            return result;
        }

        private static string GetFirstAndLastDigit(string line)
        {
            line = Regex.Replace(line, "[a-z]*", "");
            return line.Length > 0 ? line.Substring(0, 1) + line.Substring(line.Length - 1, 1) : "";
        }   

        private static string ReplaceMarkedDigits(string input) 
        {
            input = Regex.Replace(input, "#one#", "o1e");
            input = Regex.Replace(input, "#two#", "t2o");
            input = Regex.Replace(input, "#three#", "th3ee");
            input = Regex.Replace(input, "#four#", "f4ur");
            input = Regex.Replace(input, "#five#", "f5ve");
            input = Regex.Replace(input, "#six#", "s6x");
            input = Regex.Replace(input, "#seven#", "se7en");
            input = Regex.Replace(input, "#eight#", "ei8ht");
            input = Regex.Replace(input, "#nine#", "n9ne");
            return input;
        }

        private static string testData = @"two1nine
eightwothree
abcone2threexyz
xtwone3four
4nineeightseven2
zoneight234
7pqrstsixteen";
    }
}