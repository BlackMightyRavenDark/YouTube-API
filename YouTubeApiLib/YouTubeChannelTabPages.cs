
namespace YouTubeApiLib
{
	public static class YouTubeChannelTabPages
	{
		public static readonly TabPageHome Home = new TabPageHome("Home", "EghmZWF0dXJlZPIGBAoCMgA%3D");
		public static readonly TabPageVideos Videos = new TabPageVideos("Videos", "EgZ2aWRlb3PyBgQKAjoA");
		public static readonly TabPageShorts Shorts = new TabPageShorts("Shorts", "EgZzaG9ydHPyBgUKA5oBAA%3D%3D");
		public static readonly TabPageLive Live = new TabPageLive("Live", "EgdzdHJlYW1z8gYECgJ6AA%3D%3D");
		public static readonly TabPagePlaylists Playlists = new TabPagePlaylists("Playlists", "EglwbGF5bGlzdHPyBgQKAkIA");
		public static readonly TabPagePosts Posts = new TabPagePosts("Posts", "EgVwb3N0c_IGBAoCSgA%3D");

		public static YouTubeChannelTabPage PageFromParams(string paramsId)
		{
			if (paramsId == Home.ParamsId) { return Home; }
			else if (paramsId == Videos.ParamsId) { return Videos; }
			else if (paramsId == Shorts.ParamsId) { return Shorts; }
			else if (paramsId == Live.ParamsId) { return Live; }
			else if (paramsId == Playlists.ParamsId) { return Playlists; }
			else if (paramsId == Posts.ParamsId) { return Posts; }
			else
			{
				return Home;
			}
		}
	}

	public class TabPageHome : YouTubeChannelTabPage
	{
		public TabPageHome(string title, string paramsId, string urlSuffix = null)
		{
			Title = title;
			ParamsId = paramsId;
			UrlSuffix = urlSuffix;
		}
	}

	public class TabPageVideos : YouTubeChannelTabPage
	{
		public TabPageVideos(string title, string paramsId, string urlSuffix = null)
		{
			Title = title;
			ParamsId = paramsId;
			UrlSuffix = urlSuffix;
		}
	}

	public class TabPageShorts : YouTubeChannelTabPage
	{
		public TabPageShorts(string title, string paramsId, string urlSuffix = null)
		{
			Title = title;
			ParamsId = paramsId;
			UrlSuffix = urlSuffix;
		}
	}

	public class TabPageLive : YouTubeChannelTabPage
	{
		public TabPageLive(string title, string paramsId, string urlSuffix = null)
		{
			Title = title;
			ParamsId = paramsId;
			UrlSuffix = urlSuffix;
		}
	}

	public class TabPagePlaylists : YouTubeChannelTabPage
	{
		public TabPagePlaylists(string title, string paramsId, string urlSuffix = null)
		{
			Title = title;
			ParamsId = paramsId;
			UrlSuffix = urlSuffix;
		}
	}

	public class TabPagePosts : YouTubeChannelTabPage
	{
		public TabPagePosts(string title, string paramsId, string urlSuffix = null)
		{
			Title = title;
			ParamsId = paramsId;
			UrlSuffix = urlSuffix;
		}
	}

	public class TabPageChannels : YouTubeChannelTabPage
	{
		public TabPageChannels(string title, string paramsId, string urlSuffix = null)
		{
			Title = title;
			ParamsId = paramsId;
			UrlSuffix = urlSuffix;
		}
	}

	public class TabPageAbout : YouTubeChannelTabPage
	{
		public TabPageAbout(string title, string paramsId, string urlSuffix = null)
		{
			Title = title;
			ParamsId = paramsId;
			UrlSuffix = urlSuffix;
		}
	}
}
