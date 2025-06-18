using Newtonsoft.Json.Linq;

namespace YouTubeApiLib
{
	public class YouTubeVideoThumbnail
	{
		public ushort Width { get; }
		public ushort Height { get; }
		public string FileName { get; }
		public string Url { get; }

		public YouTubeVideoThumbnail(ushort width, ushort height, string fileName, string url)
		{
			Width = width;
			Height = height;
			FileName = fileName;
			Url = url;
		}

		public JObject ToJson()
		{
			JObject json = new JObject()
			{
				["width"] = Width,
				["height"] = Height,
				["fileName"] = FileName,
				["url"] = Url
			};
			return json;
		}

		public override string ToString()
		{
			return $"{Width}x{Height} | {FileName} | {Url}";
		}
	}
}
