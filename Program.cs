using System;
using System.CommandLine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.CommandLine.Invocation;

namespace ConvertICloudNotes
{
    class Program
    {
        private static readonly string NOTES_STD_ROOT_DIR_NAME = "iCloud Notes";

        static void Main(params string[] args)
        {
            //--- Set up command line options
            string version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
            string description = "This tool converts Apple Notes from iCloud backups to " +
                                 "Evernote Note Export format (*.enex). You can run the tool using the " +
                                 "following flags, or you can leave them out and answer the prompts. " +
                                 "For a complete readme and sourcecode, check out " +
                                 "https://github.com/bjafl/ConvertICloudNotes";
            RootCommand rootCommand = new RootCommand(description);
            rootCommand.AddOption(new Option(new[] { "--input", "-i" })
            {
                Name = "InputFolder",
                Description = "Folder containing your unzipped iCloud Notes backup.",
                Argument = new Argument<DirectoryInfo>()
            });
            rootCommand.AddOption(new Option(new[] { "--output", "-o" })
            {
                Name = "OutputFolder",
                Description = "Folder to put the converted notes.",
                Argument = new Argument<DirectoryInfo>()
            });
            rootCommand.AddOption(new Option(new[] { "--verbose", "-v" })
            {
                Name = "Verbose",
                Description = "Display extra info during conversion."
            });
            rootCommand.AddOption(new Option(new[] { "--replace", "-r" })
            {
                Name = "Replace",
                Description = "Allows existing .enex files in the output folder to be replaced."
            });
            rootCommand.Handler = CommandHandler.Create<DirectoryInfo, DirectoryInfo, bool, bool>(CmdHandler);
            rootCommand.Invoke(args);
            
            //---Keep cmd window open if program was started using no args.
            if(args.Length == 0)
            {
                Console.WriteLine("\nPress any key to exit....");
                Console.ReadKey();
            }
        }

        static void CmdHandler(DirectoryInfo InputFolder, DirectoryInfo OutputFolder, bool Verbose, bool Replace)
        {
            Console.WriteLine(new String('*', Console.WindowWidth));
            string version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
            Console.WriteLine("Apple notes to Evernote converter version {0}\n", version);
            Console.WriteLine("This tool converts Apple Notes from iCloud backups to Evernote Note Export format (*.enex)");
            Console.WriteLine("Run this tool with the parameter -? for more information.");
            Console.WriteLine(new String('*', Console.WindowWidth));

            try
            {
                DirectoryInfo validInDir = HandleInputFolder(InputFolder);
                DirectoryInfo validOutDir = HandleOutputFolder(OutputFolder);

                // Ask to replace files if output folder was not provided by cmd args
                if (OutputFolder == null)
                {
                    FileInfo[] enexInOutDir = validOutDir.GetFiles().Where(f => f.Name.Contains(".enex")).ToArray();
                    if ((enexInOutDir.Length > 0) & !Replace)
                        Replace = AskReplace();
                }
                // Ask if user wants to enable verbose if output or input folder was not provided by cmd args
                if (OutputFolder == null | InputFolder == null)
                {
                    if (!Verbose)
                        Verbose = AskVerbose();
                }
                ConvertNotes(validInDir, validOutDir, Verbose, Replace);
            }
            catch (Exception e)
            {
                Console.WriteLine("\n");
                Console.WriteLine(new String('_', Console.WindowWidth));
                Console.WriteLine("ERROR:\n" + e.Message);
                Console.WriteLine("\nThe program will now exit.");
            }
        }

        static int GetAnswer()
        {
            string a = Console.ReadLine();
            if (char.ToLower(a[0]) == 'y')
                return 1;
            if (char.ToLower(a[0]) == 'n')
                return 0;
            return GetAnswer();
        }
        static bool AskVerbose()
        {
            Console.WriteLine("Would you like to display extra progress " +
                              "information and extended error reports? (Y/N)");
            if (GetAnswer() == 1)
                return true;
            else
                return false;
        }
        static bool AskReplace()
        {
            Console.WriteLine("The provided output folder already contains .enex files.\n" +
                              "The existing .enex files with matching filenames can be replaced " +
                              "or the new .enex files can be named '[notebookName] (n).enex'\n" +
                              "Would you like to replace existing files? (Y/N)") ;
            if (GetAnswer() == 1)
                return true;
            else
                return false;
        }

