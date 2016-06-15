using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CsvHelper;
using System.Net;

namespace ConsoleScrape
{
    class Program
    {
        static void Main(string[] args)
        {
            //WebClient setup
            WebClient web = new WebClient();

            //File setup
            FileStream file = File.Open("met artifact data.csv", FileMode.Open);
            TextReader tr = new StreamReader(file);
            CsvParser parser = new CsvParser(tr);

            //Execute parsing method, write result to console
            string result = DownloadImages(parser, web);
            Console.WriteLine(result);

            //Wait for user input before closing
            Console.Read();
        }

        /// <summary>
        ///  Parses a CSV file for all web resources, downloading them to file if possible.
        ///  Prints a status message to the console when end of file reached. 
        /// </summary>
        /// <param name="parser">Csv Parser provided by CsvHelper Lib</param>
        /// <param name="web">Simple WebClient</param>
        /// <returns>status message as string</returns>
        private static string DownloadImages(CsvParser parser, WebClient web)
        {
            string[] row = parser.Read();
            while (row != null)
            {
                // @params 
                // [0]: title, [1]: imgUrl, [2]: date, [3]: accessionNumber, [4]: medium, [5]: location
                Artifact art = new Artifact(row[0], row[1], row[2], row[3], row[4], row[5]);

                //Logical, unique filename is the artifcat title combined with accession #
                string fileName = art.Title + "_" + art.AccessionNumber + ".jpg";
                try
                {
                    if (RemoteFileExists(art.ImgUrl))
                    {
                        //Only attempt to download the web resource if it exists (200 OK)
                        web.DownloadFile(art.ImgUrl, fileName);
                    }
                }
                catch (WebException e)
                {
                    //Something went wrong or the URL is invalid 
                    Console.WriteLine(e);

                }

                //Parse next row
                row = parser.Read();
            }
            return "All rows parsed & Files downloaded.";
        }

        /// <summary>
        /// Easy HTTP request check for valid web resources
        /// </summary>
        /// <param name="url">string url of web location</param>
        /// <returns>True if the status code is 200 OK, otherwise false</returns>
        private static bool RemoteFileExists(string url)
        {
            try
            {
                //Creating the HttpWebRequest
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "HEAD";
                //Getting the Web Response.
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                //Returns TRUE if the Status code == 200
                response.Close();
                return (response.StatusCode == HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                //Any exception is rejected
                Console.WriteLine(e);
                return false;
            }
        }
    }

    /// <summary>
    /// Structure representing a Met Museum artifcat. A simple constructor populates public data 
    /// fields that describe their data.
    /// </summary>
    struct Artifact
    {
        public Artifact(string title, string imgUrl, string date, 
            string accessionNumber, string medium, string location)
        {
            Title = title;
            ImgUrl = imgUrl;
            Date = date;
            AccessionNumber = accessionNumber;
            Medium = medium;
            Location = location;
        }

        public string Title { get; set; }
        public string ImgUrl { get; set; }
        public string Date { get; set; }
        public string AccessionNumber { get; set; }
        public string Medium { get; set; }
        public string Location { get; set; }
    }
}
