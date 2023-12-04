namespace Day4
{
    internal class Program
    {
        static bool skipLog = true;

        static async Task Main(string[] args)
        {
            bool test = false;
            string data = "";
            if (!test)
            {
                string sessionKey = File.ReadAllText("C:\\Source\\Aoc\\sessionkey.txt");
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Cookie", $"session={sessionKey}");
                data = await client.GetStringAsync("https://adventofcode.com/2023/day/4/input");
            }
            else
            {
                Console.WriteLine("## RUNNING TEST DATA");
                data = testData.Replace("\r", "");
            }

            var lines = data.Split("\n").Where(x => x != "").ToArray();
            var cards = new List<Card>();
            foreach (var line in lines)
            {
                cards.Add(new Card(line));
            }

            // Part 1
            Console.WriteLine($"Part 1: {cards.Sum(x => GetPoints(x.GetNbrOfMatches()))}");

            // Part 2
            for (var ix = 0; ix < cards.Count(); ix++)
            {
                var matches = cards[ix].GetNbrOfMatches();
                UpdateCardCounts(cards, ix, matches);
            }

            Console.WriteLine($"Part 2: {cards.Sum(x => x.Count)}");
        }

        static void UpdateCardCounts(List<Card> cards, int ix, int matches)
        {
            Log($"Processing card {ix}");
            if (matches == 0) return;

            var max = cards.Count() - 1;
            for (var i = Math.Min(ix + 1, max); i <= Math.Min(ix + matches, max); i++)
            {
                cards[i].Count += cards[ix].Count;
                Log($"Card {i} increased with {cards[ix].Count}");
            }
            Log();
        }

        static int GetPoints(int nbrOfMatches)
        {
            if (nbrOfMatches == 0)
                return 0;
            return 1 << (nbrOfMatches-1);
        }

        static void Log(string message = "", bool skipNewline = true)
        {
            if (skipLog) return;

            if (skipNewline)
            {
                Console.Write(message ?? "");
            }
            else
            {
                Console.WriteLine(message ?? ""); 
            }
        }

        private static string testData = @"Card 1: 41 48 83 86 17 | 83 86  6 31 17  9 48 53
Card 2: 13 32 20 16 61 | 61 30 68 82 17 32 24 19
Card 3:  1 21 53 59 44 | 69 82 63 72 16 21 14  1
Card 4: 41 92 73 84 69 | 59 84 76 51 58  5 54 83
Card 5: 87 83 26 28 32 | 88 30 70 12 93 22 82 36
Card 6: 31 18 13 56 72 | 74 77 10 23 35 67 36 11";
    }

    internal class Card
    {
        public int Id { get; set;}
        public int Count { get; set; }
        public List<int> Winners { get; set; }
        public List<int> MyNumbers { get; set; }
        internal Card(string data)
        {
            Count = 1;
            Winners = new List<int>();
            MyNumbers = new List<int>();
            Parse(data);
        }

        internal int GetNbrOfMatches()
        {
            return MyNumbers.Intersect(Winners).Count();
        }

        internal void Parse(string data)
        {
            var parts1 = data.Split(":");
            Id = int.Parse(parts1[0].Replace("Card", "").Trim());
            var parts2 = parts1[1].Split("|");
            var winners = parts2[0].Trim().Split(" ").Where(x => x != "");
            Winners.AddRange(winners.Select(x => int.Parse(x.Trim())));
            var myNumbers = parts2[1].Trim().Split(" ").Where(x => x != "");
            MyNumbers.AddRange(myNumbers.Select(x => int.Parse(x.Trim())));
        }
    } 
}