using System;
using System.Net;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;
namespace TransmissionRPCLib
{
	public class rpcRequest{
		public string method{set;get;}
		public Dictionary<string, object> arguments{set;get;}				
	}
	
	public class ControlTransmission
	{
		private static string requestUrl="http://{0}:{1}/transmission/rpc";
		public string Host{set;get;}
		public string Port{set;get;}		
		public string sid{set;get;}		
		
		public ControlTransmission (string host, string port)
		{
			Host=host;
			Port=port;			
		}
		
		private string makeRequest(string postData)
		{
			try{	
				WebClient myWebClient = new WebClient();
				byte[] postArray = Encoding.UTF8.GetBytes(postData);
				
				if(sid!=null&&sid!="")
					myWebClient.Headers["X-Transmission-Session-Id"] = sid;
				
				string requestLink=string.Format(requestUrl, Host, Port);
				byte[] responseArray = myWebClient.UploadData(requestLink,postArray);
				
				string response = Encoding.UTF8.GetString(responseArray);
				
				return response;		
			}
			catch(Exception e)
			{
				if(sid==null&&readSessionId(e))
					 makeRequest(postData);
			}			
			return "";
		}
		
		public string addTorrent(string savePath, string torrentPath)
		{	
			rpcRequest rr= new rpcRequest();
			rr.method="torrent-add";
			Dictionary<string, object> argDic = new Dictionary<string, object>();
			argDic.Add("filename", torrentPath);
			argDic.Add("download-dir", savePath);
			rr.arguments=argDic;
			
			string postS= JsonConvert.SerializeObject(rr);
			
			return makeRequest(postS);
		}
		
		private bool readSessionId(Exception e)
		{
			WebException ex = e as WebException;
            if (ex.Response != null)
            {
                HttpWebResponse response = (HttpWebResponse)ex.Response;
                if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    try
                    {
                        string sessionid = ex.Response.Headers["X-Transmission-Session-Id"];
                        if (sessionid != null && sessionid.Length > 0)
                        {
                            sid=sessionid;
							return true;						    
						}
					}
					catch{}
				}
			}
			return false;
		}
		
		public void getData(string id)
		{
						
		}
		
		public void getTorrents()
		{
			
		}
	}
}

