using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neural_Application.Interfaces
{
    public static class Log 
    {
        public enum LogCategories 
                            {   HARDWARE, 
                                ALGORYTHM, 
                                MCU };

        public struct LogItem
        {
            public LogCategories category;
            public string message;
        }

        private static List<LogItem> logList = new List<LogItem>();
       
        public delegate void LogAdditional();
        public static event LogAdditional onAdditional;

        //добавим в лог
        public static void Add(LogCategories category, String message)
        {
            LogItem currentItem;
            currentItem.category = category;
            currentItem.message = "[" + DateTime.Now + "] " + message;

            logList.Add(currentItem);
               onAdditional();

        }
        public static List<LogItem> GetLog()
        {
            return logList;
        }
    }
}
