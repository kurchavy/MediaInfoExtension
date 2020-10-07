using System;
using System.IO;
using System.Text;

namespace AR.MediaInfoExtension.Logger
{
    public class StringBuilderLogger : ITinyLogger
    {
        private readonly StringBuilder _log = new StringBuilder();
        public void Log(string line)
        {
            _log.AppendLine(line);
        }

        public void FlushToFile(string fileName)
        {
            try
            {
                File.WriteAllText(fileName, _log.ToString());
                _log.Clear();
            }
            catch
            {
                // nothing to do
            }
        }
    }
}