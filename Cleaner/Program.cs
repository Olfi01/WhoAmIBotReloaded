using System;
using System.IO;
using System.Threading;

namespace Cleaner
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1) throw new ArgumentNullException("directory");
            // give the program time to exit
            Thread.Sleep(1000);
            string path = args[0];
            if (File.GetAttributes(path).HasFlag(FileAttributes.Directory))
                DeleteRecursively(Directory.CreateDirectory(path));
            else File.Delete(path);
        }

        static void DeleteRecursively(DirectoryInfo directory)
        {
            foreach (var subdir in directory.EnumerateDirectories())
                DeleteRecursively(subdir);
            foreach (var file in directory.EnumerateFiles())
                file.Delete();
            directory.Delete();
        }
    }
}
