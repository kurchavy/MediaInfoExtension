using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;
using Microsoft.WindowsAPICodePack.Shell;

namespace AR.MediaInfoExtension
{
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.Directory)]
    public class MediaInfoContextMenu : SharpContextMenu
    {
        private readonly string _header = "MediaInfo ShellExtension " + typeof(MediaInfoContextMenu).Assembly.GetName().Version.ToString();
        private readonly List<string> _masks = new List<string>()
                {
                    "*.webm", "*.mpg", "*.mpeg", "*.mp2", "*.mpe", "*.mpv", "*.mkv",
                    "*.ogg", "*.mp4", "*.m4p", "*.m4v", "*.avi", "*.wmv", "*.mov", "*.qt", "*.flv", "*.svf",
                };

        public MediaInfoContextMenu()
        {

        }

        protected override bool CanShowMenu()
        {
            return SelectedItemPaths.Select(File.GetAttributes).All(attr => attr.HasFlag(FileAttributes.Directory));
        }

        protected override ContextMenuStrip CreateMenu()
        {
            var menu = new ContextMenuStrip();

            var itemVd = new ToolStripMenuItem
            {
                Text = "Video duration",
            };
            var wosub = new ToolStripMenuItem
            {
                Text = "... not including subdirs",
            };
            wosub.Click += (sender, args) => ProcessFolders(false);

            var withsub = new ToolStripMenuItem
            {
                Text = "... including subdirs",
            };
            withsub.Click += (sender, args) => ProcessFolders(true);

            itemVd.DropDownItems.Add(wosub);
            itemVd.DropDownItems.Add(withsub);

            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(itemVd);
            menu.Items.Add(new ToolStripSeparator());
            return menu;
        }

        private void ProcessFolders(bool inclSub)
        {
            var res = TimeSpan.Zero;
            var count = 0;


            Task.Run(() =>
            {
                foreach (var folder in SelectedItemPaths)
                {
                    res = res.Add(ProcessFolder(folder, inclSub, out var cnt));
                    count += cnt;
                }
                MessageBox.Show($"Selected folder(s) contains:{Environment.NewLine}Total {count} video files{Environment.NewLine}Duration: {(int)res.TotalHours} hours and {res.Minutes} minutes{Environment.NewLine}",
                    _header, MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
        }


        private TimeSpan ProcessFolder(string path, bool inclSub, out int count)
        {
            count = 0;
            try
            {
                var directory = new DirectoryInfo(path);

                var result = TimeSpan.Zero;
                var files = _masks
                    .SelectMany(m => directory.EnumerateFiles(m, inclSub ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                    .ToList();

                count = files.Count();

                return files.Select(f => GetVideoDuration(f.FullName)).Aggregate(result, (acc, ts) => acc.Add(ts));
            }
            catch
            {
                return TimeSpan.Zero;
            }
        }

        private TimeSpan GetVideoDuration(string filePath)
        {
            try
            {
                using (var shell = ShellObject.FromParsingName(filePath))
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
                return TimeSpan.Zero;
            }

        }
    }
}