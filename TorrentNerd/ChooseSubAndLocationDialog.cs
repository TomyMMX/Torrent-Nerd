using System;
using System.Net;
using System.IO;
using SubtitleDownloaderLib;
using Gtk;
using System.Windows.Forms;
namespace TorrentNerd
{
	public partial class ChooseSubAndLocationDialog : Gtk.Dialog
	{
		public delegate void TorrentAddedToDownload();
		private SubtitleDownloaderLib.XmlResultsReader xrr;
		private TorrentInfoLib.TorrentData td;
		private string path;
		
		private string language="2";
		public ChooseSubAndLocationDialog (string path)
		{
			this.Build ();
			
			if (!path.Equals ("")) {
				this.path = path;
				
				//get all the necesary data form the file, imdb and podnapisi.net
				td = TorrentInfoLib.TorentInfo.getData (path);
				
				lblRating.Text = td.ImdbRating + "/10";
				lblVotes.Text = td.ImdbVotes.ToString ();
				btnLink.Label = td.ImdbLink;
				lblName.Text = td.Name;
				lblRelease.Text = td.Release;
				tvPlot.Buffer.Text=td.ImdbPlot;
				
				btnPath.Label = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
				HttpWebRequest wreq;
				HttpWebResponse wresp;
				Stream mystream;
				
				mystream = null;
				wresp = null;
				try {
					wreq = (HttpWebRequest)WebRequest.Create (td.ImdbPosterLink);
					wreq.AllowWriteStreamBuffering = true;
					
					wresp = (HttpWebResponse)wreq.GetResponse ();
					
					mystream = wresp.GetResponseStream ();
				} catch {
				}
				
				imgPoster.Pixbuf = new Gdk.Pixbuf (mystream, 143, 209);
				
				tvSubs.AppendColumn ("Name", new Gtk.CellRendererText (), "text", 0);
				tvSubs.AppendColumn ("Fps", new Gtk.CellRendererText (), "text", 1);
				tvSubs.AppendColumn ("#CDs", new Gtk.CellRendererText (), "text", 2);
				
				getSubs();
			}
		}
		
		private void getSubs()
		{
				xrr = new SubtitleDownloaderLib.XmlResultsReader ();
				
				string kind = "3";
				if (td.Type == TorrentInfoLib.TorrentData.Mediatype.Movie)
					kind = "2";
				string epNum = "-1";
				if (td.EpNum != 0)
					epNum = td.EpNum.ToString ();
				
				xrr.getSearchResults (kind, language, td.Name, td.Release, td.SeasonNum.ToString (), epNum, td.Year);
				xrr.compileResultList ();
				
				Gtk.ListStore subListStore = new Gtk.ListStore (typeof(string), typeof(string), typeof(string));
				foreach (SubData sd in xrr.resultsList) {
					subListStore.AppendValues (sd.Name, sd.Fps, sd.CdNum);
				}
				
				tvSubs.Model = subListStore;
		}

		protected virtual void btnDownload_Clicked (object sender, System.EventArgs e)
		{
			Gtk.TreeSelection gts = tvSubs.Selection;
			TreeModel model;
			TreeIter iter;
			try {
				gts.GetSelected (out model, out iter);
				
				String val = model.GetValue (iter, 0).ToString ();
				
				SubData sd = null;
				foreach (SubData sdd in xrr.resultsList) {
					if (sdd.Name.Equals (val)) {
						sd = sdd;
						break;
					}
				}
				string subDownloadLink = "";
				if (sd != null && xrr.resultsList.Count != 0) {
					subDownloadLink = xrr.getDownloadLink (sd.DownloadPageLink);
					
					string subsDownPath = btnPath.Label + "/" + td.Release + "/subs";
					
					if (!DownloadAndUnzip.doDownload (subDownloadLink, subsDownPath))
						MessageBox.Show ("Could not download subtitle!");
					
				}
			} catch {
			}
			
			string [] settings = null;
			
			try{
				settings = DBWorkers.DBLogic.getConfig();		
			}
			catch(Exception ex)
			{
				//default setting values if not in db
				settings=new string[3];
				settings[0]="transmission";
				settings[1]="127.0.0.1";
				settings[2]="9091";
			}
			if(settings[0].ToLower().Equals("transmission"))
			{
				TransmissionRPCLib.ControlTransmission ct = new TransmissionRPCLib.ControlTransmission(settings[1], settings[2]);
				
				ct.addTorrent(btnPath.Label, path);
			}
			else if(settings[0].ToLower().Equals("utorrent"))
			{
				//TODO: send torernt path and download dir to utorrent app
			}
			
			this.Respond(ResponseType.Ok);
		}

		protected virtual void closeWindow (object o, Gtk.RemovedArgs args)
		{
			Gtk.Application.Quit ();
		}

		protected virtual void OnBtnPathClicked (object sender, System.EventArgs e)
		{
			Gtk.FileChooserDialog fcd = new FileChooserDialog ("Select download location", this, FileChooserAction.SelectFolder, "Cancel", ResponseType.Cancel, "Select", ResponseType.Accept);
			fcd.SetCurrentFolder (Environment.GetFolderPath (Environment.SpecialFolder.Personal));
			
			if (fcd.Run () == (int)ResponseType.Accept) {
				btnPath.Label = fcd.Filename;
			}
			
			fcd.Destroy ();
		}

		protected virtual void languageChanged (object sender, System.EventArgs e)
		{
			if(language=="2")
				language="1";
			else
				language="2";
			
			getSubs();
		}
		
		protected virtual void openImdbLink (object sender, System.EventArgs e)
		{
			try{
			System.Diagnostics.Process.Start(btnLink.Label);
			}
			catch{}
		}
		
		
		
	}
}

