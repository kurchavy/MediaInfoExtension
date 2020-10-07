using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;
using System.Diagnostics;
using AR.MediaInfoExtension.Logger;

namespace AR.MediaInfoExtension
{
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.AllFilesAndFolders)]
    public class MediaInfoContextMenu : SharpContextMenu
    {
        private readonly string _header = "MediaInfo ShellExtension " +
                                          typeof(MediaInfoContextMenu).Assembly.GetName().Version.ToString();
        private readonly ITinyLogger _logger;
        private readonly FileSystemProcessor _proc;

        public MediaInfoContextMenu()
        {
#if DEBUG
            _logger = new StringBuilderLogger();
#else
            _logger = new EmptyLogger();
#endif
            _proc = new FileSystemProcessor(_logger);
        }

        protected override bool CanShowMenu()
        {
            return true;
        }

        protected override ContextMenuStrip CreateMenu()
        {
            var menu = new ContextMenuStrip();

            var itemVd = new ToolStripMenuItem
            {
                Text = "Video duration",
            };
            itemVd.Click += (sender, args) =>
            {
                Task.Run(() =>
                {
                    var timer = Stopwatch.StartNew();
                    var duration = _proc.GetTotalVideoDuration(SelectedItemPaths, out var count);
                    MessageBox.Show(
                        $"Selection contains:{Environment.NewLine}{count} video file(s){Environment.NewLine}Total duration: {(int) duration.TotalHours} hours and {duration.Minutes} minutes{Environment.NewLine}Time to get info: {timer.ElapsedMilliseconds} ms",
                        _header, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _logger.FlushToFile(@"d:\temp\milog.log");
                });
            };

            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(itemVd);
            menu.Items.Add(new ToolStripSeparator());
            return menu;
        }
    }
}