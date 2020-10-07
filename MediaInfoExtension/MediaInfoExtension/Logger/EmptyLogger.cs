namespace AR.MediaInfoExtension.Logger
{
    public class EmptyLogger : ITinyLogger
    {
        public void Log(string line)
        {
            // nothing to do
        }

        public void FlushToFile(string fileName)
        {
            // nothing to do
        }
    }
}