<p align="center">
  <img src="https://dot-sys.github.io/VortexCSRSSTool/Assets/VortexLogo.svg" alt="Vortex Logo" width="100" height="100">
</p>

<h2 align="center">Vortex PCA</h2>

<p align="center">
  Small forensic analysis Tool of Windows Local PCA (Program Compatibility Assistant) Logs. Handles PcaAppLaunchDic & PcaGeneralDb with metadata enrichment!<br><br>
  ⭐ Star this project if you found it useful.
</p>

---
<p align="center">
  <img src="https://i.imgur.com/5pZpdcI.png" alt="Vortex PCA UI Picture" height="600">
</p>

### Overview

**Vortex PCA** is a robust .NET Tool for forensic analysis of local Program Compatibility Assistant (PCA) artifacts on Windows 11 22H2+. It parses the text-based PCA Logs (`PcaAppLaunchDic.txt`, `PcaGeneralDb0.txt`, `PcaGeneralDb1.txt`) to reconstruct application execution history and compatibility data. It further enriches this data by resolving file paths, validating signatures and cross-referencing with the NTFS USN Journal.

#### Core Parsing

- Parse **PcaAppLaunchDic.txt** (LAD) for application launch history.
- Parse **PcaGeneralDbx.txt** (GDB) for compatibility insights.
- **Windows 11 22H2+ Support**: Specifically handles the text-based artifact format found in newer Windows 11 builds (22621+).

#### Metadata Enrichment

- **USN Journal Tracking**: Checks NTFS USN Journal for Deletion/Rename history of referenced files.
- **Exe Metadata**: Extracts Creation/Ref/Write timestamps, MD5 hashes and Authenticode signature status.
- **PE Analysis**: Validates PE headers, Entry Points and Debug Directory presence.

---

### Features

- **Compatibility Analysis**: Detailed parsing of GDB entries including potential exit codes and program IDs.
- **Forensic Timeline**: Reconstructs execution timelines with UTC and Local timestamp alignment.
- **Live Analysis**: Targets the live system PCA directory (`%SystemRoot%\appcompat\pca`).
- **Deep File Inspection**: Analyzes file presence, headers and certificates for all PCA entries.

### Requirements

- .NET Framework 4.6.2
- Windows 11 (Version 22H2 / Build 22621 or newer)
- Administrator privileges (for access to `C:\Windows\appcompat\pca` and USN Journal)
