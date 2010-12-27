using System;
namespace TorrentInfoLib
{	
	public class TorrentData{
		public enum Mediatype{
			Movie,
			Series
		};
		public string Name{set;get;}
		public string Release{set;get;}
		public Mediatype Type{set;get;}
		public int EpNum{set;get;}
		public int SeasonNum{set;get;}
		public bool WholeSeason{set;get;}
		public string Year{set;get;}
		public string ImdbLink{set;get;}
		public int ImdbVotes{set;get;}
		public double ImdbRating{set;get;}
		public string ImdbPosterLink{set;get;}
		public string ImdbPlot{set;get;}
	}

	public static class TorentInfo
	{
		public static TorrentData getData(string pathToTorrentFile)
		{
			//load data from the torrent file
			Torrent newTorrent= Torrent.Load(pathToTorrentFile);
			
			//parse the torrent name for anithing usefull
			MovieSieveLibrary.torrentDownloader td = new MovieSieveLibrary.torrentDownloader();
			MovieSieveLibrary.movieTorrent mt = td.parseName(newTorrent.Name);
			
			//get imdb data
			mt=td.getIMDBData(mt);
			
			TorrentData data= new TorrentData();
			data.Name=mt.name;
			data.Release=mt.originalName;
			data.Year=mt.year;
			data.ImdbLink=mt.imdbLink;
			data.ImdbRating=mt.rating;
			data.ImdbVotes=mt.imdbVotes;
			data.ImdbPosterLink=mt.imdbPosterLink;
			data.ImdbPlot=mt.imdbPlot;
			
			if(mt.SeasonNum!="")
			{
				data.Type=TorrentData.Mediatype.Series;
				data.SeasonNum=Convert.ToInt32(mt.SeasonNum);
				if(mt.EpisodeNum=="")	
				{
					data.WholeSeason=true;					
				}
				else
				{
					data.WholeSeason=false;
					data.EpNum=Convert.ToInt32(mt.EpisodeNum);
				}
			}
			else
				data.Type=TorrentData.Mediatype.Movie;
			
			return data;
		}
	}
}

