using System.ComponentModel;
using System.Collections.Generic;
using System.Text;

namespace ServerLibs.LogSystem;

public class LoggerFormatter
{
    // LoggerItems
    private abstract class LoggerItem
    {
        public abstract string Format(LogLevel level, LogEvent e);
        protected string Content = "";

        public static LoggerItem GenerateItem(char itemType, string? format = null)
        {
            Dictionary<char, Func<LoggerItem>> typeMapping = new Dictionary<char, Func<LoggerItem>>()
            {
                {'m', () => new MessageItem()},
                {'p', () => new PrivilegeItem()},
                {'r', () => new ElapseItem()},
                {'c', () => new LoggerClassItem()},
                {'t', () => new ThreadIdItem()},
                {'F', () => new FabricIdItem()},
                {'n', () => new ConstStringItem("\n")},
                {'T', () => new ConstStringItem("\t")},
                {'l', () => new LineNumItem()},
                {'f', () => new FileNameItem()},
                {'d', () => new TimeItem(format)},
                {'N', () => new ThreadNameItem()}
            };
            return typeMapping[itemType]();
        }
    }

    private class ConstStringItem : LoggerItem
    {
        public ConstStringItem(string content = "") => Content = content;
        
        public override string Format(LogLevel level, LogEvent e)
        {
            return Content;
        }
    }

    private class MessageItem : LoggerItem
    {
        public override string Format(LogLevel level, LogEvent e)
        {
            return e.Message;
        }
    }

    private class PrivilegeItem : LoggerItem
    {
        public override string Format(LogLevel level, LogEvent e)
        {
            return level.ToString();
        }
    }

    private class ElapseItem : LoggerItem
    {
        public override string Format(LogLevel level, LogEvent e)
        {
            var milliseconds = (int)e.ElapseTime.TotalMilliseconds;
            return milliseconds.ToString();
        }
    }

    private class LoggerClassItem : LoggerItem
    {
        public override string Format(LogLevel level, LogEvent e)
        {
            return e.LoggerClass;
        }
    }

    private class ThreadIdItem : LoggerItem
    {
        public override string Format(LogLevel level, LogEvent e)
        {
            return e.ThreadId.ToString();
        }
    }

    private class FabricIdItem : LoggerItem
    {
        public override string Format(LogLevel level, LogEvent e)
        {
            return e.FabricId.ToString();
        }
    }

    private class LineNumItem : LoggerItem
    {
        public override string Format(LogLevel level, LogEvent e)
        {
            return e.LineNum.ToString();
        }
    }

    private class FileNameItem : LoggerItem
    {
        public override string Format(LogLevel level, LogEvent e)
        {
            return e.FileName;
        }
    }

    private class ThreadNameItem : LoggerItem
    {
        public override string Format(LogLevel level, LogEvent e)
        {
            return e.ThreadName;
        }
    }

    private class TimeItem : LoggerItem
    {
        private const string DefaultTimePattern = "%y-%M-%d %H:%m:%s";

        public TimeItem(string? timeFormat)
        {
            timeFormat ??= DefaultTimePattern;
            try
            {
                // Check if the time format is correct
                var now = DateTimeOffset.Now;
                var _ = now.ToString(timeFormat);
                Content = timeFormat;
            }
            catch (FormatException e)
            {
                Content = DefaultTimePattern;
            }
        }

        public override string Format(LogLevel level, LogEvent e)
        {
            return e.LogTime.ToString(Content);
        }
    }


    private const string DefaultPattern
        = "%d{%y-%M-%d %H:%m:%s}%T%t%T%N%T%F%T[%p]%T[%c]%T%f:%l%T%m%n";

    private List<LoggerItem> _loggerItems = new List<LoggerItem>();

    /// <summary>Create Logger Formatter with String Pattern</summary>
    /// <param name="pattern">The string pattern to generate formatter</param>
    public LoggerFormatter(string? pattern = null)
    {
        Init(pattern ??= DefaultPattern);
    }
    