        static DirectoryInfo HandleInputFolder(DirectoryInfo TryDir = null)
        {
            if (TryDir != null) 
            {
                if (TryDir.Exists)
                    return TryDir;
                else
                {
                    throw new Exception(String.Format("Can't find the input folder '{0}'.",
                                                       TryDir.FullName));
                }
            }

            DirectoryInfo curDir = new DirectoryInfo(Directory.GetCurrentDirectory());
            DirectoryInfo inputFolder = new DirectoryInfo(curDir.FullName + "\\" + NOTES_STD_ROOT_DIR_NAME);
            if (inputFolder.Exists)
            {
                Console.WriteLine("A folder that looks like a backup folder for iCloud Notes was found.");
                Console.WriteLine("Would you like to use '{0}' as your input folder? (Y/N)", 
                                    inputFolder.FullName);
                int response = GetAnswer();
                if (response == 1)
                    return inputFolder;
            }
            Console.WriteLine("\nPlease type the path of your input folder:");
            string inputPath = Console.ReadLine();
            while (!Directory.Exists(inputPath))
            {
                Console.WriteLine("The folder {0} does not exist. Please try again:", inputPath);
                inputPath = Console.ReadLine();
            }
            return inputFolder;
        }
        static DirectoryInfo HandleOutputFolder(DirectoryInfo TryDir)
        {
            if (TryDir != null)
            {
                if (!TryDir.Exists)
                {
                    try { TryDir.Create(); }
                    catch
                    {
                        throw new Exception(String.Format("Can't create output folder '{0}'.",
                                                           TryDir.FullName));
                    }
                }
                return TryDir;
            }

            DirectoryInfo curDir = new DirectoryInfo(Directory.GetCurrentDirectory());
            DirectoryInfo outputFolder = new DirectoryInfo(curDir.FullName + "\\" + "Converted notes");
            Console.WriteLine("\nWould you like to use '{0}' as your output folder? (Y/N)",
                                outputFolder.FullName);
            if (GetAnswer() == 0)
                outputFolder = RequestOutputFolder();
            return HandleOutputFolder(outputFolder);
        }
        static DirectoryInfo RequestOutputFolder()
        {
            Console.WriteLine("\nPlease type the path of your output folder:");
            DirectoryInfo outputFolder = new DirectoryInfo(Console.ReadLine());
            try
            {
                return HandleOutputFolder(outputFolder);
            }
            catch
            {
                Console.WriteLine("Couldn't create the folder '{0}.\n Please try again...",
                                   outputFolder.FullName);
                return RequestOutputFolder();
            }
        }

