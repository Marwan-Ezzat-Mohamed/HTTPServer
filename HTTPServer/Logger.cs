using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace HTTPServer
{
    class Logger
    {
        //static StreamWriter sr = new StreamWriter("log.txt");
        public static void LogException(Exception ex)
        {
            // TODO: Create log file named log.txt to log exception details in it
            // for each exception write its details associated with datetime 
            DateTime date = DateTime.Now;
            string[] message = { ex.Message, date.ToString() };
            File.WriteAllLines("log.txt", message);
        }
    }
}
