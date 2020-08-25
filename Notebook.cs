using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text;

namespace ConvertICloudNotes
{
    public class Notebook : IEnumerator,IEnumerable
    {
        private DirectoryInfo NotebookDir;
        private Note[] Notes;
        int position = -1; 

        public Notebook(string NotebookPath)
        {
            NotebookDir = new DirectoryInfo(NotebookPath);
            DirectoryInfo[] noteDirs = NotebookDir.GetDirectories();
            Notes = new Note[noteDirs.Length];
            for (int i = 0; i < noteDirs.Length; i++)
            {
                Notes[i] = new Note(noteDirs[i].FullName);
            }
        }

        public string Name
        {
            get { return NotebookDir.Name; }
        }

        public IEnumerator GetEnumerator()
        {
            return (IEnumerator)this;
        }
        public bool MoveNext()
        {
            position++;
            return (position < Notes.Length);
        }
        public void Reset()
        {
            position = 0;
        }
        public object Current
        {
            get { return Notes[position]; }
        }
    }
}
