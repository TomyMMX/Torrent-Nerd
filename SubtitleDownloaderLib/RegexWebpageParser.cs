using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace SubtitleDownloaderLib
{
   public static class RegexWebpageParser
   {
	 public static string getRegexResultOnWebPage(string link, string regexRule)
	 {
	    Stream strm;
	    StreamReader sr;
	    WebClient Client = new WebClient();
	    string line;
	    try
	    {
		  strm = Client.OpenRead(link);
		  sr = new StreamReader(strm);
		  line = "";

		  //Regullar expression we want to find in page
		  Regex rgx = new Regex(regexRule);
		  do
		  {
			line = sr.ReadLine();

			if (line != null)
			{
			   int a=1;
			   if(line.Contains("download/"))
				 a=1;
			   Match m = rgx.Match(line);

			   if (m.Value != "")
			   {
				 return m.Value;
			   }			  
			}
		  }
		  while (line != null);
		  strm.Close();
	    }
	    catch {
		  return null;	    
	    }

	    return null;
	 }
   }
}
