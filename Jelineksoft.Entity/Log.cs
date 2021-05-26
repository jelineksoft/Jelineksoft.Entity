using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace Jelineksoft.Entity
{
    public class Log
    {
        public bool LogSQLToConsole { get; set; } = false;

        public delegate void LogSQLEventDelegate(string sql, object parameters);

        public event LogSQLEventDelegate LogSQLEvent;

        public void LogSQL(string sql, object parameters)
        {
            if (LogSQLToConsole)
            {
                var date = DateTime.Now;

                Debug.Print("Jelineksoft.Entity:");
                Debug.Print($"{date:hh:mm:ss} Generated SQL: {sql}");

                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Jelineksoft.Entity:");
                Console.WriteLine($"{date:hh:mm:ss} Generated SQL: {sql}");
            }

            if (LogSQLEvent != null)
            {
                LogSQLEvent(sql, parameters);
            }
        }

    }

}