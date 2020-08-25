using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Security.Cryptography;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Globalization;

namespace ConvertICloudNotes
{
    public class OutputWriter
	{
		private XmlWriterSettings xmlSettings;
		private XmlWriter writer;

		public OutputWriter(string OutputFolderPath, string NotebookName)
        {
			xmlSettings = new XmlWriterSettings();
			xmlSettings.Indent = true;
			string outputFilePath = OutputFolderPath + "\\" + NotebookName + ".enex";
			writer = XmlWriter.Create(outputFilePath, xmlSettings);
			WriteXmlStart();
		}


		public void Finish()
        {
			WriteXmlEnd();
        }

		public void WriteNote(Note note)
        {
			writer.WriteStartElement("note");
			writer.WriteElementString("title", note.Title);
			writer.WriteStartElement("content");
			writer.WriteRaw(@"<![CDATA[");
			
			StringBuilder content = new StringBuilder();
			XmlWriter contentWriter = XmlWriter.Create(content, xmlSettings);
			contentWriter.WriteDocType("en-note", null, "http://xml.evernote.com/pub/enml2.dtd", null);
			contentWriter.WriteStartElement("en-note");
			FileInfo[] attachments = note.GetAttachments();
			int iAtt = 0;
			StringReader noteReader = new StringReader(note.GetText());
			string line = "";
			while ((line = noteReader.ReadLine()) != null)
            {
				if(line.Contains(Note.ATT_MARKER))
                {
					FileInfo att = attachments[iAtt];
					string hash = CreateMD5Hash(File.ReadAllBytes(att.FullName));
					contentWriter.WriteStartElement("div");
					contentWriter.WriteStartElement("en-media");
					contentWriter.WriteAttributeString("hash", hash);
					contentWriter.WriteAttributeString("type", EnMediaFormat(att.Extension));
					contentWriter.WriteEndElement();
					contentWriter.WriteEndElement();
					iAtt++;

					line = line.Replace(Note.ATT_MARKER, "");
                }
				if (line == "") //TODO: check....
				{
					contentWriter.WriteStartElement("div");
					contentWriter.WriteRaw("<br/>");
					contentWriter.WriteEndElement();
				}
				else if (line != note.Title)
					contentWriter.WriteElementString("div", line);
            }
			contentWriter.WriteEndDocument();
			contentWriter.Close();

			writer.WriteRaw(content.ToString());
			writer.WriteRaw("]]>");
			writer.WriteEndElement();
			string noteDateTime = FormatDateTimeEvernote(note.GetDateTime());
			writer.WriteElementString("created", noteDateTime); 
			if (note.HasAttachments)
			{
				foreach (FileInfo att in note.GetAttachments())
				{
					string attData = FileToBase64(att.FullName);
					writer.WriteStartElement("resource");
					writer.WriteStartElement("data");
					writer.WriteAttributeString("encoding", "base64");
					writer.WriteString(attData);
					writer.WriteEndElement();
					writer.WriteElementString("mime", EnMediaFormat(att.Extension));
					int[] imgWidthHeight = FindImageWidthHeight(att);
					if (imgWidthHeight != null)
					{
						writer.WriteElementString("width", imgWidthHeight[0].ToString());
						writer.WriteElementString("height", imgWidthHeight[1].ToString()); 
					}
					writer.WriteEndElement();
				}
			}
			writer.WriteEndElement();
        }

		private void WriteXmlStart()
		{
			writer.WriteStartDocument();
			writer.WriteDocType("en-export", null, "http://xml.evernote.com/pub/evernote-export2.dtd", null);
			writer.WriteStartElement("en-export");
			string currentDateTime = FormatDateTimeEvernote(DateTime.Now.ToUniversalTime());
			writer.WriteAttributeString("export-date", currentDateTime); 
			writer.WriteAttributeString("application", "Evernote/Windows");
			writer.WriteAttributeString("version", "6.x");
		}

		private void WriteXmlEnd()
        {
			writer.WriteEndElement();
			writer.WriteEndDocument();
			writer.Close();
		}

		static private string CreateMD5Hash(byte[] input)
		{
			MD5 md5 = System.Security.Cryptography.MD5.Create();
			byte[] hashBytes = md5.ComputeHash(input);

			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < hashBytes.Length; i++)
			{
				sb.Append(hashBytes[i].ToString("X2"));
			}
			return sb.ToString();
		}

		static private string FileToBase64(string path)
        {
			byte[] byteArr = File.ReadAllBytes(path);
			return Convert.ToBase64String(byteArr);
        }

		static private string FormatDateTimeEvernote(DateTime dateTime)
        {
			string dateTimeEvernote = "";
			dateTimeEvernote += dateTime.Year;
			dateTimeEvernote += dateTime.Month.ToString("D2");
			dateTimeEvernote += dateTime.Day.ToString("D2");
			dateTimeEvernote += "T";
			dateTimeEvernote += dateTime.Hour.ToString("D2");
			dateTimeEvernote += dateTime.Minute.ToString("D2");
			dateTimeEvernote += dateTime.Second.ToString("D2");
			dateTimeEvernote += "Z";
			return dateTimeEvernote;
		}

		static private string EnMediaFormat(string FileExtension)
        {
			FileExtension = FileExtension.Replace(".", "");
			string[] imageFormats = { "jpg", "jpeg","gif", "png" };
			string[] audioFormats = {"wav","mpeg" };
			string[] appFormats = { "pdf" }; //TODO: application/vnd.evernote.ink
			if (imageFormats.Contains(FileExtension))
            {
				if (FileExtension == "jpg")
					return "image/jpeg";
				return "image/" + FileExtension;
			}
			else if (audioFormats.Contains(FileExtension))
			{
				return "audio/" + FileExtension;
			}
			else if (appFormats.Contains(FileExtension))
			{
				return "application/" + FileExtension;
			}
			else
            {
				throw new Exception("Media format not supported by Evernote.");
            }
		}

		static private int[] FindImageWidthHeight(FileInfo Image)
        // Finds the pixel width and height of an image file
		// and returns it as an int array: {width, height}
		// Returns null if file is not an image file. 
		{
			try
			{
				Bitmap img = new Bitmap(Image.FullName);
				return new int[] { img.Width, img.Height };
			}
			catch //File is not image file. Retrun null.
            {
				return null;
            }
		}
	}
}
