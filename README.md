# ConvertICloudNotes
This is an easy tool to convert notes from an iCloud backup (generated at <http://privacy.icloud.com>) to the Evernote Note Export format (*.enex).

Due in part to the limited data provided by the iCloud backup some data like tables made in the Apple Notes app will not be possible to convert. For example tables are converted to a single symbol that contains no information regarding its contents.

## What will be converted
I've tried this tool with backups containing **plain text, images and embedded pdf files**, so it should work with notes containing those data types. It may also work for embedded audio files.

Locked notes are not provided in the iCloud backup, so if you want to include notes that are locked you need to unlock them before you request your backup from Apple.

If an exported note contains the object replacement character, ￼, it may be due to data that is missing in the iCloud backup compared to the original note. If that is the case, you will need to copy the contents of these notes manually. 

There may be information in the iCloud backup that this tool does not convert. The tool was made for a specific scenario so the features are limited to what was needed at the time it was written. Feel free to suggest changes or edit the sourcecode to fit your needs. 

## Usage
> **TODO:** Write instructions

## Contributing
Please feel free 

## Resources 
Check out [this page](https://dev.evernote.com/doc/articles/enml.php) at Evernote's dev-pages for information about the *.enex format.

## License
[MIT](https://choosealicense.com/licenses/mit/)