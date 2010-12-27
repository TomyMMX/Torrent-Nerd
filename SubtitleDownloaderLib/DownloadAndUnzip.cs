using System;
using System.Net;
using System.IO;
namespace SubtitleDownloaderLib
{
	public static class DownloadAndUnzip
	{
		public static bool doDownload(string downLink, string filename)
		{
			try{
				WebClient webClient = new WebClient();
				Stream data = webClient.OpenRead(downLink);	
				unzipfromDownload.UnzipStream(data, filename);		 
				return true;
			}
			catch
			{
				return false;	
			}
		}
	}
}

