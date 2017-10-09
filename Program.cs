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
        public static string MySQLConnectionString = "SERVER=localhost;DATABASE=srs;UID=root;PASSWORD=password;";

        private static List<string>[] LoadData(string TableName, string column2, string column3, bool id)
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
                if (id)
                {
                    list[0].Add(dataReader["id"] + "");
                }
                else
                {
                    list[0].Add("");
                }
                list[1].Add(dataReader[column2] + "");
                list[2].Add(dataReader[column3] + "");
            }

            dataReader.Close();
            connection.Close();

            return list;
        }

#if false
        //public static string[] keyWords = new string[] { "Texas", "United States" };
        public static string StockSymbols = "C:\\Users\\William\\Desktop\\SRS 2017-2018\\Read Twitter Example\\S&P 500 Symbols.txt";
        public static string StockNames = "C:\\Users\\William\\Desktop\\SRS 2017-2018\\Read Twitter Example\\S&P 500 Names.txt";
        public static string TweetsFile = "C:\\Users\\William\\Desktop\\SRS 2017-2018\\Read Twitter Example\\TweetsList.txt";
        public static string PrevTweets = "C:\\Users\\William\\Desktop\\SRS 2017-2018\\Read Twitter Example\\PrevTweets.txt";
        public static string AllTweetWords = "C:\\Users\\William\\Desktop\\SRS 2017-2018\\Read Twitter Example\\All Tweet Words.txt";
        public static string CompanyFiles = "C:\\Users\\William\\Desktop\\SRS 2017-2018\\Read Twitter Example\\Company Files";

        private static void Main(string[] args)
        {
            Console.WriteLine("I think I can, I think I can, I think I can...");
            var tweetList = GetTwitterFeeds();
            string FileVariable;
            string FileVariable2;
            string FileVariable3;
            bool FileVarSet = false;
            bool FileVar2Set = false;
            bool FileVar3Set = false;

            Console.WriteLine("Tweets Count " + tweetList.Count); // Make sure to change the path according to your system
            //File.Delete(TweetsFile);
            StreamWriter sw = new StreamWriter(TweetsFile, true);
            StreamWriter swpt = new StreamWriter(PrevTweets, true);

            foreach (var item in tweetList)
            {
                swpt.Write(item.Text);

                item.Text = ReplaceChars(item.Text);

                //item.Text = item.Text.ToLower();
                /*for(int i = 0; i < File.ReadAllLines(StockNames).Length; i ++)
                {
                    if(item.Text.Contains(File.ReadAllLines(StockNames)[i]))                     // Debug if unknown tweet included
                    {
                        sw.WriteLine(item.Text + " " + i);
                    }
                }*/

                for (int i = 0; i < File.ReadAllLines(StockSymbols).Length; i++)
                {
                    if ((item.Text.Contains((File.ReadAllLines(StockSymbols)[i] + " "), StringComparison.OrdinalIgnoreCase) || item.Text.Contains((File.ReadAllLines(StockNames)[i] + " "), StringComparison.OrdinalIgnoreCase)) && !FileVarSet)
                    {
                        FileVariable = File.ReadAllLines(StockSymbols)[i];
                        FileVarSet = true;
                        //item.Text = item.Text.Replace(File.ReadAllLines(StockSymbols)[i], "");
                        //item.Text = item.Text.Replace(File.ReadAllLines(StockNames)[i], "");

                        /*item.Text = Regex.Replace(item.Text, @"@\w+", "USERNAME ");
                        item.Text = Regex.Replace(item.Text, @"https?://[-\w]+(\.\w[-\w]*)+(:\d+)?(/[^.!,?;""\'<>()\[\]\{\}\s\x7F-\xFF]*([.!,?]+[^.!,?;""\'<>\(\)\[\]\{\}\s\x7F-\xFF]+)*)?", "URL ");
                        item.Text = item.Text.Replace("https://", "URL ");*/

                        StreamWriter sw1 = new StreamWriter(CompanyFiles + "\\" + FileVariable + ".train", true);
                        sw1.Write("+1 ");
                        //File.WriteAllText(CompanyFiles + "\\" + FileVariable + ".train", "yes");
                        //int test = TweetsFile.IndexOf("hi", 0, StringComparison.CurrentCultureIgnoreCase);

                        for (int ii = 1; ii <= File.ReadAllLines(AllTweetWords).Length; ii++)
                        {
                            if (item.Text.Contains((File.ReadAllLines(AllTweetWords)[ii - 1] + " "), StringComparison.OrdinalIgnoreCase))
                            {
                                sw1.Write(ii + ":1 ");
                            }
                            else
                            {
                                sw1.Write(ii + ":0 ");
                            }
                        }
                        sw1.WriteLine("");
                        sw1.Close();
                    }
                    else if ((item.Text.Contains((File.ReadAllLines(StockSymbols)[i] + " "), StringComparison.OrdinalIgnoreCase) || item.Text.Contains((File.ReadAllLines(StockNames)[i] + " "), StringComparison.OrdinalIgnoreCase)) && !FileVar2Set)
                    {
                        FileVariable2 = File.ReadAllLines(StockSymbols)[i];
                        FileVar2Set = true;
                        //item.Text = item.Text.Replace(File.ReadAllLines(StockSymbols)[i], "");
                        //item.Text = item.Text.Replace(File.ReadAllLines(StockNames)[i], "");

                        /*item.Text = Regex.Replace(item.Text, @"@\w+", "USERNAME");
                        item.Text = Regex.Replace(item.Text, @"https?://[-\w]+(\.\w[-\w]*)+(:\d+)?(/[^.!,?;""\'<>()\[\]\{\}\s\x7F-\xFF]*([.!,?]+[^.!,?;""\'<>\(\)\[\]\{\}\s\x7F-\xFF]+)*)?", "URL");
                        item.Text = item.Text.Replace("https://", "URL");*/

                        StreamWriter sw1 = new StreamWriter(CompanyFiles + "\\" + FileVariable2 + ".train", true);
                        sw1.Write("+1 ");

                        for (int ii = 1; ii <= File.ReadAllLines(AllTweetWords).Length; ii++)
                        {
                            if (item.Text.Contains((File.ReadAllLines(AllTweetWords)[ii - 1] + " "), StringComparison.OrdinalIgnoreCase))
                            {
                                sw1.Write(ii + ":1 ");
                            }
                            else
                            {
                                sw1.Write(ii + ":0 ");
                            }
                        }
                        sw1.WriteLine("");
                        sw1.Close();
                    }
                    else if ((item.Text.Contains((File.ReadAllLines(StockSymbols)[i] + " "), StringComparison.OrdinalIgnoreCase) || item.Text.Contains((File.ReadAllLines(StockNames)[i] + " "), StringComparison.OrdinalIgnoreCase)) && !FileVar3Set)
                    {
                        FileVariable3 = File.ReadAllLines(StockSymbols)[i];
                        FileVar3Set = true;
                        //item.Text = item.Text.Replace(File.ReadAllLines(StockSymbols)[i], "");
                        //item.Text = item.Text.Replace(File.ReadAllLines(StockNames)[i], "");

                        /*item.Text = Regex.Replace(item.Text, @"@\w+", "USERNAME");
                        item.Text = Regex.Replace(item.Text, @"https?://[-\w]+(\.\w[-\w]*)+(:\d+)?(/[^.!,?;""\'<>()\[\]\{\}\s\x7F-\xFF]*([.!,?]+[^.!,?;""\'<>\(\)\[\]\{\}\s\x7F-\xFF]+)*)?", "URL");
                        item.Text = item.Text.Replace("https://", "URL");*/

                        StreamWriter sw1 = new StreamWriter(CompanyFiles + "\\" + FileVariable3 + ".train", true);
                        sw1.Write("+1 ");

                        for (int ii = 1; ii <= File.ReadAllLines(AllTweetWords).Length; ii++)
                        {
                            if (item.Text.Contains((File.ReadAllLines(AllTweetWords)[ii - 1] + " "), StringComparison.OrdinalIgnoreCase))
                            {
                                sw1.Write(ii + ":1 ");
                            }
                            else
                            {
                                sw1.Write(ii + ":0 ");
                            }
                        }
                        sw1.WriteLine("");
                        sw1.Close();
                    }
                }
                FileVarSet = false;
                FileVar2Set = false;
                FileVar3Set = false;

                sw.WriteLine(item.Text);
            }
            sw.Close();

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
                                    && (File.ReadAllLines(StockSymbols).Any(tweet.Text.Contains) || File.ReadAllLines(StockNames).Any(tweet.Text.Contains))
                                    //&& keyWords.Any(tweet.Text.Contains)
                                    //&& tweet.Text.Contains("lockheed")
                                    //&& !File.ReadAllText(PrevTweets).Contains(tweet.Text)
                                    && !tweet.Text.Any(File.ReadAllText(PrevTweets).Contains)
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
                                            && (File.ReadAllLines(StockSymbols).Any(tweet.Text.Contains) || File.ReadAllLines(StockNames).Any(tweet.Text.Contains))
                                            //&& keyWords.Any(tweet.Text.Contains)
                                            //&& tweet.Text.Contains("lockheed")
                                            //&& !File.ReadAllText(PrevTweets).Contains(tweet.Text)
                                            && !tweet.Text.Any(File.ReadAllText(PrevTweets).Contains)
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