    private enum State
    {
        // Init State
        Start,
        // State that record the string
        Record,
        // Parse the terms begin with '%'
        Parse,
        // Record the Format of certain pattern (eg. Time Format)
        Format,
        // The format is wrong
        Error,
        // End of parsing
        End
    }

    private void Init(string pattern)
    {
        // Parse the pattern with Finite State Machine
        State currentState = State.Start;
        State nextState = currentState;
        int pointer = 0;
        // Store string in record mode or format mode
        StringBuilder strBuffer = new StringBuilder();
        char parseMode = '\0';
        bool normalParse = false;
        while (pointer < pattern.Length && (!normalParse))
        {
            switch (currentState)
            {
                case State.Start:
                    if (pattern[pointer] == '%')
                    {
                        // Alter to Parse Mode
                        nextState = State.Parse;
                        pointer++;
                    }
                    else
                    {
                        // Normal Characters: record them directly
                        nextState = State.Record;
                        // Clear Buffer
                        strBuffer = strBuffer.Clear();
                    }
                    break;
                case State.Record:
                    // Record Mode, record every character until meets %
                    strBuffer.Append(pattern[pointer]);
                    pointer++;
                    if (pointer >= pattern.Length)
                    {
                        // Reach the End of the String
                        _loggerItems.Add(new ConstStringItem(strBuffer.ToString()));
                        pointer--;
                        nextState = State.End;
                    }else if (pattern[pointer] == '%')
                    {
                        // Alter to Parse Mode
                        _loggerItems.Add(new ConstStringItem(strBuffer.ToString()));
                        nextState = State.Parse;
                        pointer++;
                    }
                    break;
                case State.Parse:
                    // Parse the Mode behind the %
                    parseMode = pattern[pointer];
                    if (parseMode == '%')
                    {
                        // Alter To Record Mode because it's a normal %
                        nextState = State.Record;
                        // Clear Buffer
                        strBuffer = strBuffer.Clear();
                    }
                    else
                    {
                        // Otherwise, judge if there is additional format to parse
                        pointer++;
                        if (pointer >= pattern.Length)
                        {
                            _loggerItems.Add(LoggerItem.GenerateItem(parseMode));
                            pointer--;
                            nextState = State.End;
                        }
                        else if (pattern[pointer] == '{')
                        {
                            // Enter Format Mode to parse additional format
                            strBuffer = strBuffer.Clear();
                            nextState = State.Format;
                            pointer++;
                        }
                        else if(pattern[pointer] == '%')
                        {
                            // Enter Another Parse Mode
                            _loggerItems.Add(LoggerItem.GenerateItem(parseMode));
                            nextState = State.Parse;
                            pointer++;
                        }
                        else
                        {
                            // Back to Record Mode
                            _loggerItems.Add(LoggerItem.GenerateItem(parseMode));
                            nextState = State.Record;
                            // Clear Buffer
                            strBuffer = strBuffer.Clear();
                        }
                    }
                    break;
                case State.Format:
                    if (pattern[pointer] == '}')
                    {
                        _loggerItems.Add(LoggerItem.GenerateItem(parseMode, strBuffer.ToString()));
                        pointer++;
                        if (pointer >= pattern.Length)
                        {
                            pointer--;
                            nextState = State.End;
                        }else if (pattern[pointer] == '%')
                        {
                            nextState = State.Parse;
                            pointer++;
                        }
                        else
                        {
                            nextState = State.Record;
                            // Clear Buffer
                            strBuffer = strBuffer.Clear();
                        }
                    }
                    else
                    {
                        strBuffer.Append(pattern[pointer]);
                        pointer++;
                    }
                    break;
                case State.Error:
                    throw new FormatException("Formatter Pattern is Wrong!");
                case State.End:
                    normalParse = true;
                    break;
                    
            }

            currentState = nextState;
        }

    }

    internal string Format(LogLevel level, LogEvent e)
    {
        StringBuilder output = new StringBuilder();
        foreach (var item in _loggerItems)
        {
            output.Append(item.Format(level, e));
        }

        return output.ToString();
    }
}