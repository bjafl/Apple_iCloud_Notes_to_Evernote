# ConvertICloudNotes
This is an easy tool to convert notes from an iCloud backup (generated at privacy.icloud.com) to the Evernote Note Export format (*.enex).
Due in part to the limited data provided by the iCloud backup some data like tables made in the Apple Notes app will not be possible to convert. Tables are converted to a single symbol that contains no information regarding its contents.

What will be converted
I've tried this tool with backups containing plain text, images and embedded pdf-files, so it should work with notes containing those data types. Locked notes are not provided in the iCloud backup, so if you want to include notes that are locked you need to unlock them before you request your backup from Apple.
￼￼If an exported note contains the object replacement character (see: https://en.wiktionary.org/wiki/%EF%BF%BC), it may be due to data that is missing in the iCloud backup compared to the original note. 
There may be information in the iCloud backup that this tool does not convert. The tool was made for a specific scenario and has been made to just include the features that was needed at the time it was written. Feel free to suggest changes or edit the sourcecode to fit your needs. 

How to use
TODO: Write instructions
