using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Media.Imaging;
using Newtonsoft.Json.Linq;
using MultiThreadedDownloaderLib;

namespace YouTubeApiLib.GuiTestWPF
{
	internal static class Utils
	{
		internal static CookieContainer GetCookieContainer(string t)
		{
			try
			{
				JArray jaCookies = JArray.Parse(t);
				CookieContainer result = new CookieContainer();
				foreach (JObject j in jaCookies.Cast<JObject>())
				{
					result.Add(new Cookie(
						j.Value<string>("name"),
						j.Value<string>("value"),
						j.Value<string>("path"),
						j.Value<string>("domain")));
				}
				return result;
			}
			catch
			{
				return null;
			}
		}

		internal static string FormatDateTime(DateTime dateTime)
		{
			string formatted = dateTime.ToString("yyyy.MM.dd, hh:mm:ss");
			return dateTime.Kind == DateTimeKind.Utc ? $"{formatted} GMT" : formatted;
		}

		internal static BitmapImage DownloadImage(string url)
		{
			FileDownloader d = new FileDownloader() { Url = url };
			MemoryStream stream = new MemoryStream();
			if (d.Download(stream) == 200)
			{
				stream.Position = 0L;

				BitmapImage bitmapImage = new BitmapImage();
				bitmapImage.BeginInit();
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.StreamSource = stream;
				bitmapImage.EndInit();
				bitmapImage.Freeze();

				return bitmapImage;
			}
			d.Dispose();
			stream.Dispose();

			return null;
		}

		internal static void OpenUrl(string url)
		{
			Process process = new Process();
			process.StartInfo.FileName = url;
			process.Start();
		}
	}
}
