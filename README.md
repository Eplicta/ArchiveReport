# Eplicta.Conserver.ArchiveReport

A .NET library for generating Excel reports from Conserver archive deliveries.

The package reads a SIP archive (ZIP), extracts metadata from the `sip.xml` METS file, scans the contained files, and produces a structured Excel report based on a predefined template.

## Features

* Read **METS metadata** from `sip.xml`
* Extract and scan **nested ZIP archives**
* Collect file metadata such as:

  * File name
  * File type
  * File size
  * Charset
  * Media duration (if available)
  * File path
* Generate a **formatted Excel report**
* Uses an embedded Excel template

## Installation

Install via NuGet:

```bash
dotnet add package Eplicta.Conserver.ArchiveReport
```

or via Package Manager:

```powershell
Install-Package Eplicta.Conserver.ArchiveReport
```

## Basic Usage

The simplest way to generate a report is to call:

```csharp
using Eplicta.Conserver.ArchiveReport;

Renderer.Save("archive.zip");
```

This will:

1. Read the archive
2. Extract metadata from `sip.xml`
3. Scan all files in the archive
4. Generate an Excel report
5. Save it next to the ZIP file

Example output:

```
archive.zip
archive.xlsx
```

## Generate Report as Bytes

If you want to handle the Excel file yourself (for example in a web API), you can retrieve the report as a byte array.

```csharp
using Eplicta.Conserver.ArchiveReport;

var bytes = Renderer.GetBytes("archive.zip");

File.WriteAllBytes("report.xlsx", bytes);
```

## How It Works

### 1. Read Archive Metadata

The package reads `sip.xml` inside the ZIP archive and extracts metadata such as:

* Archive creator
* System name
* Description
* Extraction date
* Comments

### 2. Scan Files

The archive is extracted and scanned to collect file metadata including:

* File name
* Extension
* File size
* Duration for media files (if available)

### 3. Generate Excel Report

An Excel report is generated using a built-in template.

The report contains two sheets:

**Allmänt**

* Archive metadata
* Contact information
* System information

**Filer**

* File name
* File type
* Size
* Duration
* Path
* Security classification
* Comments

## Example Workflow

```csharp
using Eplicta.Conserver.ArchiveReport;

string zipPath = @"C:\archives\delivery.zip";

// Generate report
Renderer.Save(zipPath);
```

Result:

```
delivery.exe
```

## Requirements

* .NET 6 or later
* EPPlus
* TagLib#

## Error Handling

If file scanning fails, the error message will be appended to the report comments so that the process can continue.
