
namespace YouTubeApiLib
{
	public static class YouTubeChannelTabPages
	{
		public static readonly TabPageHome Home = new TabPageHome("Home", "EghmZWF0dXJlZA%3D%3D");
		public static readonly TabPageVideos Videos = new TabPageVideos("Videos", "EgZ2aWRlb3MYAyAAMAE%3D");
		public static readonly TabPageShorts Shorts = new TabPageShorts("Shorts", "EgZzaG9ydHPyBgUKA5oBAA%3D%3D");
		public static readonly TabPageLive Live = new TabPageLive("Live", "EgdzdHJlYW1z8gYECgJ6AA%3D%3D", "streams");
		public static readonly TabPagePlaylists Playlists = new TabPagePlaylists("Playlists", "EglwbGF5bGlzdHPyBgQKAkIA");
		public static readonly TabPageCommunity Community = new TabPageCommunity("Community", "Egljb21tdW5pdHk%3D");
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

	public class TabPageCommunity : YouTubeChannelTabPage
	{
		public TabPageCommunity(string title, string paramsId, string urlSuffix = null)
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
