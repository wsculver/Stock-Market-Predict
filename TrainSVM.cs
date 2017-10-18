using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using StockMarketPridict;
using TwitterAPIIntegration;

namespace StockMarketPridict
{
    public static class TrainSVM
    {
        static string MySqlConnectionString = "SERVER=localhost;DATABASE=srs;UID=root;PASSWORD=password;";
        static MySqlConnection connection = new MySqlConnection(MySqlConnectionString);
        static MySqlCommand cmd;

        public static void GetAllWords()
        {
            connection.Open();
            cmd = new MySqlCommand("USE srs", connection);
            cmd.ExecuteNonQuery();

            for(int i = 1; i <= Program.LoadData("50_train_tweets", "tweet", "")[1].Count; i++)
            {
                string[] words = (Program.LoadData("50_train_tweets", "tweet", "")[1])[i].Split(' ');
                for(int ii = 0; ii < words.Length; ii++)
                {
                    Console.WriteLine(words[ii]);
                    var matchingWords = Program.LoadData("all_tweet_words", "word", "")[1].Where(w => w.Contains(words[ii] + " "));
                    if (!matchingWords.Contains(words[ii] + " ") && words[ii] != "")
                    {
                        Program.AddData("all_tweet_words", "word", words[ii] + " ", "", "", "", "");
                    }
                }
            }
            connection.Close();
        }

        public static void UpdateAllWords()
        {
            connection.Open();
            cmd = new MySqlCommand("USE srs", connection);
            cmd.ExecuteNonQuery();

            for (int i = 0; i < Program.LoadData("`50_train_tweets_(original)`", "tweet", "")[1].Count; i++)
            {
                Program.AddData("50_train_tweets", "Sentiment", Program.LoadPoint("`50_train_tweets_(original)`", "sentiment", i + 1),
                    "tweet", Program.ReplaceChars((Program.LoadData("`50_train_tweets_(original)`", "tweet", "")[1])[i]), "", "");
            }
        }

        public static void FormatTweets()
        {
            string words = "";
            for (int i = 1; i <= Program.LoadData("50_train_tweets", "tweet", "")[1].Count(); i++)
            {
                for (int ii = 1; ii <= Program.LoadData("all_tweet_words", "word", "")[1].Count; ii++)
                {
                    if (Program.LoadPoint("50_train_tweets", "tweet", i).Contains((" " + Program.LoadPoint("all_tweet_words", "word", ii)), StringComparison.OrdinalIgnoreCase))
                    {
                        words = words + ii + ":1 ";
                    }
                    else
                    {
                        words = words + ii + ":0 ";
                    }
                }
                Program.AddData("train_tweet_arrays", "Sentiment", Program.LoadPoint("50_train_tweets", "Sentiment", i), "Array", words, "", "");
                words = "";
            }
        }
    }
}
