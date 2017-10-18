using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToTwitter;
using System.IO;
using System.Data.OleDb;
using System.Data;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;
using StockMarketPridict;

namespace TwitterAPIIntegration
{
    static class Program
    {
        static string MySqlConnectionString = "SERVER=localhost;DATABASE=srs;UID=root;PASSWORD=password;";
        static MySqlConnection connection = new MySqlConnection(MySqlConnectionString);
        static MySqlCommand cmd;
        static int AllCompCount;

        public static void Main(string[] args)
        {
            Console.WriteLine("I think I can, I think I can, I think I can...");

            connection.Open();
            cmd = new MySqlCommand("USE srs", connection);
            cmd.ExecuteNonQuery();
        
            var tweetList = GetTwitterFeeds();
            bool FileVarSet = false;
            int ID = LoadData("all_word_arrays", "Array", "")[1].Count + 1;
            if(ID == 0)
            {
                ID = 1;
            }
            int TweetID = LoadData("edited_tweets", "tweet", "")[1].Count + 1;
            if(TweetID != 0)
            {
                TweetID--;
            }
            int AllCompLength = LoadData("all_companies", "symbol", "")[1].Count;
            AllCompCount = AllCompLength;
            //TrainSVM.FormatTweets();
            //connection.Close();

            Console.WriteLine("Tweets Count " + tweetList.Count);

            foreach (var item in tweetList)
            {
                item.Text = item.Text + " ";
                item.Text = item.Text.Replace("'", "");
                AddData("previous_tweets", "tweet", item.Text, "", "", "", "");
                Console.WriteLine("tweet added to previous tweets");

                item.Text = ReplaceChars(item.Text);
                AddData("edited_tweets", "tweet", item.Text, "", "", "", "");
                TweetID++;

                for (int i = 1; i <= AllCompLength; i++)
                {
                    if ((item.Text.Contains("SYMBOL ", StringComparison.OrdinalIgnoreCase) || item.Text.Contains("NAME ", StringComparison.OrdinalIgnoreCase)) && !FileVarSet)
                    {
                            FileVarSet = true;

                        AddData("all_word_arrays", "Sentiment", "+1", "tweetID", TweetID + "", "", "");
                        /*for (int ii = 1; ii <= LoadData("all_tweet_words", "word", "")[1].Count; ii++)
                        {
                            if (item.Text.Contains((" " + LoadPoint("all_tweet_words", "word", ii)), StringComparison.OrdinalIgnoreCase))
                            {
                                UpdateData("all_word_arrays", "word" + ii, ii + ":1 ", ID);
                            }
                            else
                            {
                                UpdateData("all_word_arrays", "Word" + ii, ii + ":0 ", ID);
                            }
                        }*/
                        string words = "";

                        for (int ii = 1; ii <= LoadData("all_tweet_words", "word", "")[1].Count; ii++)
                        {
                            if (item.Text.Contains((" " + LoadPoint("all_tweet_words", "word", ii)), StringComparison.OrdinalIgnoreCase))
                            {
                                words = words + ii + ":1 ";
                            }
                            else
                            {
                                words = words + ii + ":0 ";
                            }
                        }
                        UpdateData("all_word_arrays", "Array", words, ID);

                        ID++;
                        Console.WriteLine("done with tweet company table");
                    }
                }
                FileVarSet = false;
            }
            connection.Close();
            Console.WriteLine("I made it!!! Phew.");
            Console.ReadLine();
        }

