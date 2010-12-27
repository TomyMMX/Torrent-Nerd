using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Net;
using log4net;
using System.Text.RegularExpressions;

namespace SubtitleDownloaderLib
{
	public class SubData
	{
		public string Name { get; set; }
		public string Fps { get; set; }
		public string CdNum { get; set; }
		public string DownloadPageLink { get; set; }
	}
	public class XmlResultsReader
	{
		private static readonly ILog AppLog = LogManager.GetLogger (typeof(XmlResultsReader));
		public XmlDocument currentResults { get; set; }
		public List<SubData> resultsList { get; set; }
		private string feed = "http://www.podnapisi.net/ppodnapisi";

		public XmlResultsReader (string feed)
		{
			this.feed = feed;
		}
		public XmlResultsReader ()
		{
		}

		public bool getSearchResults (string kind, string lang, string searchName, string releaseData, string seasonNum, string epNum, string year)
		{
			//replace whitespaces with a plus
			searchName = searchName.Replace (" ", "+");
			releaseData=releaseData.Replace(" ", "+");
			
			string url = "";
			
			if (kind == "3") {
				if (epNum == "-1") {
					url = feed + "/search?tbsl={0}&sK={1}&sJ={2}&sTS={3}&sOCS={4}&sR={5}&sY={6}&sXML=1&sS=downloads&sO=desc&sAKA2=1";
					url = string.Format (url, kind, searchName, lang, seasonNum, 1, releaseData, year);
				} else {
					url = feed + "/search?tbsl={0}&sK={1}&sJ={2}&sTS={3}&sTE={4}&sR={5}&sY={6}&sXML=1&sS=downloads&sO=desc&sAKA2=1";
					url = string.Format (url, kind, searchName, lang, seasonNum, epNum, releaseData, year);
				}
			} else {
				url = feed + "/search?tbsl={0}&sK={1}&sJ={2}&sR={3}&sY={4}&sXML=1&sS=downloads&sO=desc&sAKA2=1";
				url = string.Format (url, kind, searchName, lang, releaseData, year);
			}
			try {
				// Create an interface to the web
				WebClient c = new WebClient ();
				
				// Download the XML into a string
				string xml = ASCIIEncoding.Default.GetString (c.DownloadData (url));
				
				// Document to contain the feed
				currentResults = new XmlDocument ();
				
				// Parse the xml
				currentResults.LoadXml (xml);
				if(releaseData!="")
					if(currentResults.SelectSingleNode("/results/pagination/results").InnerXml=="0")
						getSearchResults(kind, lang, searchName, "", seasonNum, epNum, year);
					
				
				return true;
			} catch {
				AppLog.Error ("Could not read XML feed!");
				return false;
			}
		}

		public void compileResultList ()
		{
			resultsList = new List<SubData> ();
			try {
				foreach (XmlNode node in currentResults.SelectNodes ("/results/subtitle")) {
					SubData sd = new SubData ();
					
					if(node.SelectSingleNode ("release").InnerXml.Length<9)
						sd.Name = node.SelectSingleNode ("title").InnerXml + "-" + node.SelectSingleNode ("release").InnerXml;
					else
						sd.Name = node.SelectSingleNode ("release").InnerXml;
					sd.Fps = node.SelectSingleNode ("fps").InnerXml;
					sd.CdNum = node.SelectSingleNode ("cds").InnerXml;
					sd.DownloadPageLink = node.SelectSingleNode ("url").InnerXml;
					
					bool addSub=true;
					foreach(SubData osd in resultsList)	
					{
						if(sd.Name.Equals(osd.Name))
						{
							addSub=false;
							break;
						}
					}
					
					if(addSub)
						resultsList.Add (sd);
				}
			} catch {
				AppLog.Error ("Problem parsing result XML!");
			}
		}

		public string getDownloadLink (string downloadPageUrl)
		{
			try {
				string result = RegexWebpageParser.getRegexResultOnWebPage (downloadPageUrl, "/download/.*\" title");
				
				result = result.Substring (0, result.Length - 7);
				result = feed + result;
				return result;
			} catch {
				AppLog.Error ("Problem with result.. trying next!");
			}
			return "";
		}
	}
}
