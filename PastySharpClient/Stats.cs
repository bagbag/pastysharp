using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using RestSharp;

namespace PastySharpClient
{
    public class Stats
    {
        public class SimpleResponse<T>
        {
            public T Value { get; set; }
        }

        public enum TrackActivity
        {
            AppStart,
            AppTerm,
            Paste
        };

        public static String StatsUrl;

        public static void TrackAction(TrackActivity action, String comment = null)
        {
            var t = new Thread(() =>
            {
                try
                {
                    var client = new RestClient(StatsUrl);

                    var request = new RestRequest("trackAction", Method.GET);
                    request.AddHeader("Accept", "application/xml");

                    request.AddParameter("id", GetUniqueId());
                    request.AddParameter("version", GetVersionString());
                    request.AddParameter("action", action.ToString());
                    if (!String.IsNullOrEmpty(comment))
                    {
                        request.AddParameter("comment", comment);
                    }

                    var response = client.Execute<SimpleResponse<String>>(request);

                    if (response.Data.Value == "ok")
                    {
                        //good
                    }
                }
                catch
                {
                    //sorry
                }
            });
            t.IsBackground = false;
            t.Start();
        }

        public static void TrackCustomVariable(String key, object value, String comment = null)
        {
            var t = new Thread(() =>
            {
                try
                {
                    var client = new RestClient(StatsUrl);

                    var request = new RestRequest("trackCustomVariable", Method.GET);
                    request.AddHeader("Accept", "application/xml");

                    request.AddParameter("id", GetUniqueId());
                    request.AddParameter("version", GetVersionString());
                    request.AddParameter("key", key);
                    request.AddParameter("value", SimpleJson.SerializeObject(value));
                    if (!String.IsNullOrEmpty(comment))
                    {
                        request.AddParameter("comment", comment);
                    }

                    var response = client.Execute<SimpleResponse<String>>(request);

                    if (response.Data.Value == "ok")
                    {
                        //good
                    }
                }
                catch
                {
                    //sorry
                }
            });
            t.IsBackground = false;
            t.Start();
        }

        public static string GetVersionString()
        {
            var v = Assembly.GetExecutingAssembly().GetName().Version;
            var sli = new List<String>();
            sli.Add(v.Major.ToString());
            if (v.Minor != 0 || v.Revision != 0 || v.Build != 0)
            {
                sli.Add(v.Minor.ToString());
                if (v.Revision != 0 || v.Build != 0)
                {
                    sli.Add(v.Build.ToString());
                    if (v.Revision != 0)
                    {
                        sli.Add(v.Revision.ToString());
                    }
                }
            }
            return "v" + String.Join(".", sli);
        }

        private static string GetUniqueId()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PastySharpClient", "uid.uid");

            if (!Directory.Exists(Path.GetDirectoryName(path)))
                Directory.CreateDirectory(Path.GetDirectoryName(path));

            if (!File.Exists(path) || File.ReadAllText(path).Length != 16)
            {
                var random = new RNGCryptoServiceProvider();
                var data = new byte[8];
                random.GetBytes(data);
                var uid = BitConverter.ToString(data).ToLower().Replace("-", "");

                File.WriteAllText(path, uid);
                return uid;
            }

            return File.ReadAllText(path);
        }
    }
}