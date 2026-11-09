using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace HappyMahjong;

public static class PatchCombiner
{
	public static readonly ulong MAGIC = 1039651151401573800uL;

	public const ushort VERSION = 1;

	public static void Create(string[] patchFiles, string outputPath)
	{
		List<(byte[], uint)> list = new List<(byte[], uint)>();
		List<(uint, uint)> list2 = new List<(uint, uint)>();
		for (int i = 0; i < patchFiles.Length; i++)
		{
			byte[] array = File.ReadAllBytes(patchFiles[i]);
			list.Add((array, (uint)array.Length));
		}
		uint num = (uint)(12 + 8 * list.Count);
		foreach (var item in list)
		{
			list2.Add((num, item.Item2));
			num += item.Item2;
		}
		using FileStream fileStream = new FileStream(outputPath, FileMode.Create);
		long num2 = 0L;
		using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
		{
			binaryWriter.Write(MAGIC);
			binaryWriter.Write((ushort)1);
			binaryWriter.Write((ushort)list.Count);
			foreach (var item2 in list2)
			{
				binaryWriter.Write(item2.Item1);
				binaryWriter.Write(item2.Item2);
			}
			foreach (var item3 in list)
			{
				binaryWriter.Write(item3.Item1);
			}
			num2 = fileStream.Length;
		}
		string arg = string.Join(",", patchFiles.Select((string item) => Path.GetFileName(item)).ToList());
		Debug.Log($"create combined patch of patch list:[{arg}]  at: {outputPath} success! len:{num2}");
	}
}
