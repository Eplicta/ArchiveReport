# Eplicta Conserver CLI

A command-line tool for generating structured Excel reports from archive delivery packages (ZIP files containing METS/SIP metadata).

---

## Overview

**Eplicta.Conserver.Cli** processes archive delivery ZIP files and produces an Excel report using a predefined template.

The tool:

1. Reads metadata from `sip.xml` inside the archive
2. Extracts and scans files (including nested ZIPs)
3. Collects file metadata (name, size, type, duration, etc.)
4. Outputs a formatted Excel report

---

## Project Structure

```
Eplicta.Conserver.Cli
│
├── Eplicta.Conserver.ArchiveReport
│   ├── ArchiveReport.cs
│   ├── FileEntry.cs
│   ├── FileScanner.cs
│   ├── MetsLoader.cs
│   ├── Renderer.cs
│   └── Templates/
│       └── mall_webb_2026.xlsx
│
└── Eplicta.Conserver.Cli
    └── Program.cs
```

---

## How It Works

### 1. Metadata Extraction

The system reads `sip.xml` inside the ZIP archive and extracts metadata such as:

* Description
* Archive creator
* System name
* Withdrawal date

---

### 2. File Scanning

The tool:

* Extracts nested ZIP files
* Scans all files recursively
* Collects metadata like file size and duration

---

### 3. Data Model

The report is built using structured models:

* `ArchiveReport` → overall archive metadata 
* `FileEntry` → per-file metadata 

---

### 4. Excel Generation

An embedded Excel template is populated using EPPlus:

* Sheet **"Allmänt"** → archive metadata
* Sheet **"Filer"** → file list

---

## Usage

### Basic

```bash
dotnet run -- <path-to-zip>
```

Example:

```bash
dotnet run -- archive.zip
```

---

### With Optional Parameters

You can override metadata from the command line:

```bash
dotnet run -- archive.zip \
  --description="My archive" \
  --creator="Company AB" \
  --systemname="System X" \
  --system="Internal System" \
  --withdrawaldate="2025-01-01" \
  --comment="Generated manually"
```

---

## Output

The tool generates an Excel file next to the input ZIP:

```
archive.zip → archive.xlsx
```

---

## Requirements

* .NET (compatible with your project target framework)
* EPPlus (for Excel handling)
* TagLib# (for media duration extraction)

---

## Notes

* The tool expects a `sip.xml` file inside the archive
* Nested ZIP files are automatically extracted
* Unsupported media files are ignored when reading duration
* EPPlus is configured for **non-commercial personal use**
* Follows riksarkivets E-ARK requirements

---


## Author

Eplicta ArchiveReport Team

---
