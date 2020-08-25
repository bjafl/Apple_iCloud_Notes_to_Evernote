using System;
using System.Collections.Generic;

namespace ConvertICloudNotes
{
    class Program
    {
        static string InputFolder = @"C:\Users\bjart\Lokale filer\iCloud Notes";
        static string OutputFolder = @"C:\Users\bjart\Lokale filer\Konvertert";
        static void Main(string[] args)
        {
            

			Console.WriteLine("Done!");
            Console.WriteLine("");
			Console.ReadKey();
        }

        static void ConvertNotes(string InputFolder, string OutputFolder)
        {
            InputReader input = new InputReader(InputFolder);

            foreach (Notebook notebook in input)
            {
                OutputWriter output = new OutputWriter(OutputFolder, notebook.Name);
                foreach (Note note in notebook)
                {
                    output.WriteNote(note);
                }
                output.Finish();
            }
        }
		

    }
}
