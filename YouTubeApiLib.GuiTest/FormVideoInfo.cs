using System.Windows.Forms;

namespace YouTubeApiLib.GuiTest
{
	public partial class FormVideoInfo : Form
	{
		public FormVideoInfo(YouTubeVideo youTubeVideo)
		{
			InitializeComponent();

			SetVideoInfo(youTubeVideo);
		}

		public void SetVideoInfo(YouTubeVideo video)
		{
			string fullInfo =
				$"Название видео: {video.Title}\r\n" +
				$"ID видео: {video.Id}\r\n" +
				$"Ссылка на видео: {video.Url}\r\n" +
				$"Дата загрузки: {video.DateUploaded:dd.MM.yyyy}\r\n" +
				$"Дата публикации: {video.DatePublished:dd.MM.yyyy}\r\n" +
				$"Продолжительность видео: {video.Duration:h':'mm':'ss}\r\n" +
				$"Название канала: {video.OwnerChannelTitle}\r\n" +
				$"ID канала: {video.OwnerChannelId}\r\n" +
				$"Описание видео: {video.Description}\r\n" +
				$"Просмотры: {video.ViewCount}\r\n" +
				$"Категория: {video.Category}\r\n" +
				$"Приватное видео: {BoolToString(video.IsPrivate)}\r\n" +
				$"Доступ только по ссылке: {BoolToString(video.IsUnlisted)}\r\n" +
				$"Видео 18+: {BoolToString(!video.IsFamilySafe)}\r\n" +
				$"Видео было прямой трансляцией: {BoolToString(video.IsLiveContent)}\r\n";
			if (video.Thumbnails != null && video.Thumbnails.Count > 0)
			{
				fullInfo += "Эскизы (thumbnails):\r\n";
				foreach (YouTubeVideoThumbnail videoThumbnail in video.Thumbnails)
				{
					fullInfo += $"{videoThumbnail}\r\n";
				}
			}

			textBoxFullInfo.Text = fullInfo;
		}

		private static string BoolToString(bool flag)
		{
			return flag ? "Да" : "Нет";
		}
	}
}
