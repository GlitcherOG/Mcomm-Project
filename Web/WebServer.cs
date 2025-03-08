﻿using Newtonsoft.Json;
using SSX3_Server.EAClient;
using SSX3_Server.EAServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SSX3_Server.Web
{
    internal class WebServer
    {
        //Cache HTML, JS and CSS Pages
        public static List<WebData> webDatas = new List<WebData>();

        public static void CacheWebsite()
        {
            webDatas = new List<WebData>();
            var Files = Directory.GetFiles(AppContext.BaseDirectory + "\\Web\\", "*" , SearchOption.AllDirectories);

            for (int i = 0; i < Files.Length; i++)
            {
                WebData webData = new WebData();
                webData.FileName = Files[i].Replace(AppContext.BaseDirectory + "\\Web", "");
                webData.FileText = File.ReadAllText(Files[i]).Replace(".html", "");
                webDatas.Add(webData);
            }
        }


        public static void SimpleListenerExample(/*string[] prefixes*/)
        {
            try
            {
                CacheWebsite();

                string[] prefixes = new string[1] { "http://" + EAServerManager.Instance.config.WebpageURL + ":80/" };
                if (EAServerManager.Instance.config.Https)
                {
                    prefixes = new string[2] { "http://" + EAServerManager.Instance.config.WebpageURL + ":80/", "https://" + EAServerManager.Instance.config.WebpageURL + ":443/" };
                }

                // URI prefixes are required,
                // for example "http://contoso.com:8080/index/".
                if (prefixes == null || prefixes.Length == 0)
                    throw new ArgumentException("prefixes");

                // Create a listener.
                HttpListener listener = new HttpListener();
                // Add the prefixes.
                foreach (string s in prefixes)
                {
                    listener.Prefixes.Add(s);
                }
                listener.Start();

                while (true)
                {
                    try
                    {
                        // Note: The GetContext method blocks while waiting for a request.
                        HttpListenerContext context = listener.GetContext();
                        HttpListenerRequest request = context.Request;
                        // Obtain a response object.
                        HttpListenerResponse response = context.Response;
                        // Construct a response.
                        string responseString = WebpageGenerator(context.Request.RawUrl);
                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                        // Get a response stream and write the response to it.

                        //if (request.HttpMethod == "OPTIONS")
                        //{
                        //    response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept, X-Requested-With");
                        //    response.AddHeader("Access-Control-Allow-Methods", "GET, POST");
                        //    response.AddHeader("Access-Control-Max-Age", "1728000");
                        //}

                        //response.AppendHeader("Access-Control-Allow-Origin", "*");

                        response.ContentLength64 = buffer.Length;
                        if (context.Request.RawUrl.ToLower().StartsWith("/api"))
                        {
                            response.ContentType = "application/json";
                        }
                        System.IO.Stream output = response.OutputStream;
                        output.Write(buffer, 0, buffer.Length);
                        // You must close the output stream.
                        output.Close();
                    }
                    catch
                    {
                        ConsoleManager.WriteLine("Error With Request.");
                    }
                }
            }
            catch
            {
                ConsoleManager.WriteLine("Error With Webserver. May need to be run as admin");
            }
        }

        public static string WebpageGenerator(string URL)
        {
            string[] SplitURL = URL.ToLower().Split('/');

            if (SplitCheck(SplitURL,1,"api"))
            {
                return APIReturn(SplitURL);
            }

            for (int i = 0; i < webDatas.Count; i++)
            {
                if (webDatas[i].FileName == URL.Replace("/","\\"))
                {
                    return webDatas[i].FileText;
                }

                if (webDatas[i].FileName.Split(".")[0] == URL.Replace("/", "\\"))
                {
                    return webDatas[i].FileText;
                }
            }

            //string GenPath = Path.GetDirectoryName(AppContext.BaseDirectory + "\\Web\\" + URL);
            //if (GenPath.StartsWith(AppContext.BaseDirectory + "Web"))
            //{
            //    if (File.Exists(AppContext.BaseDirectory + "\\Web\\" + URL))
            //    {
            //        return File.ReadAllText(AppContext.BaseDirectory + "\\Web\\" + URL).Replace(".html","");
            //    }

            //    if (File.Exists(AppContext.BaseDirectory + "\\Web\\" + URL+".html"))
            //    {
            //        return File.ReadAllText(AppContext.BaseDirectory + "\\Web\\" + URL + ".html").Replace(".html", "");
            //    }
            //}

            if (File.Exists(AppContext.BaseDirectory + "\\Web\\index.html") && URL != "/favicon.ico")
            {
                ConsoleManager.WriteLineVerbose("Web Request Generating Page...");
                return File.ReadAllText(AppContext.BaseDirectory + "\\Web\\index.html").Replace(".html", "");
            }

            return "<HTML><BODY>Under Construction, Please Connect using SSX 3 Online</BODY></HTML>";
        }

        public static bool SplitCheck(string[] Split, int Check, string CheckAgainst = "")
        {
            if(Split.Length>=Check-1)
            {
                if (CheckAgainst != "")
                {
                    if (Split[Check] == CheckAgainst)
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        public static string APIReturn(string[] SplitURL)
        {
            if (SplitCheck(SplitURL, 2, "highscore"))
            {
                if (SplitURL.Length >= 4)
                {
                    try
                    {
                        int ID = int.Parse(SplitURL[3]);
                        return EAServerManager.Instance.highscoreDatabase.CreateJsonCourseText(ID);
                    }
                    catch
                    {
                        return "ERROR";
                    }
                }
                else
                {
                    return EAServerManager.Instance.highscoreDatabase.CreateJsonText();
                }
            }

            if (SplitCheck(SplitURL, 2, "data"))
            {
                if(SplitCheck(SplitURL, 3, "version"))
                {
                    for (int i = 0; i < 0; i++)
                    {
                        //List out version
                    }
                }
            }

            if (SplitCheck(SplitURL, 2, "room"))
            {
                List<EAServerRoom.RoomInfo> TempRooms = new List<EAServerRoom.RoomInfo>();

                for (int i = 0; i < EAServerManager.Instance.rooms.Count; i++)
                {
                    //List rooms
                    TempRooms.Add(EAServerManager.Instance.rooms[i].GenerateRoomInfo());
                }

                var serializer = JsonConvert.SerializeObject(TempRooms);
                return serializer.ToString();
            }

            if (SplitCheck(SplitURL, 2, "game"))
            {
                if(SplitURL.Length==4)
                {
                    if (SplitURL[4].Contains("&raw=1"))
                    {
                        string FileName = SplitURL[4].Replace("&raw=1","").Replace("%20", "");
                        if(File.Exists(AppContext.BaseDirectory + "\\Races\\"+FileName))
                        {
                            return File.ReadAllText(AppContext.BaseDirectory + "\\Races\\" + FileName);
                        }
                        return "No File Found";
                    }
                }
            }

            if (SplitCheck(SplitURL, 2, "online"))
            {
                var PersonaList = new List<EAClientManager.OnlinePersonaInfo>();
                //Return Persona Info
                for (global::System.Int32 i = 0; i < EAServerManager.Instance.clients.Count; i++)
                {
                    var Temp = EAServerManager.Instance.clients[i];

                    if (Temp.LoadedPersona.Name!="")
                    {
                        PersonaList.Add(Temp.ReturnOnlineInfo());
                    }
                }

                var serializer = JsonConvert.SerializeObject(PersonaList);
                return serializer.ToString();
            }


            if (SplitCheck(SplitURL, 2, "persona"))
            {
                //Return online persona list
            }

            return "NULL";
        }

        public struct WebData
        {
            public string FileName;
            public string FileText;
        }
    }
}
