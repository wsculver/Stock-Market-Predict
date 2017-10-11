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


namespace TwitterAPIIntegration
{
    static class Program
    {
        //Login to MySql
        public static string MySQLConnectionString = "SERVER=localhost;DATABASE=srs;UID=root;PASSWORD=password;";

        //Get MySql table
        private static List<string>[] LoadData(string TableName, string column2, string column3)
        {
            List<string>[] list = new List<string>[3];
            list[0] = new List<string>();
            list[1] = new List<string>();
            list[2] = new List<string>();

            MySqlConnection connection = new MySqlConnection(MySQLConnectionString);
            connection.Open();
            
            string query = "SELECT * FROM srs." + TableName;
            MySqlCommand cmd = new MySqlCommand(query, connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            while(dataReader.Read())
            {
                list[0].Add(dataReader["id"] + "");
                list[1].Add(dataReader[column2] + "");
                if (column3 != "")
                {
                    list[2].Add(dataReader[column3] + "");
                }
            }

            dataReader.Close();
            connection.Close();

            return list;
        }

        private static void AddData(string TableName, string column, string contents)
        {
            MySqlConnection connection = new MySqlConnection(MySQLConnectionString);
            connection.Open();

            string query = "INSERT INTO `srs`.'" + TableName + " ('" + column + "') " + "VALUES('" + contents + "')";
            MySqlCommand cmd = new MySqlCommand(query, connection);

            connection.Close();
        }

        private static void CreateTable(string TableName, string column1, string type1, string column2, string type2, string column3, string type3)
        {
            MySqlConnection connection = new MySqlConnection(MySQLConnectionString);
            connection.Open();

            string query = "CREATE TABLE 'srs'.'" + TableName + "' ('" + column1 + "' " + type1 + " NOT NULL AUTO_INCREMENT,'" + column2 + "' " + type2 + " NOT NULL,'" + column3 + "' " + type3 + " NOT NULL,PRIMARY KEY('" + column1 + "'))";
            MySqlCommand cmd = new MySqlCommand(query, connection);

            connection.Close();
        }

        //Old code that needs to be updated to use MySql tables instead of files
#if true
        //public static string[] keyWords = new string[] { "Texas", "United States" };
        /*public static string StockSymbols = "C:\\Users\\William\\Desktop\\SRS 2017-2018\\Read Twitter Example\\S&P 500 Symbols.txt";
        public static string StockNames = "C:\\Users\\William\\Desktop\\SRS 2017-2018\\Read Twitter Example\\S&P 500 Names.txt";
        public static string TweetsFile = "C:\\Users\\William\\Desktop\\SRS 2017-2018\\Read Twitter Example\\TweetsList.txt";
        public static string PrevTweets = "C:\\Users\\William\\Desktop\\SRS 2017-2018\\Read Twitter Example\\PrevTweets.txt";
        public static string AllTweetWords = "C:\\Users\\William\\Desktop\\SRS 2017-2018\\Read Twitter Example\\All Tweet Words.txt";
        public static string CompanyFiles = "C:\\Users\\William\\Desktop\\SRS 2017-2018\\Read Twitter Example\\Company Files";*/

        private static void Main(string[] args)
        {
            Console.WriteLine("I think I can, I think I can, I think I can...");
            var tweetList = GetTwitterFeeds();
            /*string FileVariable;
            string FileVariable2;
            string FileVariable3;*/
            string CompanyVariable;
            string CompanyVariable2;
            string CompanyVariable3;
            bool FileVarSet = false;
            bool FileVar2Set = false;
            bool FileVar3Set = false;

            Console.WriteLine("Tweets Count " + tweetList.Count); // Make sure to change the path according to your system
            //File.Delete(TweetsFile);
            //StreamWriter sw = new StreamWriter(TweetsFile, true);
            //StreamWriter swpt = new StreamWriter(PrevTweets, true);

            foreach (var item in tweetList)
            {
                //swpt.Write(item.Text);

                AddData("previous_tweets", "tweet", item.Text);
                Console.WriteLine("tweet added to previous tweets");

                item.Text = ReplaceChars(item.Text);

                //item.Text = item.Text.ToLower();
                /*for(int i = 0; i < File.ReadAllLines(StockNames).Length; i ++)
                {
                    if(item.Text.Contains(File.ReadAllLines(StockNames)[i]))                     // Debug if unknown tweet included
                    {
                        sw.WriteLine(item.Text + " " + i);
                    }
                }*/

                for (int i = 0; i < LoadData("all_companies", "symbol", "name")[1].Count; i++)
                {
                    //if ((item.Text.Contains((File.ReadAllLines(StockSymbols)[i] + " "), StringComparison.OrdinalIgnoreCase) || item.Text.Contains((File.ReadAllLines(StockNames)[i] + " "), StringComparison.OrdinalIgnoreCase)) && !FileVarSet)
                    if ((item.Text.Contains(((LoadData("all_companies", "symbol", "name")[1])[i] + " "), StringComparison.OrdinalIgnoreCase) || item.Text.Contains((LoadData("all_companies", "symbol", "name")[1])[i], StringComparison.OrdinalIgnoreCase)) && !FileVarSet)
                    {
                        //FileVariable = File.ReadAllLines(StockSymbols)[i];
                        CompanyVariable = (LoadData("all_companies", "symbol", "name")[1])[i];

                        FileVarSet = true;
                        //item.Text = item.Text.Replace(File.ReadAllLines(StockSymbols)[i], "");
                        //item.Text = item.Text.Replace(File.ReadAllLines(StockNames)[i], "");

                        /*item.Text = Regex.Replace(item.Text, @"@\w+", "USERNAME ");
                        item.Text = Regex.Replace(item.Text, @"https?://[-\w]+(\.\w[-\w]*)+(:\d+)?(/[^.!,?;""\'<>()\[\]\{\}\s\x7F-\xFF]*([.!,?]+[^.!,?;""\'<>\(\)\[\]\{\}\s\x7F-\xFF]+)*)?", "URL ");
                        item.Text = item.Text.Replace("https://", "URL ");*/

                        //StreamWriter sw1 = new StreamWriter(CompanyFiles + "\\" + FileVariable + ".train", true);

                        CreateTable(CompanyVariable, "ID", "INT", "Sentiment", "INT", "Words", "VarChar(1000)");
                        Console.WriteLine("new table created");

                        //sw1.Write("+1 ");

                        AddData(CompanyVariable, "Sentiment", "+1");
                        //File.WriteAllText(CompanyFiles + "\\" + FileVariable + ".train", "yes");
                        //int test = TweetsFile.IndexOf("hi", 0, StringComparison.CurrentCultureIgnoreCase);

                        //for (int ii = 1; ii <= File.ReadAllLines(AllTweetWords).Length; ii++)
                        for (int ii = 1; ii <= LoadData("all_tweet_words", "word", "")[1].Count; ii++)
                        {
                            //if (item.Text.Contains((File.ReadAllLines(AllTweetWords)[ii - 1] + " "), StringComparison.OrdinalIgnoreCase))
                            if (item.Text.Contains(((LoadData("all_tweet_words", "word", "")[1])[ii - 1] + " "), StringComparison.OrdinalIgnoreCase))
                            {
                                //sw1.Write(ii + ":1 ");
                                AddData(CompanyVariable, "Words", ii + ":1 ");
                            }
                            else
                            {
                                //sw1.Write(ii + ":0 ");
                                AddData(CompanyVariable, "Words", ii + ":0 ");
                            }
                        }
                        Console.WriteLine("done with tweet company table 1");
                        //sw1.WriteLine("");
                        //sw1.Close();
                    }
                    //else if ((item.Text.Contains((File.ReadAllLines(StockSymbols)[i] + " "), StringComparison.OrdinalIgnoreCase) || item.Text.Contains((File.ReadAllLines(StockNames)[i] + " "), StringComparison.OrdinalIgnoreCase)) && !FileVar2Set)
                    else if ((item.Text.Contains(((LoadData("all_companies", "symbol", "name")[1])[i] + " "), StringComparison.OrdinalIgnoreCase) || item.Text.Contains((LoadData("all_companies", "symbol", "name")[1])[i], StringComparison.OrdinalIgnoreCase)) && !FileVar2Set)
                    {
                        //FileVariable2 = File.ReadAllLines(StockSymbols)[i];
                        CompanyVariable2 = (LoadData("all_companies", "symbol", "name")[1])[i];
                        FileVar2Set = true;
                        //item.Text = item.Text.Replace(File.ReadAllLines(StockSymbols)[i], "");
                        //item.Text = item.Text.Replace(File.ReadAllLines(StockNames)[i], "");

                        /*item.Text = Regex.Replace(item.Text, @"@\w+", "USERNAME");
                        item.Text = Regex.Replace(item.Text, @"https?://[-\w]+(\.\w[-\w]*)+(:\d+)?(/[^.!,?;""\'<>()\[\]\{\}\s\x7F-\xFF]*([.!,?]+[^.!,?;""\'<>\(\)\[\]\{\}\s\x7F-\xFF]+)*)?", "URL");
                        item.Text = item.Text.Replace("https://", "URL");*/

                        //StreamWriter sw1 = new StreamWriter(CompanyFiles + "\\" + FileVariable2 + ".train", true);

                        CreateTable(CompanyVariable2, "ID", "INT", "Sentiment", "INT", "Words", "VarChar(1000)");
                        Console.WriteLine("new table 2 created");

                        //sw1.Write("+1 ");

                        //for (int ii = 1; ii <= File.ReadAllLines(AllTweetWords).Length; ii++)
                        for (int ii = 1; ii <= LoadData("all_tweet_words", "word", "")[1].Count; ii++)
                        {
                            //if (item.Text.Contains((File.ReadAllLines(AllTweetWords)[ii - 1] + " "), StringComparison.OrdinalIgnoreCase))
                            if (item.Text.Contains(((LoadData("all_tweet_words", "word", "")[1])[ii - 1] + " "), StringComparison.OrdinalIgnoreCase))
                            {
                                //sw1.Write(ii + ":1 ");
                                AddData(CompanyVariable2, "Words", ii + ":1 ");
                            }
                            else
                            {
                                //sw1.Write(ii + ":0 ");
                                AddData(CompanyVariable2, "Words", ii + ":0 ");
                            }
                        }
                        Console.WriteLine("done with tweet company table 2");
                        //sw1.WriteLine("");
                        //sw1.Close();
                    }
                    //else if ((item.Text.Contains((File.ReadAllLines(StockSymbols)[i] + " "), StringComparison.OrdinalIgnoreCase) || item.Text.Contains((File.ReadAllLines(StockNames)[i] + " "), StringComparison.OrdinalIgnoreCase)) && !FileVar3Set)
                    else if ((item.Text.Contains(((LoadData("all_companies", "symbol", "name")[1])[i] + " "), StringComparison.OrdinalIgnoreCase) || item.Text.Contains((LoadData("all_companies", "symbol", "name")[1])[i], StringComparison.OrdinalIgnoreCase)) && !FileVar3Set)
                    {
                        //FileVariable3 = File.ReadAllLines(StockSymbols)[i];
                        CompanyVariable3 = (LoadData("all_companies", "symbol", "name")[1])[i];
                        FileVar3Set = true;
                        //item.Text = item.Text.Replace(File.ReadAllLines(StockSymbols)[i], "");
                        //item.Text = item.Text.Replace(File.ReadAllLines(StockNames)[i], "");

                        /*item.Text = Regex.Replace(item.Text, @"@\w+", "USERNAME");
                        item.Text = Regex.Replace(item.Text, @"https?://[-\w]+(\.\w[-\w]*)+(:\d+)?(/[^.!,?;""\'<>()\[\]\{\}\s\x7F-\xFF]*([.!,?]+[^.!,?;""\'<>\(\)\[\]\{\}\s\x7F-\xFF]+)*)?", "URL");
                        item.Text = item.Text.Replace("https://", "URL");*/

                        //StreamWriter sw1 = new StreamWriter(CompanyFiles + "\\" + FileVariable3 + ".train", true);

                        CreateTable(CompanyVariable3, "ID", "INT", "Sentiment", "INT", "Words", "VarChar(1000)");
                        Console.WriteLine("new table 3 created");

                        //sw1.Write("+1 ");

                        //for (int ii = 1; ii <= File.ReadAllLines(AllTweetWords).Length; ii++)
                        for (int ii = 1; ii <= LoadData("all_tweet_words", "word", "")[1].Count; ii++)
                        {
                            //if (item.Text.Contains((File.ReadAllLines(AllTweetWords)[ii - 1] + " "), StringComparison.OrdinalIgnoreCase))
                            if (item.Text.Contains(((LoadData("all_tweet_words", "word", "")[1])[ii - 1] + " "), StringComparison.OrdinalIgnoreCase))
                            {
                                //sw1.Write(ii + ":1 ");
                                AddData(CompanyVariable3, "Words", ii + ":1 ");
                            }
                            else
                            {
                                //sw1.Write(ii + ":0 ");
                                AddData(CompanyVariable3, "Words", ii + ":0 ");
                            }
                        }
                        Console.WriteLine("done with company table 3");
                        //sw1.WriteLine("");
                        //sw1.Close();
                    }
                }
                FileVarSet = false;
                FileVar2Set = false;
                FileVar3Set = false;

                //sw.WriteLine(item.Text);
                AddData("edited_tweets", "tweet", item.Text);
            }
            //sw.Close();

            //File.WriteAllText(file, File.ReadAllText(file).Replace("RT @", ""));
            Console.WriteLine("I made it!!! Phew.");
            Console.ReadLine();
        }

        public static List<Status> GetTwitterFeeds()
        {
            //string screenname = "realdonaldtrump";
            string screenname = "WSJ";

            var auth = new SingleUserAuthorizer
            {
                CredentialStore = new InMemoryCredentialStore()
                {
                    // change your AppSetting keys according to your app.
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
                                    //&& tweet.ScreenName == screenname
                                    && tweet.Count == 200
                                    && (DateTime)tweet.CreatedAt >= DateTime.Today
                                    && LoadData("all_companies", "symbol", "name")[1].Any(w => tweet.Text.Contains(w)) || LoadData("all_companies", "symbol", "name")[2].Any(w => tweet.Text.Contains(w))
                                    //&& (File.ReadAllLines(StockSymbols).Any(tweet.Text.Contains) || File.ReadAllLines(StockNames).Any(tweet.Text.Contains))
                                    //&& keyWords.Any(tweet.Text.Contains)
                                    //&& tweet.Text.Contains("lockheed")
                                    //&& !File.ReadAllText(PrevTweets).Contains(tweet.Text)
                                    //&& !tweet.Text.Any(File.ReadAllText(PrevTweets).Contains)
                                    && !LoadData("previous_tweets", "tweet", "")[1].Any(w => tweet.Text.Contains(w))
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
                                            //&& tweet.ScreenName == screenname
                                            && tweet.MaxID == maxId
                                            && tweet.Count == 200
                                            && (DateTime)tweet.CreatedAt >= DateTime.Today
                                            && LoadData("all_companies", "symbol", "name")[1].Any(w => tweet.Text.Contains(w)) || LoadData("all_companies", "symbol", "name")[2].Any(w => tweet.Text.Contains(w))
                                            //&& (File.ReadAllLines(StockSymbols).Any(tweet.Text.Contains) || File.ReadAllLines(StockNames).Any(tweet.Text.Contains))
                                            //&& keyWords.Any(tweet.Text.Contains)
                                            //&& tweet.Text.Contains("lockheed")
                                            //&& !File.ReadAllText(PrevTweets).Contains(tweet.Text)
                                            //&& !tweet.Text.Any(File.ReadAllText(PrevTweets).Contains)
                                            && !LoadData("previous_tweets", "tweet", "")[1].Any(w => tweet.Text.Contains(w))
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

        public static bool Contains(this string target, string value, StringComparison comparison)
        {
            return target.IndexOf(value, comparison) >= 0;
        }

        public static string ReplaceChars(string input)
        {
            string output = Regex.Replace(input, @"@\w+", "USERNAME ");
            output = Regex.Replace(output, @"https?://[-\w]+(\.\w[-\w]*)+(:\d+)?(/[^.!,?;""\'<>()\[\]\{\}\s\x7F-\xFF]*([.!,?]+[^.!,?;""\'<>\(\)\[\]\{\}\s\x7F-\xFF]+)*)?", "URL ");
            output = output.Replace("https://", "URL ");
            output = output.Replace(":", "");
            output = output.Replace("#", "");
            output = output.Replace("…", "");
            output = output.Replace("’s", "");
            output = output.Replace("’", "");
            output = output.Replace("‘", "");
            output = output.Replace("'s", "");
            output = output.Replace("'", "");
            output = output.Replace("RT ", "");
            output = output.Replace(",", "");
            output = output.Replace("?", "");
            output = output.Replace("!", "");
            output = output.Replace(".", "");
            output = output.Replace(";", "");
            output = Regex.Replace(output, "\\s+", " ");

            return output;
        }
#endif

    }
}
