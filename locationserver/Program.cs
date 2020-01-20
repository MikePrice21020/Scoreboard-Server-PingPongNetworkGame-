using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace Server
{
    class Serverlocation
    {
        //http://stackoverflow.com/questions/27540705/c-sharp-udp-can-not-receive
        //Static port and address for precuation
        static string log;
        static int port = 43;

        //Static Dictionary and Parse string "ipAddress" to IPAddress
        static Dictionary<string, int> multiplayerDatabase = new Dictionary<string, int>();
        static Dictionary<string, int> singleplayerDatabase = new Dictionary<string, int>();
        public static void Main(string[] args)
        {
            Console.WriteLine("################## \t SERVER Leaderboard \t ##################");
            try
            {
                DebugFill();
                runServer();
            }
            catch (IOException i)
            {
                Console.WriteLine(i);
            }
        }
        public static void runServer()
        {
            //Network
            TcpListener listener;
            Socket connection;
            try
            {
                //Intialise TcpListener
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                while (true)
                {
                    connection = listener.AcceptSocket();
                    //Parameterized Thread - DoRequest
                    Thread r = new Thread(new ParameterizedThreadStart(doRequest));
                    r.Start(connection);
                    //doRequest(socketStream);

                }
            }
            catch (Exception e)
            {
                log = (e.ToString());
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: {0}", log);
                Console.ResetColor();
            }
        }
        public static void DebugFill()
        {
            multiplayerDatabase.Add("GITGUD", 5);
            multiplayerDatabase.Add("DATGUY", 4);
            multiplayerDatabase.Add("JAMES", 3);
            multiplayerDatabase.Add("THOMAS", 2);
            multiplayerDatabase.Add("ICRY", 1);

            singleplayerDatabase.Add("AIEZ", 5);
            singleplayerDatabase.Add("IWON", 4);
            singleplayerDatabase.Add("MICHAEL", 3);
            singleplayerDatabase.Add("HELPME", 2);
            singleplayerDatabase.Add("PLSNO", 1);
        }
        private static void doRequest(object ded)
        {
            //Network
            Socket connection = (Socket)ded;
            NetworkStream socketStream;
            socketStream = new NetworkStream(connection);

            //Variables used for reading sr.ReadLine
            string Full_Client_msg = null;
            string[] Client_msg_final;
            Client_msg_final = new string[2];

            //Steam Writer/Reader
            StreamWriter sw = new StreamWriter(socketStream);
            StreamReader sr = new StreamReader(socketStream);

            try
            {
                //Timeout code for read and write
                socketStream.ReadTimeout = 1000;
                socketStream.WriteTimeout = 1000;

                //Read Stream with sr.Peek
                while (sr.Peek() >= 0)
                {
                    Full_Client_msg += (char)sr.Read();
                }
                string[] Client_msg = Regex.Split(Full_Client_msg, "\r\n");


                //MULTIPLAYER SECTION!
                if(Client_msg[0] == "@multi")
                {
                    //RETRIEVE database
                    if (Client_msg[1] == "@retrieve" && Client_msg.Length == 3)
                    {
                        RetrieveDatabase(true, sw, sr);
                    }

                    //UPDATE
                    else if (Client_msg[1] == "@update" && Client_msg.Length == 5)
                    {
                        Update(true, Client_msg[2], Client_msg[3], sw, sr);
                        RetrieveDatabase(true, sw, sr);
                    }

                    //CHECK database for highscore
                    else if (Client_msg[1] == "@check" && Client_msg.Length == 4)
                    {
                        CheckDatabase(true, Client_msg[2], sw, sr);
                    }
                    else
                    {
                        Console.WriteLine("Invalid Input");
                    }
                }

                //SINGLEPLAYER SECTION!
                else if(Client_msg[0] == "@single")
                {
                    //RETRIEVE database
                    if (Client_msg[1] == "@retrieve" && Client_msg.Length == 3)
                    {
                        RetrieveDatabase(false, sw, sr);
                    }

                    //UPDATE
                    else if (Client_msg[1] == "@update" && Client_msg.Length == 5)
                    {
                        Update(false, Client_msg[2], Client_msg[3], sw, sr);
                        RetrieveDatabase(false, sw, sr);
                    }

                    //CHECK database for highscore
                    else if (Client_msg[1] == "@check" && Client_msg.Length == 4)
                    {
                        CheckDatabase(false, Client_msg[2], sw, sr);
                    }
                    else
                    {
                        Console.WriteLine("Invalid Input");
                    }
                }


                //IF NOTHING HITS
                else
                {
                    Console.WriteLine("Invalid Input");
                }
                socketStream.Close();
                connection.Close();
            }
            catch (Exception e)
            {
                log = (e.ToString());
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: {0}", log);
                Console.ResetColor();
            }
        }
        public static void Update(bool multi, string playerName, string PlayerScore, StreamWriter sw, StreamReader sr)
        {
            int PlayerScoreInt = 0;
            int ComparedPlayerScoreInt;
            bool playerHasHighscore = false;
            try
            {
                PlayerScoreInt = Convert.ToInt32(PlayerScore);
                //search User
                string[,] playerScores = new string[6, 2];
                int x = 0;
                int SamePlayer = 1;
                bool nameFound = false;
                bool nameFoundMulti = false;
                string temPlayerName = playerName;
                if(multi == true)
                {
                    foreach (KeyValuePair<string, int> entry in multiplayerDatabase)
                    {
                        if (entry.Key == playerName)
                        {
                            nameFound = true;
                        }
                        for (int i = 1; i < 6; i++)
                        {
                            if (entry.Key == (temPlayerName += "(" + i + ")"))
                            {
                                SamePlayer++;
                                nameFoundMulti = true;
                                nameFound = false;
                            }
                            temPlayerName = playerName;
                        }

                    }
                }
                else
                {
                    foreach (KeyValuePair<string, int> entry in singleplayerDatabase)
                    {
                        if (entry.Key == playerName)
                        {
                            nameFound = true;
                        }
                        for (int i = 1; i < 6; i++)
                        {
                            if (entry.Key == (temPlayerName += "(" + i + ")"))
                            {
                                SamePlayer++;
                                nameFoundMulti = true;
                                nameFound = false;
                            }
                            temPlayerName = playerName;
                        }

                    }
                }
                if (nameFound == true && nameFoundMulti == false)
                {
                    playerName = playerName += "(1)";
                }
                if (nameFoundMulti == true)
                {
                    playerName = playerName += "(" + SamePlayer + ")";
                }
                if(multi == true)
                {
                    foreach (KeyValuePair<string, int> entry in multiplayerDatabase)
                    {
                        ComparedPlayerScoreInt = Convert.ToInt32(entry.Value);
                        if (PlayerScoreInt > ComparedPlayerScoreInt && playerHasHighscore == false)
                        {
                            playerScores[x, 0] = playerName;
                            playerScores[x, 1] = PlayerScore;

                            playerScores[x + 1, 0] = entry.Key;
                            playerScores[x + 1, 1] = entry.Value.ToString();
                            playerHasHighscore = true;
                            x++;
                        }
                        else
                        {
                            playerScores[x, 0] = entry.Key;
                            playerScores[x, 1] = entry.Value.ToString();
                        }
                        // key - value
                        // do something with entry.Value or entry.Key;
                        x++;
                    }
                }
                else
                {
                    foreach (KeyValuePair<string, int> entry in singleplayerDatabase)
                    {
                        ComparedPlayerScoreInt = Convert.ToInt32(entry.Value);
                        if (PlayerScoreInt > ComparedPlayerScoreInt && playerHasHighscore == false)
                        {
                            playerScores[x, 0] = playerName;
                            playerScores[x, 1] = PlayerScore;

                            playerScores[x + 1, 0] = entry.Key;
                            playerScores[x + 1, 1] = entry.Value.ToString();
                            playerHasHighscore = true;
                            x++;
                        }
                        else
                        {
                            playerScores[x, 0] = entry.Key;
                            playerScores[x, 1] = entry.Value.ToString();
                        }
                        // key - value
                        // do something with entry.Value or entry.Key;
                        x++;
                    }
                }
                if (playerHasHighscore == true)
                {
                    Dictionary<string, int> temDatabase = new Dictionary<string, int>();
                    for (int i = 0; i < 5; i++)
                    {
                        ComparedPlayerScoreInt = Convert.ToInt32(playerScores[i, 1]);
                        temDatabase.Add(playerScores[i, 0], ComparedPlayerScoreInt);
                    }
                    if(multi == true)
                    {
                        multiplayerDatabase = temDatabase;
                    }
                    else
                    {
                        singleplayerDatabase = temDatabase;
                    }
                }
                /*database.TryGetValue(entry.Key, out comparedPlayerScore);
                sw.WriteLine(comparedPlayerScore);
                database.Add(playerName, PlayerScore);
                sw.Flush();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Server - WHOIS - User {0} Exists and his location is: {1}", playerName, playerScore);
                Console.ResetColor();
                */
            }
            catch (Exception e)
            {
                log = (e.ToString());
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: {0}", log);
                Console.ResetColor();
            }
        }
        public static void RetrieveDatabase(bool multi, StreamWriter sw, StreamReader sr)
        {
            Console.Clear();
            try
            {
                //search User
                string[,] playerScores = new string[6, 2];
                int x = 0;
                if (multi == true)
                {
                    foreach (KeyValuePair<string, int> entry in multiplayerDatabase)
                    {
                        playerScores[x, 0] = entry.Key;
                        playerScores[x, 1] = entry.Value.ToString();
                        x++;
                    }
                }
                else
                {
                    foreach (KeyValuePair<string, int> entry in singleplayerDatabase)
                    {
                        playerScores[x, 0] = entry.Key;
                        playerScores[x, 1] = entry.Value.ToString();
                        x++;
                    }
                }
                sw.WriteLine("{0}\r\n{1}\r\n{2}\r\n{3}\r\n{4}\r\n{5}\r\n{6}\r\n{7}\r\n{8}\r\n{9}", playerScores[0, 0], playerScores[0, 1], playerScores[1, 0], playerScores[1, 1], playerScores[2, 0], playerScores[2, 1], playerScores[3, 0], playerScores[3, 1], playerScores[4, 0], playerScores[4, 1]);
                sw.Flush();
                Console.ForegroundColor = ConsoleColor.Green;
                if (multi == true)
                {
                    Console.WriteLine("Multiplayer Leaderboards");
                    Console.WriteLine("========================");
                }
                else
                {
                    Console.WriteLine("Singleplayer Leaderboards");
                    Console.WriteLine("========================");
                }
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Top 5 Highscores:");
                Console.WriteLine(" ");
                Console.WriteLine("1. {0} {1}\r\n2. {2} {3}\r\n3. {4} {5}\r\n4. {6} {7}\r\n5. {8} {9}", playerScores[0, 0], playerScores[0, 1], playerScores[1, 0], playerScores[1, 1], playerScores[2, 0], playerScores[2, 1], playerScores[3, 0], playerScores[3, 1], playerScores[4, 0], playerScores[4, 1]);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("========================");
                Console.ResetColor();
            }
            catch (Exception e)
            {
                log = (e.ToString());
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: {0}", log);
                Console.ResetColor();
            }
        }
        public static void CheckDatabase(bool multi, string PlayerScore, StreamWriter sw, StreamReader sr)
        {
            int PlayerScoreInt = 0;
            int ComparedPlayerScoreInt;
            bool playerHasHighscore = false;
            try
            {
                PlayerScoreInt = Convert.ToInt32(PlayerScore);
                //search User
                if (multi == true)
                {
                    foreach (KeyValuePair<string, int> entry in multiplayerDatabase)
                    {
                        ComparedPlayerScoreInt = Convert.ToInt32(entry.Value);
                        if (PlayerScoreInt > ComparedPlayerScoreInt)
                        {
                            playerHasHighscore = true;
                        }
                        // key - value
                        // do something with entry.Value or entry.Key;
                    }
                }
                else
                {
                    foreach (KeyValuePair<string, int> entry in singleplayerDatabase)
                    {
                        ComparedPlayerScoreInt = Convert.ToInt32(entry.Value);
                        if (PlayerScoreInt > ComparedPlayerScoreInt)
                        {
                            playerHasHighscore = true;
                        }
                        // key - value
                        // do something with entry.Value or entry.Key;
                    }
                }
                sw.WriteLine("@checkis\r\n{0}", playerHasHighscore);
                sw.Flush();
                Console.WriteLine("Player checked if he has a highscore and its - {0}", playerHasHighscore);
                /*database.TryGetValue(entry.Key, out comparedPlayerScore);
                sw.WriteLine(comparedPlayerScore);
                database.Add(playerName, PlayerScore);
                sw.Flush();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Server - WHOIS - User {0} Exists and his location is: {1}", playerName, playerScore);
                Console.ResetColor();
                */
            }
            catch (Exception e)
            {
                log = (e.ToString());
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: {0}", log);
                Console.ResetColor();
            }
        }
        /*public static void Http11(string[] word, StreamWriter sw, StreamReader sr)
        {
            int loops = 0;
            string playerName = null, word2 = null;
            try
            {

                if (word[0] != null)
                {
                    playerName = word[0];
                    loops++;
                }
                if (word[1] != null)
                {
                    word2 = word[1];
                    loops++;
                }
                if (loops == 2)
                {
                    //update location
                    if (database.ContainsKey(playerName))
                    {
                        string location;
                        database.TryGetValue(playerName, out location);
                        database[playerName] = word2;
                        sw.WriteLine("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n");
                        sw.Flush();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Server - HTTP/1.1 - Updated location of {0} to {1}", playerName, word2);
                        Console.ResetColor();
                    }
                    //else add user + location
                    else
                    {
                        database.Add(playerName, word2);
                        sw.WriteLine("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n");
                        sw.Flush();
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("Server - HTTP/1.1 - Added User: {0} to {1}", playerName, word2);
                        Console.ResetColor();
                    }
                }
                if (loops == 1)
                {
                    //search User
                    if (database.ContainsKey(playerName))
                    {
                        string location;
                        database.TryGetValue(playerName, out location);
                        sw.WriteLine("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n{0}", location);
                        sw.Flush();
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Server - HTTP/1.1 - User {0} Exists and his location is: {1}", playerName, location);
                        Console.ResetColor();
                    }
                    //else respond no user
                    else
                    {
                        sw.WriteLine("HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n");
                        sw.Flush();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Server - HTTP/1.1 - ERROR: no entries found for {0}", playerName);
                        Console.ResetColor();
                    }
                }
                loops = 0;
            }
            catch (Exception e)
            {
                log = (e.ToString());
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: {0}", log);
                Console.ResetColor();
            }
        }*/
    }
}
