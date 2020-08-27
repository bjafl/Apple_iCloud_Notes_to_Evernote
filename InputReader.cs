using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ConvertICloudNotes
{
    public class InputReader : IEnumerator,IEnumerable
    {
        //
		private readonly DirectoryInfo RootDir;
		private readonly Notebook[] Notebooks;
        int position = -1;

		public InputReader(DirectoryInfo InputRootDir)
        {
			RootDir = InputRootDir;
            DirectoryInfo[] notebookDirs = RootDir.GetDirectories();
            Notebooks = new Notebook[notebookDirs.Length];
            for (int i = 0; i < notebookDirs.Length; i++)
            {
                Notebooks[i] = new Notebook(notebookDirs[i]);
            }
        }

        public int NotebookCount
        {
            get { return Notebooks.Length; }
        }

        public int NotesCount()
        {
            int count = 0;
            foreach (Notebook notebook in Notebooks)
            {
                count += notebook.NoteCount;
            }
            return count;
        }

        public IEnumerator GetEnumerator()
        {
            return (IEnumerator)this;
        }
        public bool MoveNext()
        {
            position++;
            return (position < Notebooks.Length);
        }
        public void Reset()
        {
            position = 0;
        }
        public object Current
        {
            get { return Notebooks[position]; }
        }


    }
}
