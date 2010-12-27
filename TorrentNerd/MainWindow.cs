using System;
using Gtk;
using System.Net;
using System.IO;
using GLib;
using System.Collections;
using SubtitleDownloaderLib;

namespace TorrentNerd
{		
	public partial class MainWindow : Gtk.Window
	{
		public MainWindow () : base("")
		{
			this.Build ();
			this.DeleteEvent += delegate { Application.Quit (); };	
		}		
		
		public void ShowDownDialog (string path)
		{			
			ChooseSubAndLocationDialog subDialog= new ChooseSubAndLocationDialog(path);
			
			ResponseType rsp = (ResponseType) subDialog.Run();
			int a=1;
			if(rsp==ResponseType.Ok)
			{
				a=5;
			}
			subDialog.Destroy();
		}
	}
}

