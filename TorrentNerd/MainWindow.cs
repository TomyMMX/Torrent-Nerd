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
		private bool doRun=true;
		public MainWindow () : base("")
		{
			this.Build ();
			this.DeleteEvent += delegate { doRun=false; Application.Quit(); };	
		}	
		
		public void ShowAll(string path)
		{
			base.ShowAll();
			
			if(path!="")
				ShowDownDialog(path);
			
			System.Threading.Thread t = new System.Threading.Thread(checkStatus);
			t.Name="stateChecker";
			t.Start();
		}
		
		public void ShowDownDialog (object pathx)
		{			
			string path=(string)pathx;
			ChooseSubAndLocationDialog subDialog= new ChooseSubAndLocationDialog(path);
			
			ResponseType rsp = (ResponseType) subDialog.Run();
			if(rsp==ResponseType.Ok)
			{
				
			}
			subDialog.Destroy();
		}
		
		//periodicali check the status of tracked torrents and update their status.
		private void checkStatus()
		{
			while(doRun)
			{
				System.Threading.Thread.Sleep(100);
			}
		}
	}
}

