using System.Runtime.Versioning;

namespace ServerLibs.MultiThreadsSupport;

/// <summary>
/// This class is used to synchronize the normal IOs (Stdout, Stderr, Stdin, File)
/// in a multi-thread application.
/// </summary>
public class SynchronizedIO
{
    /// In a multi-thread application, always use these instead of normal IOs
    public static readonly TextWriter Stdout = TextWriter.Synchronized(Console.Out);
    public static readonly TextWriter Stderr = TextWriter.Synchronized(Console.Error);
    public static readonly TextReader Stdin = TextReader.Synchronized(Console.In);
    
    // Synchronization to File operations
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("macos")]
    [UnsupportedOSPlatform("tvos")]
    [UnsupportedOSPlatform("freebsd")]
    public static void FileWriteLine(string path, string content)
    {
        bool lockAcquire = false;
        while (!lockAcquire)
        {
            using var fileStream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.None);
            try
            {
                fileStream.Lock(0, fileStream.Length);
                lockAcquire = true;
                using var writer = new StreamWriter(fileStream);
                writer.WriteLine(content);
                fileStream.Unlock(0, fileStream.Length);
                lockAcquire = false;
            }
            catch (IOException e)
            {
                Thread.Sleep(new Random().Next(10, 100));
            }
        }
    }
    
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("macos")]
    [UnsupportedOSPlatform("tvos")]
    [UnsupportedOSPlatform("freebsd")]
    public static void FileWrite(string path, string content)
    {
        bool lockAcquire = false;
        while (!lockAcquire)
        {
            using var fileStream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.None);
            try
            {
                fileStream.Lock(0, fileStream.Length);
                lockAcquire = true;
                using var writer = new StreamWriter(fileStream);
                writer.Write(content);
                fileStream.Unlock(0, fileStream.Length);
                lockAcquire = false;
            }
            catch (IOException e)
            {
                Thread.Sleep(new Random().Next(10, 100));
            }
        }
    }
    
    public static void FileAppend(string path, string content)
    {
        // The system will deal with thread-safe problems automatically
        bool lockAcquire = false;
        while (!lockAcquire)
        {
            try
            {
                using var writer = File.AppendText(path);
                lockAcquire = true;
                writer.Write(content);
            }
            catch (IOException e)
            {
                // wait for file to be unlocked
                Thread.Sleep(new Random().Next(10, 100));
            }
        }
    }
    
    public static void FileAppendLine(string path, string content)
    {
        // The system will deal with thread-safe problems automatically
        bool lockAcquire = false;
        while (!lockAcquire)
        {
            try
            {
                using var writer = File.AppendText(path);
                lockAcquire = true;
                writer.WriteLine(content);
            }
            catch (IOException e)
            {
                // wait for file to be unlocked
                Thread.Sleep(new Random().Next(10, 100));
            }
        }
    }
    
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("macos")]
    [UnsupportedOSPlatform("tvos")]
    [UnsupportedOSPlatform("freebsd")]
    public static string FileRead(string path)
    {
        bool lockAcquire = false;
        
        while (!lockAcquire)
        {
            using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            try
            {
                fileStream.Lock(0, fileStream.Length);
                lockAcquire = true;
                using var reader = new StreamReader(fileStream);
                // In C#, return statement will also go to finally block first
                string result =  reader.ReadToEnd();
                fileStream.Unlock(0, fileStream.Length);
                return result;
            }
            catch (IOException e)
            {
                // wait for file to be unlocked
                Thread.Sleep(new Random().Next(10, 100));
            }
        }

        return "";
    }
}