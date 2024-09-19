# File Transfer Windows Service

This project is a Windows Service that monitors a specified directory for text files, processes them, and records relevant information into a SQL Server database. Processed files are then moved to a designated "processed" folder.

## Features

- **File Monitoring**: Continuously monitors a specified source directory for new text files.
- **File Processing**: Extracts information from file names to generate a unique `CallId`.
- **Database Interaction**: Saves unique records to a SQL Server database and avoids duplicate entries.
- **File Management**: Moves processed files to a separate folder to keep the source directory clean.

## Prerequisites

- .NET Framework 4.7.2 or later
- SQL Server (or SQL Server Express)
- Windows Service environment

## Configuration

### Connection String

Update the `connectionString` in the `Service1` class to match your SQL Server configuration:

```csharp
private readonly string connectionString = @"server=YOUR_SERVER_NAME;Initial Catalog=YOUR_DATABASE_NAME;User Id=YOUR_USER_ID;Password=YOUR_PASSWORD;TrustServerCertificate=True";
