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

```
## Service Functionality

- **Polling Interval**: 
  - The service checks the source directory every 30 seconds.

- **File Processing**:
  - **Extract Information**: 
    - The expected file name format is `P{Channel} {Date} {Time}` (e.g., `P0 01-10-2011 142536`).
  - **Generate CallId**: 
    - Constructed as `Channel-YYYYMMDDHHMMSS` (e.g., `0-20110101142536`).
  - **Check for Duplicates**: 
    - The service verifies if the `CallId` exists in the database before inserting a new record.
  - **Insert Data**: 
    - Inserts records into the `ProcessedFiles_New` table if the `CallId` is unique.

- **File Management**: 
  - Moves files from the source directory to the processed directory after successful processing.
 

## Error Handling

- **Format Errors**: 
  - Logs issues related to invalid file formats.

- **Database Errors**: 
  - Captures errors encountered during database operations.

- **Logging Errors**: 
  - Handles errors while writing logs to files.

## Logging

- **Logs**:
  - Logs are stored in the `Logs` folder within the application directory.
  - They include:
    - Service start and stop times
    - Processing details and any errors encountered
    - 
## Contact

- **For support or inquiries, please reach out to Pratham Ghosalkar.**

### Instructions for Use

1. Replace placeholders (e.g., `YOUR_SERVER_NAME`, `YOUR_DATABASE_NAME`, `YOUR_USER_ID`, `YOUR_PASSWORD`, and contact information) with actual values.
2. Save this content in a file named `README.md` in the root of your GitHub repository.
3. Adjust any paths or specific instructions based on your project's needs.

This README provides a comprehensive guide to understanding, installing, configuring, and running the File Transfer Windows Service.
