using System.IO;

namespace TownOfHost.Modules
{
    public static class ModTempData
    {
        public const string RelativePath = "./TOH_DATA/Temp/";
        public static readonly DirectoryInfo TempDirectory = new(RelativePath);
        private static readonly LogHandler logger = Logger.Handler(nameof(ModTempData));

        public static void Clear()
        {
            logger.Info("一時ファイルクリーンアップ");
            if (TempDirectory.Exists)
            {
                TempDirectory.Delete(true);
            }
            TempDirectory.Create();
        }
        public static DirectoryInfo GetSubDirectory(string path)
        {
            var directoryInfo = new DirectoryInfo($"{RelativePath}{path}");
            if (!directoryInfo.Exists)
            {
                logger.Info($"一時ディレクトリ作成: {path}");
                directoryInfo.Create();
            }
            return directoryInfo;
        }
    }
}
