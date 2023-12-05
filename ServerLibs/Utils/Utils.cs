namespace ServerLibs.Utils;
using System.Diagnostics;

public class Utils
{
    public static string GetFileName(int skipFrame)
    {
        StackTrace st = new StackTrace(skipFrame, true);
        StackFrame? frame = st.GetFrame(0);
        if (frame is null) return "";
        string? source = frame.GetFileName();
        return source ??= "";
    }

    public static uint GetLineNum(int skipFrame)
    {
        StackTrace st = new StackTrace(skipFrame, true);
        StackFrame? frame = st.GetFrame(0);
        if (frame is null) return 0;
        uint lineNum = (uint)frame.GetFileLineNumber();
        return lineNum;
    }

    public static int GetThreadId()
    {
        return Thread.CurrentThread.ManagedThreadId;
    }

    public static string GetThreadName()
    {
        return Thread.CurrentThread.Name ??= "main";
    }
}