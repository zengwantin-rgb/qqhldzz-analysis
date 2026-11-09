using System;
using System.Collections.Generic;
using System.IO;

namespace HappyMahjong;

public static class CombinedPatchProcessor
{
	public static void ProcessCombinedPatchFile(string filePath, Action<Stream> processSubPatch)
	{
		using FileStream stream = File.OpenRead(filePath);
		ProcessCombinedPatchStream(stream, processSubPatch);
	}

	public static void ProcessCombinedPatchStream(Stream stream, Action<Stream> processSubPatch)
	{
		using BinaryReader binaryReader = new BinaryReader(stream);
		if (binaryReader.ReadUInt64() != PatchCombiner.MAGIC)
		{
			throw new InvalidDataException("Not a valid combined patch file (invalid magic)");
		}
		if (binaryReader.ReadUInt16() != 1)
		{
			throw new InvalidDataException("Not a valid combined patch file (invalid vesion)");
		}
		ushort num = binaryReader.ReadUInt16();
		List<PatchHeaderEntry> list = new List<PatchHeaderEntry>();
		for (int i = 0; i < num; i++)
		{
			list.Add(new PatchHeaderEntry
			{
				Offset = binaryReader.ReadInt32(),
				Length = binaryReader.ReadInt32()
			});
		}
		foreach (PatchHeaderEntry item in list)
		{
			using SubStream obj = new SubStream(stream, item.Offset, item.Length);
			processSubPatch?.Invoke(obj);
		}
	}
}
