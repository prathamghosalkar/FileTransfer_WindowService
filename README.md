# File Transfer Windows Service

## Project Overview

The File Transfer Windows Service is a background service that monitors a specified directory for text files, processes them, and records relevant information into a SQL Server database. Processed files are then moved to a designated "processed" folder.

## Features

- **File Monitoring**: Continuously monitors a specified source directory for new text files.
- **File Processing**: Extracts information from file names to generate a unique `CallId`.
- **Database Interaction**: Saves unique records to a SQL Server database and avoids duplicate entries.
- **File Management**: Moves processed files to a separate folder to keep the source directory clean.

## Prerequisites

- .NET Framework 4.7.2 or later
- SQL Server (or SQL Server Express)
- Windows Service environment

## Installation and Setup

### 1. Build the Project

Compile the project to generate the Windows Service executable.

### 2. Create and Install the Service

1. Open Command Prompt as Administrator.
2. Navigate to the directory containing the executable.
3. Use the `sc` command to create and start the service:

    ```bash
    sc create FileTransferService binPath= "C:\path\to\your\executable.exe"
    sc start FileTransferService
    ```

### 3. Configure the Service

- **Source and Processed Folder Paths**: Set the paths for the source and processed folders in the `Service1` class:

    ```csharp
    private readonly string sourceFolder = @"D:\Textfile"; // Path to monitor
    private readonly string processedFolder = @"D:\ProcessedFile"; // Folder to move processed files
    ```

- **Connection String**: Update the `connectionString` in the `Service1` class to match your SQL Server configuration:

    ```csharp
    private readonly string connectionString = @"server=YOUR_SERVER_NAME;Initial Catalog=YOUR_DATABASE_NAME;User Id=YOUR_USER_ID;Password=YOUR_PASSWORD;TrustServerCertificate=True";
    ```

### 4. Database Schema

Create the `ProcessedFiles_New` table in your SQL Server database with the following schema:

```sql
CREATE TABLE ProcessedFiles_New (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CallId NVARCHAR(50) UNIQUE,
    Channel INT,
    [Date] DATE,
    [Time] TIME
);
