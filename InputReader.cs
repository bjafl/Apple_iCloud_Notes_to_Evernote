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
		private DirectoryInfo RootDir;
		private Notebook[] Notebooks;
        int position = -1;

		public InputReader(string InputRootFolderPath)
        {
			RootDir = new DirectoryInfo(InputRootFolderPath);
            DirectoryInfo[] notebookDirs = RootDir.GetDirectories();
            Notebooks = new Notebook[notebookDirs.Length];
            for (int i = 0; i < notebookDirs.Length; i++)
            {
                Notebooks[i] = new Notebook(notebookDirs[i].FullName);
            }
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
