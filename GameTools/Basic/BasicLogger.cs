using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameTools.Basic
{
    public class BasicLogger
    {
        private string LogFile;
        private StreamWriter w;

        public BasicLogger(string tmpFile)
        {
            if (!File.Exists(tmpFile))
            {
                FileStream fs = File.Create(tmpFile);
                fs.Close();
            }
            this.LogFile = tmpFile;
            
        }

        public void Log(string logEntry)
        {
            this.w = File.AppendText(this.LogFile);
            w.Write("\rLog Entry : ");
            w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                DateTime.Now.ToLongDateString() + " : " + logEntry);
            w.Close();
        }

        public void Log(string logEntry, bool enabled)
        {
            if (enabled)
            {
                this.w = File.AppendText(this.LogFile);
                w.Write("\rLog Entry : ");
                w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString() + " : " + logEntry);
                w.Close();
            }
        }
    }
}
