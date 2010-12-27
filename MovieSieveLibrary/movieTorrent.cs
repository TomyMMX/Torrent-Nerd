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
using System.Xml.Serialization;

namespace MovieSieveLibrary
{
    [XmlRootAttribute("movieTorrent", Namespace = "", IsNullable = false)]
    public class movieTorrent
    {
        
        public String name { set; get; }
        
        public String url { set; get; }
       
        public bool dled { set; get; }
        
        public String year { set; get; }
       
        public string originalName { set; get; }
        public double rating { set; get; }
        public string imdbLink { set; get; }
		
		public int imdbVotes { set; get; }
		
		public String imdbPosterLink{set;get;}
		public String imdbPlot{set;get;}
		
		public string EpisodeNum{set;get;}
		
		public string SeasonNum{set;get;}

        public movieTorrent()
        {
        }

        public void doShit (String name, String url, String year, String oName)
        {   
            this.name = name;
            this.url = url;
            dled = false;
            this.year = year;
            originalName=oName;
        }
    }
}
