using System.Text.RegularExpressions;

namespace Day1_2
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string sessionKey = File.ReadAllText("C:\\Source\\Aoc\\sessionkey.txt");
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Cookie", $"session={sessionKey}");
            var data = await client.GetStringAsync("https://adventofcode.com/2023/day/1/input");

            var lines2 = data.Split("\n");
            for (int i = 0; i < lines2.Length; i++)
            {
                var tmp = lines2[i];
                var first = GetOne(tmp, RegexOptions.None);
                var last = GetOne(tmp, RegexOptions.RightToLeft);
                Console.WriteLine($"{tmp} -> {first} {last}");
                lines2[i] = first + last;
            }
            //Console.WriteLine(lines2.Sum(x => x != "" ? int.Parse(x) : 0));
            //Part(lines2, 2);
            //Console.WriteLine(string.Join("\n", lines2));
            // Fel 54235, 54729(too high)

            Console.WriteLine(lines2.Sum(x => x != "" ? int.Parse(x) : 0));
        }


        static string GetOne(string line, RegexOptions options)
        {
            var r = new Regex("(nine|eight|seven|six|five|four|three|two|one|0|1|2|3|4|5|6|7|8|9)", options);
            var match = r.Match(line);
            var res = match.Groups.Count > 0 ? match.Groups[0].Value : "";
            return ReplaceDigits(res);
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

        private static string ReplaceDigits(string input)
        {
            input = Regex.Replace(input, "one", "1");
            input = Regex.Replace(input, "two", "2");
            input = Regex.Replace(input, "three", "3");
            input = Regex.Replace(input, "four", "4");
            input = Regex.Replace(input, "five", "5");
            input = Regex.Replace(input, "six", "6");
            input = Regex.Replace(input, "seven", "7");
            input = Regex.Replace(input, "eight", "8");
            input = Regex.Replace(input, "nine", "9");
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