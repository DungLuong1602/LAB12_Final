using System.Numerics;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json;
namespace ConsoleApp1
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            HttpClient client = new HttpClient();
            string url = "https://raw.githubusercontent.com/NTH-VTC/OnlineDemoC-/refs/heads/main/lab12_players.json";
            string json = await client.GetStringAsync(url);


            List<Player> players = JsonConvert.DeserializeObject<List<Player>>(json);

            var firebase = new FirebaseClient("https://lab12-6a9d4-default-rtdb.asia-southeast1.firebasedatabase.app/");

            //câu 1.1:
            DateTime now = new DateTime(2025, 07, 01, 0, 0, 0, DateTimeKind.Utc);
            var inactivePlayers = players.Where(p => (!p.IsActive || (now - p.LastLogin).Days > 10) && p.Level <= 8)
                .Select(p => new
                {
                    p.Name,
                    p.IsActive,
                    p.Level,
                    lastlogin = p.LastLogin.ToString("yyyy-MM-dd HH:mm:ss")
                })
                .ToList();
            int index1 = 0;
            Console.WriteLine("-------DANH SÁCH NGƯỜI CHƠI KHÔNG HOẠT ĐỘNG QUÁ 10 NGÀY VÀ CÓ LEVEL THẤP HƠN 9-------");
            foreach (var player in inactivePlayers)
            {
                Console.WriteLine($"Name: {player.Name}     | IsActive: {player.IsActive}     | Level: {player.Level}     | LastLogin: {player.lastlogin}");
                index1++;
                await firebase
                    .Child("inactive_lowlevel_players")
                    .Child(index1.ToString())
                    .PutAsync(player);
            }
            Console.WriteLine("------------------------------------------------------------------------------------\n");
            Console.ReadLine();
            //câu 1.2:
            var richPlayers = players.Where(p => p.Level >= 12 && p.Gold > 2000)
                .Select(p => new
                {
                    p.Name,
                    p.Level,
                    p.Gold
                })
                .ToList();
            int index2 = 0;
            Console.WriteLine("-------DANH SÁCH NGƯỜI CHƠI GIÀU CÓ VÀ CÓ LEVEL CAO HƠN 12-------");
            foreach (var player in richPlayers)
            {
                Console.WriteLine($"Name: {player.Name}     | Level: {player.Level}     | CurrentGold: {player.Gold}");
                index2++;
                await firebase
                    .Child("highlevel_rich_players")
                    .Child(index2.ToString())
                    .PutAsync(new
                    {
                        player.Name,
                        player.Level,
                        CurrentGold = player.Gold
                    });
            }
            Console.WriteLine("------------------------------------------------------------------------------------\n");
            Console.ReadLine();
            //Câu 2:
            var top3coins = players.Where(p => p.IsActive && (now - p.LastLogin).Days <= 3)
                .OrderByDescending(p => p.Coins)
                .Take(3)
                .Select(p => new
                {
                    p.Name,
                    p.Coins,
                    p.Level,
                    p.AwardedCoinAmount
                })
                .ToList();
            int[] awards = { 3000, 2000, 1000 };
            int index3 = 0;
            Console.WriteLine("-------TOP 3 NGƯỜI CHƠI CÓ COIN CAO NHẤT VÀ HOẠT ĐỘNG TRONG VÒNG 3 NGÀY VỪA QUA-------");
            for (int i = 0; i < top3coins.Count; i++)
            {
                var player = top3coins[i];
                Console.WriteLine($"Name: {player.Name}     | Coins: {player.Coins}     | Level: {player.Level}     | AwardedCoinAmount: {awards[i]}     | Rank: {i + 1} ");
                index3++;
                await firebase
                    .Child("top3_active_coin_players")
                    .Child(index3.ToString())
                    .PutAsync(new
                    {
                        player.Name,
                        player.Coins,
                        player.Level,
                        AwardedCoinAmount = awards[i],
                        Rank = index3
                    });
            }
            Console.WriteLine("------------------------------------------------------------------------------------\n");
            Console.ReadLine();
        }
        public class Player
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Level { get; set; }
            public int Gold { get; set; }
            public int Coins { get; set; }
            public Boolean IsActive { get; set; }
            public string VipLevel { get; set; }
            public string Region { get; set; }
            public DateTime LastLogin { get; set; }
            public int AwardedCoinAmount { get; set; }
        }
    }
}
