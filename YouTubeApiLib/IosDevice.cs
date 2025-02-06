
namespace YouTubeApiLib
{
	internal class IosDevice
	{
		public string DeviceMaker { get; }
		public string DeviceModel { get; }
		public string OsName { get; }
		public ushort OsVersionMajor { get; }
		public ushort OsVersionMinor { get; }
		public ushort OsVersionPatch { get; }
		public string OsVersionBuild { get; }
		public string ClientName { get; }
		public ushort ClientVersionMajor { get; }
		public ushort ClientVersionMinor { get; }
		public ushort ClientVersionPatch { get; }

		public readonly string UserAgent;

		public IosDevice(string deviceMaker, string deviceModel, string osName,
			ushort osVersionMajor, ushort osVersionMinor, ushort osVersionPatch,
			string osVersionBuild, string clientName, ushort clientVersionMajor,
			ushort clientVersionMinor, ushort clientVersionPatch)
		{
			DeviceMaker = deviceMaker;
			DeviceModel = deviceModel;
			OsName = osName;
			OsVersionMajor = osVersionMajor;
			OsVersionMinor = osVersionMinor;
			OsVersionPatch = osVersionPatch;
			OsVersionBuild = osVersionBuild;
			ClientName = clientName;
			ClientVersionMajor = clientVersionMajor;
			ClientVersionMinor = clientVersionMinor;
			ClientVersionPatch = clientVersionPatch;

			UserAgent = $"com.google.ios.youtube/{clientVersionMajor}.{clientVersionMinor}.{clientVersionPatch} " +
				$"({deviceModel}; U; CPU iOS {osVersionMajor}_{osVersionMinor}_{osVersionPatch} like Mac OS X; US)";
		}
	}
}
