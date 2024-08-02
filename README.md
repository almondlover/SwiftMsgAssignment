## **Summary**

A standalone .NET 8 Web API that can process a SWIFT MT799 message and uploads the fields in its text block to an SQLite Database.
Data is passed through a single controller with a single endpoint for transferring the file containing the message. Said file can be uploaded through Swagger. The POST method parses the message and uploads its relevant fields to the database.

Additional info is logged to the console using the Serilog library.

Message Parsing and data transfer is handled through a MessageService which is called by the MessageController.

The API initializes an object containing data ragarding all the message's structure blocks but only uploads relevant fields specified in its text block to the database.

