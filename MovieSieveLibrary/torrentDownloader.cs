//Copyright 2010 Tomaz Toplak
/*This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Xml;
using System.Collections;
using System.Text.RegularExpressions;
using System.Threading;
using System.Net;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Configuration;

namespace MovieSieveLibrary
{
    public class torrentDownloader
    {
        #region Data

            private static ArrayList torrents;
            private XmlTextReader rssReader;
            private XmlDocument rssDoc;
            private XmlNode nodeRss;
            private XmlNode nodeChannel;
            private XmlNode nodeItem;
            
            //the link to the RSS feed
            private String mainFeed;
            //the place where the app stores the torrents
            private String path;
            //the minimal rating a movie should have nto be downloaded
            private double ratingArg;
            //the minimum number of votes a movie should have on IMDB
            private int numVotesArg;
            //the time in minutes before the app check the feed again for new stuff
            private int refreshTime;
            //the arguments for the torrent program
            private String argss;
            //the path to the torrent program to be used
            private String programPath;
            //strings that can be a part of the torrent name and describe its quality
            private String rsWhat="DSR|WEB-DL|INTERNAL|HDTV|PDTV|WS|S\\d\\dE\\d\\d|S\\dE\\d\\d|S\\dE\\d|S\\d\\d|S\\d|SEASON\\d\\d|SEASON\\d|DC|DVDSCR|DVD-R|BRSCR|SLOSUB|DVDRIP|XVID|LIMITED|R5|DVDR|NTSC|PAL|1080P|BLURAY|X264|720P|\\sAKA\\s|MOVIE PACK|\\sTS\\s|\\sCAM\\s|BRRIP|MULTISUB|BDRIP|SCREENER|COLLECTION|EXTRAS?";

            private String rsWw="DVDRIP|DVD-R|DVDSCR|BRSCR|BRRIP|R5|SCREENER|BDRIP|S\\d\\dE\\d\\d|S\\dE\\d\\d|S\\dE\\d|S\\d\\d|S\\d|SEASON\\d\\d|SEASON\\d|HDTV";

        #endregion

        #region Constructor
        public torrentDownloader(string feed, string savePath, double rating, int numVotes, 
                                 int refreshTime, string argss, string programPath, 
                                 string rsWhat, string rsWw)
        {
            torrents = new ArrayList();
            mainFeed = feed;
            path = savePath;
            ratingArg = rating;
            numVotesArg = numVotes;
            this.refreshTime = refreshTime;
            this.argss = argss;
            this.programPath=programPath;
            this.rsWhat = rsWhat;
            this.rsWw = rsWw;
        }        
		
		public torrentDownloader()
		{}
        #endregion

        #region Public Methods
        public void runDownloader()
        {
            Console.WriteLine("Program started at: " + DateTime.Now.ToString());
            Console.WriteLine("Movies vith a rating greater than " + ratingArg + " will be saved in " + path + "!!");

            //read movies from file that have allready been found good or have been allready downloaded.
            readSavedMovies();

            bool loop = true;
            while (loop)
            {                
                Thread t= new Thread(getGoodMovies);
                t.Start();
            
                //wait so long to refreash the feed
                Thread.Sleep(refreshTime*60*1000);

                if (t.IsAlive == true)
                {
                    Console.WriteLine("<" + DateTime.Now.ToString() + "> <ERROR> Previous thread still active, waiting 1 minute before abort!!");
                    Thread.Sleep(1 * 60 * 1000);

                    t.Abort();

                    torrents = new ArrayList();
                    readSavedMovies();
                }
            }
        }
		
		public movieTorrent parseName(string torrentName)
        {
            //the title will be in here!
            String title;
			String year;
            //clean the name
            
            torrentName = torrentName.Replace("[REQ]", "");
            title = torrentName;

            String bckTitle = torrentName;
            String random;

            //replace all non word characters with a space
            Regex spaceChar=new Regex("\\W");
            Match s = spaceChar.Match(bckTitle);           
            String matchValue;
            Match s2;

            while ((matchValue = getnextMatch(s, title, out s2)) != null)
            {
                title = title.Replace(matchValue, " ");
                s = s2;
            }

            title = title.ToUpper();

            //to get the movie releas year
            Regex yearregex = new Regex("\\s\\d\\d\\d\\d\\s");
            
            //to find random shit in torrent names
            Regex randomShit = new Regex(rsWhat);
			
			//to determine if it is a series
			Regex seasonRegex= new Regex("S\\d\\dE\\d\\d|S\\dE\\d\\d|S\\dE\\d|S\\d\\d|S\\d|SEASON\\d\\d|SEASON\\d");

            //find matches
            Match yearMatch = yearregex.Match(title);
            Match randomMatch = randomShit.Match(title);
			Match seasonMatch= seasonRegex.Match(title);
            year = yearMatch.Value;
            random = randomMatch.Value;
			String season = seasonMatch.Value;

            //determine where they are in the name
            int yearIndex = title.IndexOf(year);
            int randomIndex = title.IndexOf(random);

            //determine which one is first
            if (randomIndex < yearIndex&&randomIndex!=0)
                yearIndex = randomIndex;
            
            if (yearIndex == 0)
                yearIndex = randomIndex;

            //use only the part till the first match(year or randomshit)
            if(yearIndex!=0)
                title = title.Substring(0, yearIndex);
   							
			movieTorrent mt = new movieTorrent();
			mt.name=title;
			mt.originalName=torrentName;
			mt.year=year;
			mt.SeasonNum="";
			mt.EpisodeNum="";
			if(!season.Equals(""))
			{
				Regex sregex= new Regex("S\\d\\dE\\d\\d|S\\dE\\d\\d|S\\dE\\d");
				Match smatch=sregex.Match(season);
				int startindexs=0;
				int startindexe=0;
				
				startindexs=season.IndexOf("S");
				
				if(smatch.Value!="")
				{					
					startindexe=smatch.Value.IndexOf("E");					
					mt.SeasonNum=smatch.Value.Substring(startindexs+1, startindexe-startindexs-1);
					mt.EpisodeNum=smatch.Value.Substring(startindexe+1, smatch.Value.Length-startindexe-1);
				}
				else
				{
					sregex= new Regex("S\\d\\d|S\\d");
					smatch=sregex.Match(season);
					if(smatch.Value!="")
					{
						mt.SeasonNum=smatch.Value.Substring(startindexs+1, smatch.Value.Length-startindexs-1);			
					}
					else
					{
						sregex= new Regex("SEASON\\d\\d|SEASON\\d");
						smatch=sregex.Match(season);
						mt.SeasonNum=smatch.Value.Substring(6, smatch.Value.Length-6);
					}
				}								
			}
			
			return mt;            
        }
		
		public movieTorrent getIMDBData(movieTorrent mt)
        {
            string imdbLink = "";
            double ratingd = 0;
            int votes = 0;
			string posterLink="";
			string plot="";
			
            imdbLink = getIMDBLink(mt);
            if (imdbLink.Equals(""))
                throw new Exception("Did not find link!");

            ratingd = getIMDBRating(mt, imdbLink, out votes, out posterLink, out plot);            
            if (ratingd==-1)
               throw new Exception("Did not find rating!");
			
            mt.imdbLink=imdbLink;
			mt.imdbVotes=votes;
			mt.rating=ratingd;
			mt.imdbPosterLink=posterLink;
			mt.imdbPlot=plot;
			
			return mt;
		}
		
        #endregion

        #region Private Methods

        private void getGoodMovies()
        {
            Console.WriteLine("<"+DateTime.Now.ToString()+"> Refreshing feed!!!");

            if (!readFeed(mainFeed))
                return;

            bool write = false;
            foreach (movieTorrent mt in torrents)
            {
                if (!mt.dled)
                {
                    String name = mt.url.Substring(mt.url.LastIndexOf("/") + 1);
                    download(mt.url, name + ".torrent", mt.name);
                    if(!argss.Equals(""))
                        runTorrentProgram(name +".torrent", mt.name, argss, programPath);
                    mt.dled = true;
                    write = true;
                    Thread.Sleep(4000);
                }
            }

            if(write)
                writeInfoToFile();               
        }

        private void readSavedMovies()
        {
            Console.WriteLine("Reading movie data from file!!");
            try
            {
                System.Xml.Serialization.XmlSerializer serializer = 
                    new System.Xml.Serialization.XmlSerializer(typeof(ArrayList), new Type[] {typeof(movieTorrent)});
                System.IO.TextReader reader =
                    new System.IO.StreamReader("movies.xml");
                torrents = (ArrayList)serializer.Deserialize(reader);

                reader.Close();
            }
            catch
            {
                Console.WriteLine("<" + DateTime.Now.ToString() + "> <ERROR> Reading movie data FAILED!! Creating new movies.txt!!");
            }
        }

        private bool readFeed(string feed)
        {
            try
            {
                // Create a new XmlTextReader from the specified URL (RSS feed)
                rssReader = new XmlTextReader(feed);             

                rssDoc = new XmlDocument();
            
                // Load the XML content into a XmlDocument
                rssDoc.Load(rssReader);
            }
            catch(Exception e) {
                if(e.ToString().Contains("System.UriFormatException"))
                    Console.WriteLine("<" + DateTime.Now.ToString() + "> <ERROR> The RSS feed URL is weird! Check that!");
                else
                    Console.WriteLine("<" + DateTime.Now.ToString() + "> <ERROR> Can't accsses feed.. maybe internet connection broken!!");
                return false;
            }
            // Loop for the <rss> tag
            for (int i = 0; i < rssDoc.ChildNodes.Count; i++)
            {
                // If it is the rss tag
                if (rssDoc.ChildNodes[i].Name == "rss")
                {
                    // <rss> tag found
                    nodeRss = rssDoc.ChildNodes[i];
                }
            }

            if (nodeRss == null)
            {
                Console.WriteLine("<" + DateTime.Now.ToString() + "> <ERROR> Can't find <rss> node.. this feed is corrupt!!!");
                return false;
            }
            // Loop for the <channel> tag
            for (int i = 0; i < nodeRss.ChildNodes.Count; i++)
            {
                // If it is the channel tag
                if (nodeRss.ChildNodes[i].Name == "channel")
                {
                    // <channel> tag found
                    nodeChannel = nodeRss.ChildNodes[i];
                }
            }

            if (nodeChannel == null)
            {
                Console.WriteLine("<" + DateTime.Now.ToString() + "> <ERROR> Can't find <channel> node.. this feed is corrupt!!!");
                return false;
            }

            // Set the labels with information from inside the nodes
            Console.WriteLine("Reading RSS fead: " + nodeChannel["title"].InnerText);
            Console.WriteLine("Description: " + nodeChannel["description"].InnerText);

            // Loop for the <title>, <link> and all the other tags
            for (int i = 0; i < nodeChannel.ChildNodes.Count; i++)
            {                
                if (nodeChannel.ChildNodes[i].Name == "item")
                {
                    nodeItem = nodeChannel.ChildNodes[i];
                    String year;

                    //in case the feed is mixed check if the torents have category descriptions
                    bool hasCatDescription = false;
                    if (nodeItem.InnerXml.ToLower().Contains("<category>"))
                    {
                        hasCatDescription = true;
                        if (nodeItem["category"].InnerText.ToLower().Contains("movie"))
                            hasCatDescription = false;
	
                    }
					else if(nodeItem.InnerXml.ToLower().Contains("<description>"))
					{
						hasCatDescription = true;
						if (nodeItem["description"].InnerText.ToLower().Contains("video"))
							 hasCatDescription = false;
					}

                    if (checkIfThere(nodeItem["title"].InnerText)&&!hasCatDescription)
                    {
                        String title = parseTorrentName(nodeItem["title"].InnerText, out year);
                        
                        if (!title.Equals(""))
                        {                       
                            movieTorrent mt = new movieTorrent();

                            string guid = findTorrentLink(nodeItem); ;

                            if (!guid.Equals(""))
                            {
                                mt.doShit(title, guid, year, nodeItem["title"].InnerText);
                                double rating;
                                string imdbLink;

                                if (checkIMDB(mt, out rating, out imdbLink))
                                {
                                    mt.imdbLink = imdbLink;
                                    mt.rating = rating;
                                    torrents.Add(mt);
                                }
                            }
                        }
                    }
                }
            }
            writeInfoToFile();
            return true;
        }

        //some feeds are strange and the link is somewhere just not where
        //it should be..
        //so we check the link end gudi nodes and then the whole node xml for
        //a .torrent file.. 
        private string findTorrentLink(XmlNode nodeItem)
        {
            string guid="";
            try
            {
                guid = nodeItem["link"].InnerText;
            }
            catch { }

            if (guid.Equals(""))
            {
                try
                {
                    guid = nodeItem["guid"].InnerText;
                }
                catch { }
            }

            Regex guidRegex = new Regex("http://[^<^\"]*\\.torrent");
            Match m = guidRegex.Match(nodeItem.InnerXml);
            if (m.Value != "")
            {
                guid = m.Value;
            }

            return guid;
        }

        private void writeInfoToFile()
        {
            Console.WriteLine("Writing movies to file!!");
            try{
                System.Xml.Serialization.XmlSerializer serializer =
                    new System.Xml.Serialization.XmlSerializer(typeof(ArrayList), new Type[] { typeof(movieTorrent) });
                System.IO.TextWriter writer =
                    new System.IO.StreamWriter("movies.xml", false);
                serializer.Serialize(writer, torrents);

                writer.Close();
            }
            catch
            {
                Console.WriteLine("<" + DateTime.Now.ToString() + "> <ERROR> Writing movies to file FAILED!!");    
            }
       }

        //check if a saved movie is the same as the current one
        private bool checkImdbLink(movieTorrent mt)
        {
            foreach (movieTorrent mt2 in torrents)
            {
                if (mt2.imdbLink.Equals(mt.imdbLink))
                {
                    return false;
                }
            }
            return true;
        
        }

        private bool checkIfThere(string title)
        {
            foreach (movieTorrent mt in torrents)
            {
                if (mt.originalName.Equals(title))
                {
                    return false;
                }
            }
            return true;
        }

        private bool download(String url, String filename, String movieName)
        {
            try
            {
                WebClient DLer = new WebClient();
                DLer.DownloadFile(url, path + "/" + movieName+".torrent");
            }
            catch 
            {
                Console.WriteLine("<" + DateTime.Now.ToString() + "> <ERROR> Something went wrong while downloading " + movieName + " - TORRENT");
                return false;
            }
            return true;
        }

        private bool runTorrentProgram(String filename, String movieName, String argss, String programPath)
        {
            try
            {
                Process p = new Process();

                //the user has to put the comand in the right format
                argss = argss.Replace("{torrent}", " " + '"' + path + "\\" + movieName + ".torrent" + '"');
                programPath = "uTorrent.exe";

                p.StartInfo = new ProcessStartInfo(programPath, argss)

                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                p.Start();
                Console.WriteLine("<" + DateTime.Now.ToString() + "> Downloading " + movieName + "!!");
            }
            catch
            {
                Console.WriteLine("<" + DateTime.Now.ToString() + "> <ERROR> Something went wrong while starting " + movieName + " - TORRENT DOWNLOAD");
                return false;
            }
            return true;
        }

        private string parseTorrentName(string torrentName, out string year)
        {
            //the title will be in here!
            String title;

            //clean the name
            
            torrentName = torrentName.Replace("[REQ]", "");
            title = torrentName;

            String bckTitle = torrentName;
            String random;

            //replace all non word characters with a space
            Regex spaceChar=new Regex("\\W");
            Match s = spaceChar.Match(bckTitle);           
            String matchValue;
            Match s2;

            while ((matchValue = getnextMatch(s, title, out s2)) != null)
            {
                title = title.Replace(matchValue, " ");
                s = s2;
            }

            title = title.ToUpper();

            //to get the movie releas year
            Regex yearregex = new Regex("\\s\\d\\d\\d\\d\\s");
            
            //to find random shit in torrent names
            Regex randomShit = new Regex(rsWhat);

            //find matches
            Match yearMatch = yearregex.Match(title);
            Match randomMatch=randomShit.Match(title);
            year = yearMatch.Value;
            random = randomMatch.Value;

            //determine where they are in the name
            int yearIndex = title.IndexOf(year);
            int randomIndex = title.IndexOf(random);

            //determine which one is first
            if (randomIndex < yearIndex&&randomIndex!=0)
                yearIndex = randomIndex;
            
            if (yearIndex == 0)
                yearIndex = randomIndex;

            //use only the part till the first match(year or randomshit)
            if(yearIndex!=0)
                title = title.Substring(0, yearIndex);

            string upperTitle = bckTitle.ToUpper();
            //if there is random shit we want it to include DVDRIP or BRRIP ...
            Regex randomShitWeWant = new Regex(rsWw);
            Match shitWeWantMatch = randomShitWeWant.Match(upperTitle);

            if(!shitWeWantMatch.Value.Equals(""))           
                return title;

            else if (yearIndex == 0)
                return title;
            
            //if the torrent name stinks, ignore the torrent
            Console.WriteLine("<" + DateTime.Now.ToString() + "> <ERROR> the torrent "+ bckTitle +
                              " stinks. Will NOT PROCESS!!!");
            
            return "";
        }

        private String getnextMatch(Match s, string title, out Match s2)
        {
            String matchValue;
            int c = 0;
            do
            {
                matchValue = s.Value;
                s2 = s.NextMatch();

                c++;
                if (c > title.Length)
                {
                    matchValue = null;
                    break;
                }
            }
            while (matchValue.Equals(""));

            return matchValue;
        }

        private bool checkIMDB(movieTorrent mt, out double ratingd, out string imdbLink)
        {
            imdbLink = "";
            ratingd = 0;
            int votes = 0;
			string posterlink="";
            imdbLink = getIMDBLink(mt);
            if (imdbLink.Equals(""))
                return false;
			string plot="";
            ratingd = getIMDBRating(mt, imdbLink, out votes, out posterlink, out plot);            
            if (ratingd==-1)
                return false;

            mt.imdbLink = imdbLink;

            if (ratingd > ratingArg && votes > numVotesArg)                                
            {
                if (checkImdbLink(mt))
                {
                    Console.WriteLine("The movie " + mt.name + "(" + mt.year + ")" + " has a rating of " + ratingd + ". Will DOWNLOAD!!");
                    return true;
                }
                else 
                {
                    Console.WriteLine("The movie " + mt.name + "(" + mt.year + ")" + " was already downloaded from a previous release" + ". Will NOT DOWNLOAD!!");
                    return false;
                }
            }
            else
            {
                Console.WriteLine("The movie " + mt.name + "(" + mt.year + ")" + " has a rating of " + ratingd + " and " + votes + " votes. Will NOT DOWNLOAD!!");
                return false;
            }
            
        }

        private string getIMDBLink(movieTorrent mt)
        {
            System.Net.WebClient Client;
            Stream strm;
            StreamReader sr;
            string line;
            string imdbLink="";
			string searchName=mt.name.Replace(" ", "+");
            try
            {
                Client = new WebClient();
                strm = Client.OpenRead("http://www.google.si/search?q=site:imdb.com+" + searchName + "+" + mt.year);
                sr = new StreamReader(strm);

                do
                {
                    line = sr.ReadLine();
                    if (line != null)
                    {
                        Regex imdb = new Regex("http://www.imdb.com/title/[a-z0-9]*/");
                        Match m = imdb.Match(line);
                        if (m.Value != "")
                        {
                            imdbLink = m.Value;
                            break;
                        }
                    }
                }
                while (line != null);
                strm.Close();
            }
            catch
            {
                Console.WriteLine("<" + DateTime.Now.ToString() + "> <ERROR> while connecting to google ("+mt.name+")!!");
            }
              
            if (imdbLink.Equals(""))
            {
                Console.WriteLine("<" + DateTime.Now.ToString() + "> <ERROR> Could not find a imdb link for " + mt.originalName+" !!");
            }

            return imdbLink;

        }

        private string checkIfMatchesTitle(string imdbTitle, string torrentTitle)
        {

            string imdbTitleBck = imdbTitle;
       
            torrentTitle = torrentTitle.ToUpper();
            imdbTitle = imdbTitle.ToUpper();

            string[] torrentTParts = 
                torrentTitle.Split(new char[1]{' '},System.StringSplitOptions.RemoveEmptyEntries);
            string[] imdbTParts = 
                imdbTitle.Split(new char[1]{' '},System.StringSplitOptions.RemoveEmptyEntries);
            int count = 0;
                            
            foreach (string tnp in torrentTParts)
            {
                if (imdbTitle.Contains(tnp))
                {
                    count++;
                }
            }

            //in case the torrent name is an alternative long version
            //if it maches all the words in the imdb title it is probbaly
            //the same movie.. we can never be absolutely certain
            if (count == imdbTParts.Length)
                count = torrentTParts.Length;

            double treshold = torrentTParts.Length*0.7;
            if (count<treshold)
                return "";
            

            return imdbTitleBck;
        }

        private double getIMDBRating(movieTorrent mt, String imdbLink, out int votes, out string posterLink, out string plot)
        {
            System.Net.WebClient Client;
            Stream strm;
            StreamReader sr;
            string line;
            string rating = "";
            string voteCount="";
            double ratingd = 0;
            string title = "";
            bool foreign = true;
			posterLink="";
            votes = 0;
			plot="";
            
            try
            {
                Client = new WebClient();
                strm = Client.OpenRead(imdbLink);
                sr = new StreamReader(strm);
                line = "";
				
				bool plotCounter=false;
				int plotC=0;
				string previousLine="";
                do
                {
                    line = sr.ReadLine();

                    if (line != null)
                    {			
						if(plotCounter)
						{
							plot=line;
							plotC++;
							if(plotC==2)
							{
								plot=line;
								plotCounter=false;	
							}
						}
                        //Regullar expression that find the rating on an IMDB page.
                        Regex imdb = new Regex("\\d.\\d<span>/10</span>");
                        Match m = imdb.Match(line);

                        //the one, that finds the vote count
                        Regex votecount = new Regex("[,\\d]* votes<");
                        Match m2 = votecount.Match(line);

                        //and the one, that determines the movie name of the found imdb link.
                        Regex imdbTitle = new Regex("<h1>.*<span>");
                        Match m3 = imdbTitle.Match(line);
						
						//find the poster link
						Regex posterRegex= new Regex("height=\"314\" width=\"214\"");
						Match m4=posterRegex.Match(line);
						
						//finde the plot
						Regex plotRegex=new Regex("<h2>Storyline</h2>");
						Match m5=plotRegex.Match(line);
						
						if(m5.Value!="")
							plotCounter=true;
						
                        if (m3.Value != "")
                        {
                            if (title.Equals(""))
                                title = m3.Value.Substring(4, m3.Value.IndexOf("<span>") - 4);
                        }
                        //this part is hard.. needs more work.. but lets leave it this way for now
                        if (foreign)
                            if (line.Contains("/Sections/Languages/English/"))
                            {
                                foreign = false;
                                break;
                            }
                        
                        if (m.Value != "")
                        {
                            if (rating.Equals(""))
                                rating = m.Value;
                        }
                        if (m2.Value != "")
                            voteCount = m2.Value;
						
						if(m4.Value!="")
						{
							int ih=previousLine.LastIndexOf("img src=\"");	
							posterLink=previousLine.Substring(ih+9, previousLine.Length-10-ih);
						}
						
						previousLine=line;
                    }
                }
                while (line != null);
                strm.Close();
            }
            catch
            {
                Console.WriteLine("<" + DateTime.Now.ToString() + "> <ERROR> while connecting to IMDB(" + mt.name + ")!!");
               
                return -1;
            }

            if (!foreign)
            {
                mt.name = checkIfMatchesTitle(title, mt.name);
            }

            if (mt.name.Equals(""))
            {
                Console.WriteLine("<" + DateTime.Now.ToString() + "> <ERROR> The IMDB link, did not match the torrent name!! {"+mt.originalName+"}");
                return -1;
            }

            if (rating.Equals(""))
            {
                Console.WriteLine("<" + DateTime.Now.ToString() + "> <ERROR> The movie " + mt.name + " has no rating!!");
                return -1;
            }

            try
            {
                rating = rating.Substring(0, rating.IndexOf("/")-6);
                rating = rating.Replace(",", ".");

                //get the double value of the rating
                ratingd = Convert.ToDouble(rating);

                //get the number of votes
                string votess = voteCount.Substring(0, voteCount.IndexOf(" vot"));
                votes = Convert.ToInt32(votess.Replace(",", ""));
            }
            catch
            {
                Console.WriteLine("<" + DateTime.Now.ToString() + "> <ERROR> IMDB changed something.. contact the author of this tool!!");
               
                return -1;
            }
            return ratingd;
        }
        #endregion

    }
}