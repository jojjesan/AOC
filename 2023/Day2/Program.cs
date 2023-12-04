using System.Text.RegularExpressions;

namespace Day2
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string sessionKey = File.ReadAllText("C:\\Source\\Aoc\\sessionkey.txt");
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Cookie", $"session={sessionKey}");
            var data = await client.GetStringAsync("https://adventofcode.com/2023/day/2/input");

            var max = new Draw() { Red = 12, Green = 13, Blue = 14 };

            var lines = data.Split("\n");
            var sum = 0;
            var sum2 = 0;
            foreach (var line in lines)
            {
                if (line == "") continue;

                var game = new Game();
                var m = Regex.Match(line, "Game (.*): (.*)");
                if (m.Groups.Count == 3)
                {
                    game.Id = int.Parse(m.Groups[1].Value);
                    var draws = m.Groups[2].Value.Split("; ");
                    foreach (var d in draws)
                    {
                        var draw = new Draw() { Red = GetColor("red", d), Green = GetColor("green", d), Blue = GetColor("blue", d) };
                        game.Items.Add(draw);
                        if (draw.Red > max.Red || draw.Green > max.Green || draw.Blue > max.Blue)
                        {
                            game.IsValid = false;
                        }
                    }
                }
                var max_red = game.Items.Max(x => x.Red);
                var max_green = game.Items.Max(x => x.Green);
                var max_blue = game.Items.Max(x => x.Blue);
                sum2 += max_red * max_green * max_blue;

                //Console.WriteLine(line);
                //Console.WriteLine($"{game.Id} {string.Join(" | ", game.Items.Select(x => $"r-{x.Red} g-{x.Green} b-{x.Blue}"))}");
                //Console.WriteLine();
                if (game.IsValid)
                {
                    sum += game.Id;
                }
            }

            Console.WriteLine(sum);
            Console.WriteLine(sum2);
        }

        private static int GetColor(string color, string d)
        {
            var m = Regex.Match(d, $"(\\d+) {color}");
            if (m.Groups.Count == 2)
            {
                return int.Parse(m.Groups[1].Value);
            }
            return 0;
        }   

        public class Game
        {
            public int Id { get; set; }
            public bool IsValid = true;
            public IList<Draw> Items = new List<Draw>();
        }
        public class Draw
        {
            public int Red { get; set; }
            public int Green { get; set; }
            public int Blue { get; set; }
        }
    }
}