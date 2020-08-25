using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.CompilerServices;

namespace ConvertICloudNotes
{
    public class Note
    {
        private DirectoryInfo NoteDir;
        public static string ATT_MARKER = "###ATTACHMENT*HERE###";
        private static string OBJ_MARKER = "￼";

        public Note(string NoteDirPath)
        {
            NoteDir = new DirectoryInfo(NoteDirPath);
        }

        public string GetText()
        {
            FileInfo[] txtFilesInDir = NoteDir.GetFiles("*.txt", SearchOption.TopDirectoryOnly);
            StreamReader txtFileReader = txtFilesInDir[0].OpenText();
            int nAtt = 0; 
            if (this.HasAttachments)
                nAtt = GetAttachments().Length;
            int iAtt = 0;
            String text = "";
            while (!txtFileReader.EndOfStream)
            {
                string line = txtFileReader.ReadLine();
                if (line.Contains(OBJ_MARKER) && iAtt < nAtt) //TODO!!
                {
                    line = line.Replace(OBJ_MARKER, ATT_MARKER);
                    iAtt++;
                }

                text += line + "\n";
            }
            return text;
        }

        public DateTime GetDateTime()
        {
            FileInfo[] txtFilesInDir = NoteDir.GetFiles("*.txt", SearchOption.TopDirectoryOnly);
            string filename = txtFilesInDir[0].Name;
            string dateTimeRaw = filename.Substring(filename.Length - 24, 20);
            int year = Convert.ToInt32(dateTimeRaw.Substring(0, 4));
            int month = Convert.ToInt32(dateTimeRaw.Substring(5, 2));
            int day = Convert.ToInt32(dateTimeRaw.Substring(8, 2));
            int hr = Convert.ToInt32(dateTimeRaw.Substring(11, 2));
            int m = Convert.ToInt32(dateTimeRaw.Substring(14, 2));
            int s = Convert.ToInt32(dateTimeRaw.Substring(17, 2));
            return new DateTime(year, month, day, hr, m, s);
        }

        public FileInfo[] GetAttachments()
        {
            if (this.HasAttachments)
            {
                DirectoryInfo attDir = NoteDir.GetDirectories()[0];
                return attDir.GetFiles();
            }
            else return null;
        }

        public bool HasAttachments
        {
            get { return (NoteDir.GetDirectories().Length > 0); }
        }

        public string Title
        {
            get { return NoteDir.Name; }
        }
    }
}
