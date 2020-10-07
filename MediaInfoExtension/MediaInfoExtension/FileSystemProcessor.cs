using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AR.MediaInfoExtension.Logger;
using Microsoft.WindowsAPICodePack.Shell;

namespace AR.MediaInfoExtension
{
    public class FileSystemProcessor
    {
        private readonly ITinyLogger _logger;

        private readonly string[] _masks = new string[]
        {
            "*.webm", "*.mpg", "*.mpeg", "*.mp2", "*.mpe", "*.mpv", "*.mkv",
            "*.ogg", "*.mp4", "*.m4p", "*.m4v", "*.avi", "*.wmv", "*.mov", "*.qt", "*.flv", "*.svf",
        };

        public FileSystemProcessor(ITinyLogger logger)
        {
            _logger = logger;
        }

        public TimeSpan GetTotalVideoDuration(IEnumerable<string> fsItems, out int count)
        {
            var res = TimeSpan.Zero;
            count = 0;

            var enumerable = fsItems as string[] ?? fsItems.ToArray();
            var dirs = enumerable.Where(i => File.GetAttributes(i).HasFlag(FileAttributes.Directory));
            var files = enumerable.Where(i =>
                !File.GetAttributes(i).HasFlag(FileAttributes.Directory) && _masks.Contains("*"+Path.GetExtension(i)));

            foreach (var d in dirs)
            {
                res = res.Add(GetDuration(d, true, out var cntDirs));
                count += cntDirs;
            }

            res = res.Add(GetDuration(files.ToList(), out var cntFiles));
            count += cntFiles;
            
            return res;
        }

        private TimeSpan GetDuration(string path, bool inclSub, out int count)
        {
            count = 0;
            try
            {
                var files = _masks
                    .SelectMany(m => new DirectoryInfo(path).EnumerateFiles(m, inclSub ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                    .Select(f => f.FullName)
                    .ToList();

                return GetDuration(files, out count);
            }
            catch
            {
                return TimeSpan.Zero;
            }
        }

        private TimeSpan GetDuration(IList<string> files, out int count)
        {
            count = files.Count();
            var result = TimeSpan.Zero;

            var tasks = files.Select(f => Task.Run(() => GetDuration(f))).ToArray();
            Task.WaitAll(tasks);
            return tasks.Aggregate(result, (current, t) => current.Add(t.Result));
        }

        private TimeSpan GetDuration(string file)
        {
            var tmr = Stopwatch.StartNew();
            var result = true;
            try
            {

                using (var shell = ShellObject.FromParsingName(file))
                {
                    var prop = shell.Properties.System.Media.Duration;
                    if (prop.ValueAsObject == null)
                        return TimeSpan.Zero;
                    var t = (ulong)prop.ValueAsObject;
                    return TimeSpan.FromTicks((long)t);
                }
            }
            catch
            {
                result = false;
                return TimeSpan.Zero;
            }
            finally
            {
                tmr.Stop();
                _logger.Log($"File {file} processed in {tmr.Elapsed}. Result {result}");
            }

        }
    }
}