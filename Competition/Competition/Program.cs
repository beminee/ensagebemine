using System;
using System;
using System.Net;
using System.IO;
using System.Threading;

namespace SouceCodeProject
{
    class SourceCode
    {
        public static void Main(string[] args)
        {
            GetSourceCode();
        }

        public static void GetSourceCode()
        {
            string url = AskTheUserForURL();
            StreamReader theSourceCode = HTTPRequest(url);

            int count = 0;

            while (!theSourceCode.EndOfStream && !Console.KeyAvailable)
            {
                Console.CursorVisible = false;
                if (!theSourceCode.ToString().Contains("062c53e44fdc8a7aa2524d67300ae579")) continue;
                Console.WriteLine(theSourceCode.ReadLine().ToString());
            }

            Console.CursorVisible = true;
            Console.WriteLine();

        }

        public static string AskTheUserForURL()
        {
            string url = "https://www.ensage.io/store/loaderAuth&key";
            return url;
        }

        public static StreamReader HTTPRequest(string url)
        {
            HttpWebRequest myCall = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse theResponse = (HttpWebResponse)myCall.GetResponse();
            StreamReader source = new StreamReader(theResponse.GetResponseStream());
            return source;
        }
    }
}