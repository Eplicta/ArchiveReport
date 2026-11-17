# Eplicta.ArchiveReport

Generates Riksarkivet's metadata appendix ("Metadatabilaga") — an Excel workbook describing a web archiving delivery, with one row per archived file.

The repository contains two projects:

- **`Eplicta.Conserver.ArchiveReport`** — the reusable NuGet package (`Eplicta.ArchiveReport`). It exposes the data model and the Excel renderer; it does **not** touch the file system or read archives. Consumers build the data model and call the renderer.
- **`Eplicta.Conserver.Cli`** — a thin command-line harness that builds the data model from a finished delivery zip (reads `sip.xml`, scans files) and renders the report. Useful for manual testing.

---

## Package API

```csharp
var report = new ArchiveReport
{
    Description = "My delivery",
    ArchiveBuilder = "Example AB",
    // ... remaining delivery-level metadata (see ArchiveReport) ...
    Files =
    [
        new FileEntry { FileName = "index.html", FileType = "text/html", SizeBytes = 1234, Charset = "utf-8", Path = "https://example.org/" },
        // ... one FileEntry per page/resource ...
    ]
};

byte[] xlsx = new Renderer().GetBytes(report);
// or: new Renderer().Save(report, "metadatabilaga.xlsx");
```

- `ArchiveReport` — delivery-level metadata + the file list (the "Allmänt" sheet).
- `FileEntry` — the 10 Riksarkivet columns per file (the "Filer" sheet).
- `Renderer` — fills the embedded `mall_webb_2026.xlsx` template and returns the workbook as bytes.

Excel handling uses **ClosedXML** (MIT). No external license configuration is required.

---

## CLI usage

```bash
dotnet run --project Eplicta.Conserver.Cli -- <path-to-delivery.zip> [options]
```

The report is written next to the input as `<name>.xlsx`. Metadata not present in the archive can be supplied with `--description=`, `--creator=`, `--systemname=`, `--system=`, `--withdrawaldate=`, `--comment=`.

The CLI expects a `sip.xml` inside the archive and a nested content zip. Media duration is read with TagLib#.

---

## Releasing

Every merge to `master` builds, tests and publishes `Eplicta.ArchiveReport` to nuget.org, and creates a GitHub release `v<version>`. The version is `<major>.<minor>` from `version.txt` followed by the workflow run number, so each merge gets a new patch version. Change `version.txt` to start a new minor or major line. A specific version can be published from the Actions tab with **Publish → Run workflow**.

## License

MIT, see [LICENSE](LICENSE).

---

## Author

Eplicta ArchiveReport Team
