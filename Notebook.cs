using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text;

namespace ConvertICloudNotes
{
    public class Notebook : IEnumerator,IEnumerable
    {
        public DirectoryInfo Dir { get; private set; }
        private readonly Note[] Notes;
        int position = -1; 

        public Notebook(DirectoryInfo NotebookDir)
        {
            Dir = NotebookDir;
            DirectoryInfo[] noteDirs = Dir.GetDirectories();
            Notes = new Note[noteDirs.Length];
            for (int i = 0; i < noteDirs.Length; i++)
            {
                Notes[i] = new Note(noteDirs[i]);
            }
        }

        public string Name
        {
            get { return Dir.Name; }
        }

        public int NoteCount
        {
            get { return Notes.Length; }
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