        static void ConvertNotes(DirectoryInfo InputFolder, DirectoryInfo OutputFolder, bool Verbose, bool Replace)
        {

            InputReader input;
            try
            {
                input = new InputReader(InputFolder);
            }
            catch (Exception e)
            {
                string eText = "An error occured while reading the notes. " +
                               "Please check if the folder '" + InputFolder.FullName +
                               "' contains the supported directory structure with correctly formatted notes.";
                if (Verbose)
                    eText += "\n\nError message: " + e.Message;
                throw new Exception(eText);
            }

            Console.WriteLine("\nConverting notes from\n    '{0}'\nto\n    '{1}'", InputFolder, OutputFolder); 
            Console.WriteLine("\nPlease wait...\n");

            int totNotes = input.NotesCount();
            int notesFinished = 0;
            foreach (Notebook notebook in input)
            {
                string msg = "";
                if (Verbose)
                {
                    msg = String.Format("Converting notebook '{0}'.\n", notebook.Name);
                    msg += String.Format("Creating output file '{0}.enex' in " + 
                                         "'{1}'......", notebook.Name, OutputFolder.FullName);
                    WriteProgressCmd(notesFinished, totNotes, msg);
                }
                OutputWriter output;
                try
                {
                    output = new OutputWriter(OutputFolder.FullName, notebook.Name, Replace);
                    if (Verbose)
                        WriteProgressCmd(notesFinished, totNotes, "DONE!\n");
                }
                catch (Exception e)
                {
                    string eText = String.Format("An error occured trying to create the " +
                                                 "outputfile '{0}.enex' in '{1}'.",
                                                 notebook.Name, OutputFolder.FullName);
                    if (Verbose)
                        eText += "\n\nError message: " + e.Message;
                    throw new Exception(eText);
                }
                foreach (Note note in notebook)
                {
                    if (Verbose)
                    {
                        msg = String.Format("Converting note '{0}' " + 
                                            "and writing it to '{1}'......",
                                            note.Title, output.OutputFile.Name);
                        WriteProgressCmd(notesFinished, totNotes, msg);
                    }
                    try 
                    { 
                        output.WriteNote(note);
                        notesFinished++;
                        if (Verbose)
                            WriteProgressCmd(notesFinished, totNotes, "DONE!\n");
                        else
                            WriteProgressCmd(notesFinished, totNotes);
                    }
                    catch (Exception e)
                    {
                        string eText = String.Format("An error occured trying to convert the " +
                                                     "note '{0}' in the notebook " +
                                                     "'{1}', and write it to '{2}'.",
                                                     note.Title, notebook.Name, output.OutputFile.Name);
                        if (Verbose)
                        {
                            eText += String.Format("\nOutput file path: '{0}'", output.OutputFile.FullName);
                            eText += "\n\nError message: " + e.Message;
                        }
                        throw new Exception(eText);
                    }
                }
                if (Verbose)
                {
                    msg = String.Format("Writing end of file, saving and closing '{0}'......",
                                        output.OutputFile.Name);
                    WriteProgressCmd(notesFinished, totNotes, msg);
                }
                try
                {
                    output.Finish();
                    if (Verbose)
                        WriteProgressCmd(notesFinished, totNotes, "DONE!\n");
                }
                catch (Exception e)
                {
                    string eText = String.Format("An error occured trying to finish, save and close " +
                                                 "the file '{0}'.", output.OutputFile.Name);
                    if (Verbose)
                    {
                        eText += String.Format("\nOutput file path: '{0}'", output.OutputFile.FullName);
                        eText += "\n\nError message: " + e.Message;
                    }
                    throw new Exception(eText);
                }
                if (Verbose)
                {
                    msg = String.Format("Finished converting notebook '{0}'.\n\n", notebook.Name);
                    WriteProgressCmd(notesFinished, totNotes, msg);
                }
            }

            //--- Write completion message
            //Set cursor two lines below progress bar
            Console.SetCursorPosition(0, Console.WindowTop + Console.WindowHeight + 2);
            Console.WriteLine("FINISHED!");
            Console.WriteLine("{0} notes was successfully converted.", totNotes);
            Console.WriteLine("You'll find the {0} Evernote Note Export files (*.enex) " +
                              "in the following folder:\n   '{1}'", input.NotebookCount, OutputFolder);
        }

        static void WriteProgressCmd(int currentNote, int totNotes, string message = "")
        {
            int curLeft = Console.CursorLeft;
            int curTop = Console.CursorTop;
            int winHeight = Console.WindowHeight;
            int winBottom = Console.WindowTop + winHeight-1;

            //--- Clear progress bar
            Console.SetCursorPosition(0, winBottom);
            Console.Write(new String(' ', Console.WindowWidth));

            //--- Write message
            Console.SetCursorPosition(curLeft, curTop);
            Console.Write(message);
            curLeft = Console.CursorLeft;
            curTop = Console.CursorTop;


            //--- Set cursor to start of progress bar
            int curProgBarTop = curTop + 1;
            if (curLeft != 0) //Add blank line between message stack and progress bar
                curProgBarTop++;
            if (curProgBarTop < winBottom) //Keeps progress bar at the bottom of the window.
                curProgBarTop = winBottom;
            Console.SetCursorPosition(0, curProgBarTop);

            //--- Write progress bar
            Console.Write("Converting note {0}/{1} [", currentNote, totNotes); //Write start of progress line
            //Define bounds of progress bar
            int progBarLeft = Console.CursorLeft;
            int progress = (int)(((double)currentNote / totNotes) * 100); //Progress in %
            string progBarTail = String.Format("] {0,3:##0}%", progress); 
            int progBarRight = Console.WindowWidth - progBarTail.Length;
            int progBarWidth = progBarRight - progBarLeft;
            //Calculate progress bar width corresponding to completed %
            int currentProgBarWidth = (int)(((double)currentNote / totNotes) * progBarWidth);
            //Write progress bar
            Console.Write(new String('=', currentProgBarWidth));
            Console.SetCursorPosition(progBarRight, curProgBarTop);
            Console.Write(progBarTail);


            //--- Return cursor to position for next message
            Console.SetCursorPosition(curLeft, curTop);
        }
		

    }
}
