using System;

namespace YouTubeApiLib.TestSearch
{
	internal class Program
	{
		static void Main(string[] args)
		{
			const string searchQuery = "coding train";
			Console.WriteLine($"Searching for \"{searchQuery}\"... Please wait!");

			YouTubeApi api = new YouTubeApi();
			YouTubeApiV1SearchResults searchResults =
				api.Search(searchQuery, null, YouTubeApiV1SearchResultFilters.Video);
			if (searchResults == null || searchResults.RawData == null)
			{
				Console.WriteLine("Error! The search result object is NULL!");
			}
			else if (searchResults.ErrorCode != 200)
			{
				Console.WriteLine($"There is error with code {searchResults.ErrorCode}!");
			}
			else
			{
				// Внимание! Сырые данные слишком длинные для вывода в консоль и могут быть обрезаны!
				// TODO: Написать могучие анализаторы для этих данных.
				Console.WriteLine(searchResults.RawData.ToString());
			}

			Console.ReadLine();
		}
	}
}