        //Gets tweets
        public static List<Status> GetTwitterFeeds()
        {
            var auth = new SingleUserAuthorizer
            {
                CredentialStore = new InMemoryCredentialStore()
                {
                    ConsumerKey = "qGuAmepeXNmKBX94XdFkHa8Pz",
                    ConsumerSecret = "wNcdbn3jYSITazILqOG33Lj1RY2k6kUlKaG2cjKnEuWZ4YWIZP",
                    OAuthToken = "901596402383544320-V4szsa3jD5vfW7L3HtZpcZ7jVMl67ek",
                    OAuthTokenSecret = "SYhKAQKm8jC9YTaP5as4KXZJIHgzoxSTPiXGJPehAyCc1"
                }
            };
            var twitterCtx = new TwitterContext(auth);

            var Tweets = new List<Status>();

            ulong maxId = 0;
            bool flag = true;
            var statusResponse = new List<Status>();
            
            statusResponse = (from tweet in twitterCtx.Status
                              where tweet.Type == StatusType.Home
                                    && tweet.Count == 200
                                    && (DateTime)tweet.CreatedAt >= DateTime.Today
                                    && (LoadData("all_companies", "symbol", "")[1]).Any(w => tweet.Text.Contains(w)) || (LoadData("all_companies", "name", "")[1]).Any(w => tweet.Text.Contains(w))
                                    && !((LoadData("previous_tweets", "tweet", "")[1]).Any(w => tweet.Text.Contains(w)))
                              select tweet).ToList();

            if (statusResponse.Count > 0)
            {
                maxId = ulong.Parse(statusResponse.Last().StatusID.ToString()) - 1;
                Tweets.AddRange(statusResponse);
            }

            while (flag)
            {
                int rateLimitStatus = twitterCtx.RateLimitRemaining;

                if (rateLimitStatus != 0)
                {
                    statusResponse = (from tweet in twitterCtx.Status
                                      where tweet.Type == StatusType.Home
                                            && tweet.MaxID == maxId
                                            && tweet.Count == 200
                                            && (DateTime)tweet.CreatedAt >= DateTime.Today
                                            && (LoadData("all_companies", "symbol", "")[1]).Any(w => tweet.Text.Contains(w)) || (LoadData("all_companies", "name", "")[1]).Any(w => tweet.Text.Contains(w))
                                            && !((LoadData("previous_tweets", "tweet", "")[1]).Any(w => tweet.Text.Contains(w)))
                                      select tweet).ToList();


                    if (statusResponse.Count != 0)
                    {
                        maxId = ulong.Parse(statusResponse.Last().StatusID.ToString()) - 1;
                        Tweets.AddRange(statusResponse);
                    }
                    else
                    {
                        flag = false;
                    }
                }
                else
                {
                    flag = false;
                }
            }
            return Tweets;
        }

