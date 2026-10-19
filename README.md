# File Transfer Tool

A .NET console app that copies a large file in chunks and checks the copy: every chunk with MD5, and the whole file with SHA-256.

## Features

- Copies files of any size (tested up to 50 GiB) in 1 MiB chunks, several at a time.
- Checks every chunk with MD5 by reading it back after it's written, and writes it again on a mismatch.
- Compares the SHA-256 of the source and the copy, and shows both.
- Lists each chunk's position and MD5.
- Shows progress while copying.
- Gives the copy its final name only after it's verified.
- Uses no third-party packages.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

## Getting started

```shell
git clone https://github.com/ilijamitev/file-transfer-tool.git
cd file-transfer-tool
dotnet run --project FileTransferTool/FileTransferTool -c Release
```

Or open `FileTransferTool/FileTransferTool.slnx` in Visual Studio and run the `FileTransferTool` project.

## Usage

The tool asks for two paths:

1. **Source file path**: the file to copy.
2. **Destination path**: the folder to copy it into. The file keeps its name, and the folder is created if it doesn't exist.

If the file is already there, it asks before overwriting it. At the end, it waits for Enter before closing.

### Example

Copying a 1 GiB file:

```text
Welcome to File Transfer Tool
It copies a file in chunks, checking every chunk with MD5 and the whole file with SHA-256.

Enter source file path (e.g. c:\source\my_large_file.bin): "C:\tool-test\src\random-1GiB.bin"
Enter destination path (e.g. d:\destination\): D:\tool-test\dest

Source:      C:\tool-test\src\random-1GiB.bin (1.00 GiB)
Destination: D:\tool-test\dest\random-1GiB.bin
Plan:        1,024 chunks of 1.00 MiB, 8 in parallel, up to 3 attempts each
[####################] 100%  1.00 GiB / 1.00 GiB  270.96 MiB/s  done

Chunk checksums (MD5):
1) position = 0, hash = 26b7bab93af6862cb2a213ec1caf728c
2) position = 1048576, hash = e22eba9272020c6bdb28a67b59442a9b
3) position = 2097152, hash = db9600968ab073fa74ad4ae71f9e9141
...
1024) position = 1072693248, hash = a21cfeca389f8b125a3d580ce8ceb154

Source SHA-256:      cacdad75f4ca5426ecf8f55bc082bf6b76f0779ed075c16f071e150b2f077186
Destination SHA-256: cacdad75f4ca5426ecf8f55bc082bf6b76f0779ed075c16f071e150b2f077186
Result: MATCH

Copied and verified 1.00 GiB in 3.81 s (268.94 MiB/s): 1,024 chunks.
```

## How it works

1. **Plan.** Both paths are checked. The source has to be an existing file, not a link, and not the destination itself.
2. **Prepare.** The source is opened so that nothing else can write to it. The copy goes to a temp file, `<name>.<random>.part`, in the destination folder, with the full size reserved up front.
3. **Copy.** Up to 8 chunks are copied at once, each at its own offset. Every chunk is hashed with MD5, written, read back and hashed again. If the hashes differ, the chunk is written again; after 3 failed attempts the copy stops.
4. **Verify.** The SHA-256 of the source and of the temp file are compared.
5. **Commit.** The temp file gets its final name. An existing file is replaced only now, and only if you answered `y`.

If anything fails, the error is shown in red, the temp file is deleted and an existing file is left as it was. Only a failed rename in step 5 keeps the verified copy, as the `.part` file.

## Settings

The defaults are in [`TransferOptions`](FileTransferTool/FileTransferTool.Core/Models/TransferOptions.cs). There are no command-line options yet.

| Setting | Default |
|---|---|
| Chunk size | 1 MiB |
| Chunks in parallel | min(processor count, 8) |
| Attempts per chunk | 3 |

## Project structure

```text
FileTransferTool/
├── FileTransferTool.slnx
├── Directory.Build.props      Build settings for both projects
├── FileTransferTool/          Console app: prompts, progress, report and errors
│   ├── Program.cs             Entry point; shows any error
│   ├── TransferWorkflow.cs    One run: ask, plan, copy, report
│   ├── Input/                 Reads the paths and the overwrite answer
│   └── Output/                Writes the plan, progress, report and errors
└── FileTransferTool.Core/     Class library with the copy logic; no console code
    ├── Planning/              Checks the paths and builds the plan
    ├── Transfer/              Copies the whole file, and each chunk
    ├── Storage/               The source file, and the temp file with its final rename
    ├── Hashing/               MD5 for chunks, SHA-256 for whole files
    ├── Models/                Plan, options, chunk, progress and result
    └── Exceptions/            Setup, verification and commit failures
```

The build is strict: warnings are errors, and the .NET analyzers and code-style rules run on every build.

## Known limitations

- **No resume support:** If a copy is interrupted (Ctrl+C, closing the window, a power cut), the `.part` file stays behind and has to be deleted by hand, and the next run starts over.
- **Console report size:** Every chunk gets its own line, 1,024 per GiB, so for a big file the top of the report scrolls out of the console.
- **Path handling:** A source that's a link is refused, and the same-file check compares paths as text, so it misses a destination reached through a link.
- **Platform and testing:** Tested manually on Windows 11 only, with files from 0 B to 50 GiB. There are no automated tests yet.