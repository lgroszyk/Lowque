using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Lowque.DataAccess.Utils
{
    public static class IoExtensions
    {
        public static void ClearDirectory(string path)
        {
            var dir = new DirectoryInfo(path);

            foreach (var file in dir.GetFiles())
            {
                file.Delete();
            }
            foreach (var subdir in dir.GetDirectories())
            {
                subdir.Delete(true);
            }
        }

        public static void CopyDirectory(string sourcePath, string targetPath)
        {
            CopyDirectory(sourcePath, targetPath, Enumerable.Empty<string>());
        }

        public static void CopyDirectory(string sourcePath, string targetPath, IEnumerable<string> ignored)
        {
            var dir = new DirectoryInfo(sourcePath);

            Directory.CreateDirectory(targetPath);

            foreach (var file in dir.GetFiles())
            {
                if (ignored.Any(phrase => file.Name.Contains(phrase)))
                {
                    continue;
                }
                var tempPath = Path.Combine(targetPath, file.Name);
                file.CopyTo(tempPath, false);
            }

            foreach (var subdir in dir.GetDirectories())
            {
                if (ignored.Any(phrase => subdir.Name.Contains(phrase)))
                {
                    continue;
                }
                var tempPath = Path.Combine(targetPath, subdir.Name);
                CopyDirectory(subdir.FullName, tempPath, ignored);
            }
        }
    }
}