        public static List<string>[] LoadData(string TableName, string column, string column2)
        {
            List<string>[] list = new List<string>[3];
            list[0] = new List<string>();
            list[1] = new List<string>();
            list[2] = new List<string>();

            string query;

            if (column2 != "")
            {
                query = "SELECT * FROM " + TableName;
            }
            else
            {
                query = "SELECT " + column + " FROM " + TableName;
            }
            MySqlCommand cmd = new MySqlCommand(query, connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            while (dataReader.Read())
            {
                list[1].Add(dataReader[column] + "");
                if (column2 != "")
                {
                    list[0].Add(dataReader["id"] + "");
                    list[2].Add(dataReader[column2] + "");
                }
            }

            dataReader.Close();

            return list;
        }

        public static string LoadPoint(string TableName, string column, int row)
        {
            string query = "SELECT " + column + " FROM " + TableName + " WHERE ID = " + row;
            MySqlCommand cmd = new MySqlCommand(query, connection);
            string point = Convert.ToString(cmd.ExecuteScalar());

            return point;
        }

        public static void AddData(string TableName, string column, string contents, string column2, string contents2, string column3, string contents3)
        {
            string query;

            if (column2 != "")
            {
                if (column3 != "")
                {
                    query = "INSERT INTO " + TableName + " (" + column + "," + column2 + "," + column3 + ") " + "VALUES('" + contents + "','" + contents2 + "','" + contents3 + "')";
                }
                else
                {
                    query = "INSERT INTO " + TableName + " (" + column + "," + column2 + ") " + "VALUES('" + contents + "','" + contents2 + "')";
                }
            }
            else
            {
                query = "INSERT INTO " + TableName + " (" + column + ") " + "VALUES('" + contents + "')";
            }

            cmd = new MySqlCommand(query, connection);
            cmd.ExecuteNonQuery();
        }

        public static void UpdateData(string TableName, string column, string contents, int ID)
        {
            string query = "UPDATE " + TableName + " SET " + column + "= '" + contents + "' WHERE ID = " + ID;

            cmd = new MySqlCommand(query, connection);
            cmd.ExecuteNonQuery();
        }

        public static void CreateTable(string TableName, string column1, string type1, string column2, string type2, string column3, string type3, int conn)
        {
            string query = "CREATE TABLE " + TableName + " (" + column1 + " " + type1 + " NOT NULL AUTO_INCREMENT, " + column2 + " " + type2 + " NOT NULL, " + column3 + " " + type3 + " NOT NULL, PRIMARY KEY(" + column1 + "))";

            cmd = new MySqlCommand(query, connection);
            cmd.ExecuteNonQuery();
        }
        
        public static bool Contains(this string target, string value, StringComparison comparison)
        {
            return target.IndexOf(value, comparison) >= 0;
        }

        public static string ReplaceChars(string input)
        {
            string output = input;

            output = Regex.Replace(output, @"((([A-Za-z]{3,9}:(?:\/\/)?)(?:[-;:&=\+\$,\w]+@)?[A-Za-z0-9.-]+|(?:https://|[-;:&=\+\$,\w]+@)[A-Za-z0-9.-]+)((?:\/[\+~%\/.\w-_]*)?\??(?:[-\+=&;%@.\w_]*)#?(?:[\w]*))?)", "URL ");
            output = Regex.Replace(output, @"((?<=\-)\d+\.?((?<=\.)\d+)?)", " NEGNUM ");
            output = Regex.Replace(output, @"(\d+\.?((?<=\.)\d+)?)", " POSNUM ");
            output = output.Replace(".", "");
            output = output.Replace("…", "");
            output = output.Replace("?", "");
            output = output.Replace("!", "");
            output = output.Replace(",", "");
            output = output.Replace("(", "");
            output = output.Replace(")", "");
            for (int i = 1; i <= AllCompCount; i++)
            {
                if (output.Contains(LoadPoint("all_companies", "symbol", i), StringComparison.OrdinalIgnoreCase))
                {
                    output = output.Replace(LoadPoint("all_companies", "symbol", i), " SYMBOL ");
                }

                if (output.Contains(LoadPoint("all_companies", "name", i), StringComparison.OrdinalIgnoreCase))
                {
                    output = output.Replace(LoadPoint("all_companies", "name", i), " NAME ");
                }
            }
            //output = Regex.Replace(output, @"https?://[-\w]+(\.\w[-\w]*)+(:\d+)?(/[^.!,?;""\'<>()\[\]\{\}\s\x7F-\xFF]*([.!,?]+[^.!,?;""\'<>\(\)\[\]\{\}\s\x7F-\xFF]+)*)?", "URL ");
            //output = output.Replace("https://", "URL ");
            output = output.Replace("&amp;", "&");
            output = Regex.Replace(output, @"@\w+", "USERNAME ");
            output = Regex.Replace(output, @"(?:\$\w+)*", "");
            output = output.Replace(":", "");
            output = output.Replace(";", "");
            output = output.Replace("#", "");
            output = output.Replace("'s", "");
            output = output.Replace("'", "");
            output = output.Replace("’s", "");
            output = output.Replace("’", "");
            output = output.Replace("‘", "");
            output = output.Replace("RT ", "");
            output = output.Replace("*", "");
            output = output.Replace("~", "");
            output = output.Replace("$", "");
            output = output.Replace("-", "");
            output = output.Replace("/", "");
            output = output.Replace("_", "");
            output = output.Replace("|", "");
            output = output.Replace("POSNUM POSNUM", " POSNUM ");
            output = output.Replace("POSNUM NEGNUM", " POSNUM ");
            output = Regex.Replace(output, "\\s+", " ");

            return output;
        }
    }
}
