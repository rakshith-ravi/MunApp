using System;
using System.IO;
using System.Collections.Generic;

namespace MunApp.Common
{
    public enum DebugType { Message, Error, Warning, Exception };

    public sealed class Debug
    {
        private static List<Debug> debugs = new List<Debug>();

        private DateTime time;
        private DebugType level;
        private string message;
        private Exception exception;

        public DebugType Level
        {
            get
            {
                return level;
            }
        }

        public string Message
        {
            get
            {
                return message;
            }
        }

        public Exception Exception
        {
            get
            {
                return exception;
            }
        }

        public DateTime Time
        {
            get
            {
                return time;
            }
        }

        public static void Log(string message)
        {
            debugs.Add(new Debug(message));
        }
        public static void Log(Exception ex)
        {
            debugs.Add(new Debug(ex));
            LogToFile();
        }

        private static void LogToFile()
        {
            string toWrite = "";
#if DEBUG
            int type = 0;
#else
            int type = Random.Next(1, Data.EncryptionCount + 1);
#endif
            toWrite += type.ToString() + Environment.NewLine;
            foreach (Debug d in debugs)
            {
                string line = string.Empty;
                switch (d.Level)
                {
                    case DebugType.Exception:
                        line = Data.Encrypt(d.Level + " on " + d.Time + ". Type : " + d.Exception.GetType() + Environment.NewLine + "Stack Trace : " + d.Exception.StackTrace + Environment.NewLine + "Data : " + d.Exception.Data, type);
                        break;
                    case DebugType.Error:
                    case DebugType.Message:
                    case DebugType.Warning:
                    default:
                        line = Data.Encrypt(d.Level + " on " + d.Time + " having message : " + d.Message, type);
                        break;
                }
                toWrite += line + Environment.NewLine;
            }
            File.WriteAllText("ErrorLog.dat", toWrite);
        }

        public static void Log(DebugType level, string message)
        {
            debugs.Add(new Debug(level, message));
        }

        private Debug(string message)
        {
            level = DebugType.Message;
            this.message = message;
            time = DateTime.Now;
        }
        private Debug(Exception exception)
        {
            this.exception = exception;
            level = DebugType.Exception;
            time = DateTime.Now;
        }
        private Debug(DebugType level, string message)
        {
            this.level = level;
            this.message = message;
            time = DateTime.Now;
        }
    }
}
