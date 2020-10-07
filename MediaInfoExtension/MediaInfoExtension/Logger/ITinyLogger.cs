namespace AR.MediaInfoExtension.Logger
{
    public interface ITinyLogger
    {
        void Log(string line);
        void FlushToFile(string fileName);
    }
}