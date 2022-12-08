using System;
using System.IO;
using System.Threading.Tasks;

namespace Ogettobot
{
    public class Distribution
    {
        public void Distribut()
        {
            Random rand = new Random();
            string[] players = File.ReadAllLines(@"Users\allplayers.txt");
            string[] disctrubplayers = File.ReadAllLines(@"Users\allplayers.txt");

            for (int i = 0; i < players.Length; i++)
            {
                int n = rand.Next(players.Length);

                if (players[i] != disctrubplayers[n] && players[i] != "" && disctrubplayers[n] != "")
                {
                    string[] stats = File.ReadAllLines($@"Users\{players[i]}.txt");
                    stats[3] = disctrubplayers[n];
                    File.WriteAllLines($@"Users\{players[i]}.txt", stats);
                    disctrubplayers[n] = "";
                }

                else if (players[i] != "")
                    i--;
            }
        }
    }
}
