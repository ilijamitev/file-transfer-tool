namespace FileTransferTool.Core.Models;

public readonly record struct Chunk(int Index, long Offset, int Length);