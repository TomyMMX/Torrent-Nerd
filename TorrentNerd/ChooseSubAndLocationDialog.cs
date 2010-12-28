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
		private SubtitleDownloaderLib.XmlResultsReader xrr;
		private TorrentInfoLib.TorrentData td;
		private string path;
		
		private string language="2";
		public ChooseSubAndLocationDialog (string path)
		{
			this.Build ();
			
			if (!path.Equals ("")) {
				this.path = path;
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
					
					string subsDownPath = btnPath.Label + "/" + td.Release + "/TNSubs";
					
					if (!DownloadAndUnzip.doDownload (subDownloadLink, subsDownPath))
						MessageBox.Show ("Could not download subtitle!");
					
				}
			} catch {
			}
			
			//executeTorrentProgram (path);
			//TODO read settings and open correct app
			TransmissionRPCLib.ControlTransmission ct = new TransmissionRPCLib.ControlTransmission("127.0.0.1", "9091");
			ct.addTorrent(btnPath.Label, path);
			
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

		private void executeTorrentProgram (string path)
		{
			System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo ("transmission-gtk", "\"" + path + "\"");
			
			// The following commands are needed to redirect the standard output.
			// This means that it will be redirected to the Process.StandardOutput StreamReader.
			procStartInfo.RedirectStandardOutput = false;
			procStartInfo.UseShellExecute = false;
			// Do not create the black window.
			procStartInfo.CreateNoWindow = false;
			// Now we create a process, assign its ProcessStartInfo and start it
			System.Diagnostics.Process proc = new System.Diagnostics.Process ();
			proc.StartInfo = procStartInfo;
			try {
				proc.Start ();
			} catch (Exception ex) {
				path = "";
			}
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

