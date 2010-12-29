using System;
using Gtk;
using Gdk;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using NDesk.DBus;
using GLib;
using org.freedesktop.DBus;
using System.IO;
using DBWorkers;

namespace TorrentNerd
{
	class MainClass
	{
		private static StatusIcon trayIcon;
		private static string busName = "org.ttsoft.torrentnerd";	
		private static string objectPath="/org/ttsoft/torrentnerd";
		public static TorrentNerd.MainWindow win;

		[DllImport ("libc")] // Linux
		private static extern int prctl (int option, byte [] arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5);
		[DllImport ("libc")] // BSD
		private static extern void setproctitle (byte [] fmt, byte [] str_arg);
		
		private static bool InitDBus(string path)
		{
			//Connection connection = Connection.Open ("tcp:host=localhost,port=8899");
			BusG.Init (/*connection*/);
			
			//get the session bus
			Bus bus = Bus.Session;
			ObjectPath oPath = new ObjectPath (objectPath);
									
			//INewTorrent torrentBusobject;
			INewTorrentOne torrentBusobject;
			if (bus.RequestName (busName) == RequestNameReply.PrimaryOwner) {
			  	//torrentBusobject = new NewTorrentBus();
				torrentBusobject = new NewTorrent ();
				torrentBusobject.newTorrentEvent+=HandleTorrentEvent;
				bus.Register(oPath, torrentBusobject);
							
				return true;
			}	
			else
			{
			   torrentBusobject = bus.GetObject<INewTorrent> (busName, oPath);				  
			   //torrentBusobject.sendPathToFirstinstance(path);
			   torrentBusobject.sendPathToFirstinstance (path);
			   return false;
			}
		}
		
		public static void HandleTorrentEvent(string path)
		{
			win.Visible=true;
			
			if(path!=null&&path != "")
			{
				win.ShowDownDialog(path);
			}
			else
			{
				
			}
		}

		public static void Main (string[] args)
		{
			//set MONO process name
			try {
    			SetProcessName ("TorrentNerd");
			} catch (Exception e) {
			    //Probably a windows machine
			}
			
			String path = "";
			try {
				path = args[0];	
			} catch {}			
		
			if(InitDBus(path))
			{			
				Application.Init();
				
				win = new TorrentNerd.MainWindow ();
				
				trayIcon = new StatusIcon (new Pixbuf ("ico/tn.png"));
				trayIcon.Visible = true;
				
				// Show/Hide the window (even from the Panel/Taskbar) when the TrayIcon has been clicked.
				trayIcon.Activate += delegate { win.Visible = !win.Visible; };
				
				if(!File.Exists("TorrentNerd.sqlite"))
					DBLogic.createSqliteSchema();			
				
				win.ShowAll(path);				
									
				Application.Run();				
			
			}
			else
			{				
				//instance allready running
			}			
			
		}		
		
		//so on linux or bsd we se TorrentNerd, not mono
		public static void SetProcessName (string name)
		{			
		    try {
				//Linux
		        if (prctl (15 /* PR_SET_NAME */, Encoding.ASCII.GetBytes (name + "\0"),
		            IntPtr.Zero, IntPtr.Zero, IntPtr.Zero) != 0) {
		            throw new ApplicationException ("Error setting process name: " +
		                Mono.Unix.Native.Stdlib.GetLastError ());
		        }
		    } catch (EntryPointNotFoundException) {
				//BSD
		        setproctitle (Encoding.ASCII.GetBytes ("%s\0"),
		            Encoding.ASCII.GetBytes (name + "\0"));
		    }
		}
	}
	
	[Interface ("org.ttsoft.torrentnerd")]
	public interface INewTorrentOne
	{
		event NewTorrentEventHandler newTorrentEvent;
		void sendPathToFirstinstance (string str);	
	}
	public interface INewTorrent : INewTorrentOne
	{
	}
	
	public delegate void NewTorrentEventHandler(string path);
	public class NewTorrent : INewTorrentOne
	{
		public event NewTorrentEventHandler newTorrentEvent;
		public void sendPathToFirstinstance (string str)
		{
			newTorrentEvent(str);
		}
	}

}

