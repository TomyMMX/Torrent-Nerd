using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;

namespace SubtitleDownloaderLib
{
   public static class unzipfromDownload
   {
	 public static void UnzipStream(Stream zipStream, string outFolder) {
		 ZipInputStream zipInputStream = new ZipInputStream(zipStream);
		 ZipEntry zipEntry = zipInputStream.GetNextEntry();
		 while (zipEntry != null) {
			 String entryFileName = zipEntry.Name;
			 // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
			 // Optionally match entrynames against a selection list here to skip as desired.
			 // The unpacked length is available in the zipEntry.Size property.

			 byte[] buffer = new byte[4096];		// 4K is optimum

			 // Manipulate the output filename here as desired.
			 String fullZipToPath = Path.Combine(outFolder, entryFileName);
			 string directoryName = Path.GetDirectoryName(fullZipToPath);
			 if (directoryName.Length > 0)
				 Directory.CreateDirectory(directoryName);

			 // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
			 // of the file, but does not waste memory.
			 // The "using" will close the stream even if an exception occurs.
			 using (FileStream streamWriter = File.Create(fullZipToPath)) {
				 StreamUtils.Copy(zipInputStream, streamWriter, buffer);
			 }
			 zipEntry = zipInputStream.GetNextEntry();
		 }
	 }
   }
}
