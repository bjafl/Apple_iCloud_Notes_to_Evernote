using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.CompilerServices;

namespace ConvertICloudNotes
{
    public class Note
    {
        public static readonly string ATT_MARKER = "###ATTACHMENT*HERE###";
        private static readonly string OBJ_MARKER = "￼";

        public DirectoryInfo Dir { get; private set; }
        public string Text { get; private set; }
        public FileInfo[] Attachments { get; private set; }
        public DateTime DateTime { get; private set; }

        public Note(DirectoryInfo NoteDir)
        {
            Dir = NoteDir;

            //--- Find and store a list of any attachments
            if (HasAttachments)
            {
                DirectoryInfo attDir = NoteDir.GetDirectories()[0];
                Attachments = attDir.GetFiles();
            }

            //--- Read and store note text 
            FileInfo[] txtFilesInDir = NoteDir.GetFiles("*.txt", SearchOption.TopDirectoryOnly);
            FileInfo noteTxtFile = txtFilesInDir[0];
            StreamReader txtFileReader = noteTxtFile.OpenText();
            int nAtt = 0;
            if (HasAttachments)
                nAtt = Attachments.Length;
            int iAtt = 0;
            Text = "";
            while (!txtFileReader.EndOfStream)
            {
                string line = txtFileReader.ReadLine();

                // Find locations of any attachments in the text and mark them.
                if (line.Contains(OBJ_MARKER) && iAtt < nAtt)
                {
                    line = line.Replace(OBJ_MARKER, ATT_MARKER);
                    iAtt++;
                }

                Text += line + "\n";
            }

            //--- Find and store note creation date and time.
            string filename = noteTxtFile.Name;
            string dateTimeRaw = filename.Substring(filename.Length - 24, 20);
            int year = Convert.ToInt32(dateTimeRaw.Substring(0, 4));
            int month = Convert.ToInt32(dateTimeRaw.Substring(5, 2));
            int day = Convert.ToInt32(dateTimeRaw.Substring(8, 2));
            int hr = Convert.ToInt32(dateTimeRaw.Substring(11, 2));
            int m = Convert.ToInt32(dateTimeRaw.Substring(14, 2));
            int s = Convert.ToInt32(dateTimeRaw.Substring(17, 2));
            DateTime = new DateTime(year, month, day, hr, m, s);
        }

        public bool HasAttachments
        {
            get { return (Dir.GetDirectories().Length > 0); }
        }
        public string Title
        {
            get { return Dir.Name; }
        }


    }
}
