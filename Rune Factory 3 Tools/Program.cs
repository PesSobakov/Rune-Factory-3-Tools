using System.IO;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System.Xml.Linq;
using System.Net.Http.Headers;
using System.Drawing;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Rune_Factory_3_Tools
{
    public static class Myextension
    {

        public static int IndexOf(this byte[] input, string text)
        {
            for (int i = 0; i < input.Length - (text.Length - 1); i++)
            {
                int j = 0;
                for (; j < text.Length;)
                {
                    if (input[i + j] == (byte)text[j])
                    {
                        j++;
                    }
                    else { break; }
                }
                if (j == text.Length)
                {
                    return i;
                }
            }
            return -1;
        }

        public static string ReadString(this byte[] input, long offset = 0)
        {
            StringBuilder output = new();
            long current = offset;
            while (current < input.Length && input[current] != 0)
            {
                output.Append((char)input[current++]);
            }
            return output.ToString();
        }


        public static byte[] SubArray(this byte[] input, long offset, long length = -1)
        {
            if (length == -1 || length > input.Length - offset)
            {
                length = input.Length - offset;
            }

            byte[] output = new byte[length];
            for (long i = offset, j = 0; i < offset + length; i++)
            {
                output[j++] = input[i];
            }
            return output;
        }

        public static uint ReadUintLE(this byte[] input, long offset)
        {
            return (uint)(input[offset] | (input[offset + 1] << 8) | (input[offset + 2] << 16) | (input[offset + 3] << 24));
        }
        public static uint ReadUintLE(this List<byte> input, long offset)
        {
            return (uint)(input[(int)offset] | (input[(int)offset + 1] << 8) | (input[(int)offset + 2] << 16) | (input[(int)offset + 3] << 24));
        }
        public static byte[] WriteUintLE(this byte[] input, long offset, uint number)
        {
            input[offset] = (byte)(number & 0xff);
            input[offset + 1] = (byte)((number & 0xff00) >> 8);
            input[offset + 2] = (byte)((number & 0xff0000) >> 16);
            input[offset + 3] = (byte)((number & 0xff000000) >> 24);
            return input;
        }
        public static List<byte> WriteUintLE(this List<byte> input, long offset, uint number)
        {
            input[(int)offset] = (byte)(number & 0xff);
            input[(int)offset + 1] = (byte)((number & 0xff00) >> 8);
            input[(int)offset + 2] = (byte)((number & 0xff0000) >> 16);
            input[(int)offset + 3] = (byte)((number & 0xff000000) >> 24);
            return input;
        }
        public static List<byte> WriteUintLE(this List<byte> input, long offset, int number)
        {
            uint number2 = (uint)number;
            input[(int)offset] = (byte)(number2 & 0xff);
            input[(int)offset + 1] = (byte)((number2 & 0xff00) >> 8);
            input[(int)offset + 2] = (byte)((number2 & 0xff0000) >> 16);
            input[(int)offset + 3] = (byte)((number2 & 0xff000000) >> 24);
            return input;
        }
        public static byte[] ToBytearrLE(this uint input)
        {
            return new byte[] { (byte)(input & 0xff), (byte)((input & 0xff00) >> 8), (byte)((input & 0xff0000) >> 16), (byte)((input & 0xff000000) >> 24) };
        }
        public static byte[] ToBytearr(this string input)
        {
            byte[] output = new byte[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                output[i] = (byte)input[i];
            }
            return output;
        }

        public static ushort ReadShortLE(this byte[] input, long offset)
        {
            return (ushort)(input[offset] | (input[offset + 1] << 8));
        }
        public static ushort ReadShortLE(this List<byte> input, long offset)
        {
            return (ushort)(input[(int)offset] | (input[(int)offset + 1] << 8));
        }

        public static byte[] WriteShortLE(this byte[] input, long offset, ushort number)
        {
            input[offset] = (byte)(number & 0xff);
            input[offset + 1] = (byte)((number & 0xff00) >> 8);

            return input;
        }
        public static List<byte> WriteShortLE(this List<byte> input, long offset, ushort number)
        {
            input[(int)offset] = (byte)(number & 0xff);
            input[(int)offset + 1] = (byte)((number & 0xff00) >> 8);

            return input;
        }
        public static byte[] ToBytearrLE(this ushort input)
        {
            return new byte[] { (byte)(input & 0xff), (byte)((input & 0xff00) >> 8) };
        }
        public static byte[] ToUintLE(this int input)
        {
            uint number = (uint)input;
            return [(byte)(number & 0xff), (byte)((number & 0xff00) >> 8), (byte)((number & 0xff0000) >> 16), (byte)((number & 0xff0000) >> 24)];
        }
    }

    internal class Program
    {
        public static void UnpackTextPseudoarchive(string inputFile, string outputFolder)
        {
            byte[] file = File.ReadAllBytes(inputFile);

            uint count = file.ReadUintLE(0x20);
            uint headerOffset = 0x24;
            uint dataOffset = headerOffset + count * 8;

            List<(uint offset, uint size)> files = new((int)count);

            for (int i = 0; i < count; i++)
            {
                uint offset = dataOffset + file.ReadUintLE(headerOffset + i * 8);
                uint size = file.ReadUintLE(headerOffset + i * 8 + 4);
                files.Add((offset, size));
            }

            for (int i = 0; i < files.Count; i++)
            {
                Directory.CreateDirectory(outputFolder);
                string extension = new string(file.SubArray(files[i].offset, 4).Select(x => (x >= 'A' && x <= 'Z' || x >= 'a' && x <= 'z' || x >= '0' && x <= '9') ? (char)x : '_').ToArray());
                string path = outputFolder + "\\file " + i.ToString("0000") + "." + extension;
                File.WriteAllBytes(path, file.SubArray(files[i].offset, files[i].size));
            }
        }

        public static void PackTextPseudoarchive(string inputFile, string outputFile)
        {
            byte[] file = File.ReadAllBytes(outputFile);

            uint count = file.ReadUintLE(0x20);
            uint headerOffset = 0x24;
            uint dataOffset = headerOffset + count * 8;

            List<byte[]> files = new((int)count);

            for (int i = 0; i < count; i++)
            {
                uint offset = dataOffset + file.ReadUintLE(headerOffset + i * 8);
                uint size = file.ReadUintLE(headerOffset + i * 8 + 4);
                files.Add(file.SubArray(offset, size));
            }

            int number = int.Parse(inputFile[(inputFile.LastIndexOf(" ") + 1)..inputFile.LastIndexOf(".")]);
            files[number] = File.ReadAllBytes(inputFile);

            List<byte> output = new();
            output.AddRange(file.SubArray(0, 0x24));

            for (int i = 0; i < count * 8; i++)
            {
                output.Add(0);
            }

            for (int i = 0; i < count; i++)
            {
                output.WriteUintLE(0x24 + i * 8, (uint)(output.Count - (0x24 + count * 8)));
                output.WriteUintLE(0x24 + i * 8 + 4, files[i].Length);
                output.AddRange(files[i]);
            }

            while (output.Count % 16 != 0)
            {
                output.Add(0);
            }

            output.WriteUintLE(0x14, output.Count - 0x1b);

            File.WriteAllBytes(outputFile, output.ToArray());
        }

        public static void PackTextPseudoarchiveFolder(string inputFolder, string outputFile)
        {
            foreach (var item in Directory.EnumerateFiles(inputFolder))
            {
                PackTextPseudoarchive(item, outputFile);
            }
        }

        public static void UnpackText(string inputFile, string outputFile)
        {
            byte[] file = File.ReadAllBytes(inputFile);

            uint count = file.ReadUintLE(0x4);
            uint headerOffset = 0x8;

            List<string> strings = new((int)count);

            for (int i = 0; i < count; i++)
            {
                uint offset = file.ReadUintLE(headerOffset + i * 8 + 4);
                uint size = file.ReadUintLE(headerOffset + i * 8);
                byte[] currentString = file.SubArray(offset, size);
                string decoded = Encoding.UTF8.GetString(currentString);
                strings.Add(decoded);
            }

            using (StreamWriter writer = new StreamWriter(outputFile))
            {
                for (int i = 0; i < strings.Count; i++)
                {
                    writer.WriteLine($"{i:00000}={strings[i].Replace("\n", "\\n")}");
                }
            }
        }

        public static void PackText(string inputFile, string outputFile)
        {
            string[] strings = File.ReadAllLines(inputFile);

            List<string> text = new List<string>();

            foreach (var item in strings)
            {
                if (!(item == "" || item.StartsWith("//")))
                {
                    text.Add(item[(item.IndexOf("=") + 1)..].Replace("\\n", "\n"));
                }
            }

            if (File.Exists(outputFile))
            {
                using (FileStream output = new FileStream(outputFile, FileMode.Open))
                {
                    byte[] count = new byte[4];
                    output.Seek(4, SeekOrigin.Begin);
                    output.Read(count);
                    uint stringsCount = count.ReadUintLE(0);
                    if (stringsCount != text.Count)
                    {
                        throw new Exception($"Wrong strings count. Need {stringsCount}, present {text.Count}");
                    }
                }
            }


            List<byte> file = new List<byte>();

            file.AddRange("TEXT".Select(x => (byte)x));
            file.AddRange(text.Count.ToUintLE());

            for (int i = 0; i < text.Count * 8; i++)
            {
                file.Add(0);
            }

            for (int i = 0; i < text.Count; i++)
            {
                byte[] encoded = Encoding.UTF8.GetBytes(text[i]);
                file.WriteUintLE(8 + i * 8, encoded.Length);
                file.WriteUintLE(8 + i * 8 + 4, file.Count);
                file.AddRange(encoded);
                file.Add(0);
            }

            File.WriteAllBytes(outputFile, file.ToArray());
        }

        public static void UnpackTextFolder(string inputFolder, string outputFolder)
        {
            Directory.CreateDirectory(outputFolder);
            foreach (var item in Directory.EnumerateFiles(inputFolder))
            {
                if (item.EndsWith(".TEXT"))
                {
                    string name = item[(item.LastIndexOf("\\") + 1)..item.LastIndexOf(".")];
                    string path = item[..item.LastIndexOf("\\")];

                    UnpackText(item, $@"{outputFolder}\{name}.txt");
                }
            }
        }

        public static void PackTextFolder(string inputFolder, string outputFolder)
        {
            Directory.CreateDirectory(outputFolder);
            foreach (var item in Directory.EnumerateFiles(inputFolder))
            {
                string name = item[(item.LastIndexOf("\\") + 1)..item.LastIndexOf(".")];
                string path = item[..item.LastIndexOf("\\")];

                PackText(item, $@"{outputFolder}\{name}.TEXT");
            }
        }


        public static void UnpackTextMonoBehaviour(string inputFile, string outputFile)
        {
            byte[] file = File.ReadAllBytes(inputFile);

            uint nameLength = file.ReadUintLE(0x1c);
            uint countPointer = 0x20 + nameLength;
            while (countPointer % 4 != 0)
            {
                countPointer++;
            }
            uint count = file.ReadUintLE(countPointer);
            uint pointer = countPointer + 0x4;


            List<string> strings = new((int)count);

            for (int i = 0; i < count; i++)
            {
                uint length = file.ReadUintLE(pointer + 4);
                byte[] currentString = file.SubArray(pointer + 8, length);
                string decoded = Encoding.UTF8.GetString(currentString);
                strings.Add(decoded);
                pointer += length + 8;
                while (pointer % 4 != 0)
                {
                    pointer++;
                }
            }

            using (StreamWriter writer = new StreamWriter(outputFile))
            {
                for (int i = 0; i < strings.Count; i++)
                {
                    writer.WriteLine($"{i:00000}={strings[i].Replace("\n", "\\n")}");
                }
            }
        }

        public static void PackTextMonoBehaviour(string inputFile, string outputFile)
        {
            byte[] file = File.ReadAllBytes(outputFile);

            uint nameLength = file.ReadUintLE(0x1c);
            uint countPointer = 0x20 + nameLength;
            while (countPointer % 4 != 0)
            {
                countPointer++;
            }
            uint count = file.ReadUintLE(countPointer);
            uint pointer = countPointer + 0x4;


            List<string> strings = new((int)count);
            List<uint> indexes = new((int)count);

            for (int i = 0; i < count; i++)
            {
                uint length = file.ReadUintLE(pointer + 4);
                byte[] currentString = file.SubArray(pointer + 8, length);
                string decoded = Encoding.UTF8.GetString(currentString);
                strings.Add(decoded);
                indexes.Add(file.ReadUintLE(pointer));
                pointer += length + 8;
                while (pointer % 4 != 0)
                {
                    pointer++;
                }
            }

            List<string> text = new List<string>();

            string[] stringsInFile = File.ReadAllLines(inputFile);
            foreach (var item in stringsInFile)
            {
                if (!(item == "" || item.StartsWith("//")))
                {
                    text.Add(item[(item.IndexOf("=") + 1)..].Replace("\\n", "\n"));
                }
            }

            if (strings.Count != text.Count)
            {
                throw new Exception($"Wrong strings count. Need {strings.Count}, present {text.Count}");
            }

            List<byte> newFile = new();
            newFile.AddRange(file.SubArray(0, countPointer + 0x4));

            for (int i = 0; i < count; i++)
            {
                byte[] encoded = Encoding.UTF8.GetBytes(text[i]);

                newFile.AddRange(indexes[i].ToBytearrLE());
                newFile.AddRange(encoded.Length.ToUintLE());
                newFile.AddRange(encoded);
                while (newFile.Count % 4 != 0)
                {
                    newFile.Add(0);
                }
            }

            File.WriteAllBytes(outputFile, newFile.ToArray());
        }


        public static void UnpackTextMonoBehaviourFolder(string inputFolder, string outputFolder)
        {
            Directory.CreateDirectory(outputFolder);
            foreach (var item in Directory.EnumerateFiles(inputFolder, "", SearchOption.AllDirectories))
            {
                if (item.EndsWith(".dat"))
                {
                    string name = item[(item.LastIndexOf("\\") + 1)..item.LastIndexOf(".")];

                    UnpackTextMonoBehaviour(item, $@"{outputFolder}\{name}.txt");
                }
            }
        }

        public static void PackTextMonoBehaviourFolder(string inputFolder)
        {
            foreach (var file in Directory.EnumerateFiles(inputFolder))
            {
                if (file.EndsWith(".txt"))
                {
                    string inputFile = file;
                    string outputFile = file[..file.LastIndexOf(".")] + ".dat";
                    if (File.Exists(outputFile))
                    {
                        PackTextMonoBehaviour(inputFile, outputFile);
                    }
                }
            }
        }

        public static void UnpackAssetBundle(string inputFile)
        {
            string outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), Path.GetFileNameWithoutExtension(inputFile));
            Directory.CreateDirectory(outputFolder);

            using FileStream fist = new FileStream(inputFile, FileMode.Open);
            BundleFileInstance buni = new BundleFileInstance(fist);
            AssetsManager am = new AssetsManager();
            AssetsFileInstance afi = am.LoadAssetsFileFromBundle(buni, 0, true);

            foreach (var item in afi.file.AssetInfos)
            {
                using MemoryStream ms = new MemoryStream();
                using AssetsFileWriter aw = new AssetsFileWriter(ms);

                afi.file.Write(aw);


                byte[] file = ms.ToArray().SubArray(item.GetAbsoluteByteOffset(afi.file), item.ByteSize);
                if (file.ReadUintLE(0) != 0)
                {
                    continue;
                }
                string name = file.SubArray(0x20, file.ReadUintLE(0x1c)).ReadString();
                string path = Path.Combine(outputFolder, name);

                using FileStream fs = new FileStream(path, FileMode.Create);
                fs.Write(file);
            }
        }

        public static void PackAsset(string inputFile, string inputAsset)
        {
            string outputFile = Path.Combine(Path.GetDirectoryName(inputFile), Path.GetFileNameWithoutExtension(inputFile) + Path.GetExtension(inputFile));

            using FileStream fist = new FileStream(inputFile, FileMode.Open);
            BundleFileInstance buni = new BundleFileInstance(fist);
            AssetsManager am = new AssetsManager();
            am.LoadClassPackage(@"classdata.tpk");
            ClassDatabaseFile cdf = am.LoadClassDatabaseFromPackage(buni.file.Header.EngineVersion);
            AssetsFileInstance afi = am.LoadAssetsFileFromBundle(buni, 0, true);

            for (int i = 0; i < afi.file.AssetInfos.Count; i++)
            {
                var item = afi.file.AssetInfos[i];
                string name = AssetHelper.GetAssetNameFast(afi.file, cdf, item);

                if (name + "-" + afi.name + "-" + item.PathId == Path.GetFileNameWithoutExtension(inputAsset))
                {
                    item.SetNewData(File.ReadAllBytes(inputAsset));
                    using MemoryStream ms1 = new MemoryStream();
                    using AssetsFileWriter aw1 = new AssetsFileWriter(ms1);
                    buni.file.BlockAndDirInfo.DirectoryInfos[0].SetNewData(afi.file);
                    buni.file.Write(aw1);

                    fist.Close();
                    using FileStream fs = new FileStream(outputFile, FileMode.Create);
                    fs.Write(ms1.ToArray());
                    return;
                }
            }
        }

        public static void PackAssetAll(string bundleFolder, string asssetFolder)
        {
            foreach (var bundle in Directory.EnumerateFiles(bundleFolder, "*", SearchOption.AllDirectories))
            {
                if (bundle.EndsWith(".bundle"))
                {
                    foreach (var asset in Directory.EnumerateFiles(asssetFolder))
                    {
                        if (asset.EndsWith(".dat"))
                        {
                            PackAsset(bundle, asset);
                        }
                    }
                }
            }
        }


        public static void PackAssetBundle(string inputFile, string inputFolder)
        {
            string outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), Path.GetFileNameWithoutExtension(inputFile));
            Directory.CreateDirectory(outputFolder);

            using FileStream fist = new FileStream(inputFile, FileMode.Open);
            BundleFileInstance buni = new BundleFileInstance(fist);
            AssetsManager am = new AssetsManager();
            AssetsFileInstance afi = am.LoadAssetsFileFromBundle(buni, 0, true);

            foreach (var item in afi.file.AssetInfos)
            {
                using MemoryStream ms = new MemoryStream();
                using AssetsFileWriter aw = new AssetsFileWriter(ms);

                afi.file.Write(aw);


                byte[] file = ms.ToArray().SubArray(item.GetAbsoluteByteOffset(afi.file), item.ByteSize);
                if (file.ReadUintLE(0) != 0)
                {
                    continue;
                }
                string name = file.SubArray(0x20, file.ReadUintLE(0x1c)).ReadString();
                string path = Path.Combine(outputFolder, name);

                using FileStream fs = new FileStream(path, FileMode.Create);
                fs.Write(file);
            }
        }



        public static void UnpackMycf(string inputFile, string outputFolder)
        {
            byte[] file = File.ReadAllBytes(inputFile);

            uint headerOffset = 0x4c;

            List<(uint offset, uint size)> files = new(300);
            files.Add((file.ReadUintLE(0xc), file.ReadUintLE(0x10)));


            for (int i = 0; ; i++)
            {
                uint offset = file.ReadUintLE(headerOffset + i * 8);
                uint size = file.ReadUintLE(headerOffset + i * 8 + 4);
                if (offset == 0 || size == 0)
                {
                    break;
                }
                files.Add((offset, size));
            }

            Directory.CreateDirectory(outputFolder);
            for (int i = 0; i < files.Count; i++)
            {
                //string extension = new string(file.SubArray(files[i].offset, 4).Select(x => (x >= 'A' && x <= 'Z' || x >= 'a' && x <= 'z' || x >= '0' && x <= '9') ? (char)x : '_').ToArray());
                string path = outputFolder + "\\file " + i.ToString("0000") + ".dat";
                File.WriteAllBytes(path, file.SubArray(files[i].offset, files[i].size));
            }
        }

        public static void CopyFiles(string start, string endsPath, string outputFolder)
        {
            string[] files = File.ReadAllLines(endsPath);
            Directory.CreateDirectory(outputFolder);
            foreach (var item in files)
            {
                string path = start + item;
                File.Copy(path, outputFolder + "\\" + Path.GetFileName(path));
            }
        }

        public static void ReadMycf(string inputFile, string outputFile)
        {
            byte[] input = File.ReadAllBytes(inputFile);

            List<ushort> ushortData = [];

            for (int i = 0; i < (input.Length - input.Length % 2); i += 2)
            {
                ushortData.Add(input.ReadShortLE(i));
            }

            List<(int line, List<ushort> data)> lineData = [];
            List<(int line, string description)> lineDescription = [];

            int startIndex = 0;
            ushort? last = null;
            ushort[] exceptions = [0x0, 0x2, 0x4, 0x5, 0x0006, 0x7, 0xc, 0xa];
            for (int i = 0; i < ushortData.Count(x => x == 0xfffc); i++)
            {
                int index = ushortData.IndexOf(0xfffc, startIndex);
                startIndex = index + 1;

                int lineNumber = ushortData[index + 1];
                int pointer = index - 1;
                List<ushort> data = [];
                for (int j = 0; pointer >= 0; j++)
                {
                    if (ushortData[pointer] == 0xfffe)
                    {
                        data.Add(0xfffe);
                        break;
                    }
                    if (ushortData[pointer] == j)
                    {
                        if (ushortData[pointer - 1] < 0x0047)
                        {
                            if (!exceptions.Contains(ushortData[pointer - 1]))
                            {
                                data.Add(ushortData[pointer - 1]);
                                break;
                            }
                        }
                        pointer -= 2;
                        j = 0;
                    }
                    if ((ushortData[pointer] & 0xff00) == 0xff00)
                    {
                        break;
                    }
                    else
                    {
                        pointer--;
                    }
                }
                if (data.Count > 0)
                {
                    lineDescription.Add((lineNumber, "Talker " + string.Join('/', data.Select(x => x.ToString("X4")))));
                    last = data[0];
                }
                else
                {
                    if (last != null)
                    {
                        lineDescription.Add((lineNumber, "Talker " + last.Value.ToString("X4") + "?"));
                    }
                    else
                    {
                        lineDescription.Add((lineNumber, "Talker"));
                    }
                }
            }

            startIndex = 0;
            for (int i = 0; i < ushortData.Count(x => x == 0xfff5); i++)
            {
                int index = ushortData.IndexOf(0xfff5, startIndex);
                startIndex = index + 1;

                ushort choicesNumber = ushortData[index + 1];
                for (int j = 0; j < choicesNumber; j++)
                {
                    int lineNumber = ushortData[index + 1 + choicesNumber + 1 + j * 2];
                    lineDescription.Add((lineNumber, "Choice"));
                }
            }

            startIndex = 0;
            for (int i = 0; i < ushortData.Count(x => x == 0xfff4); i++)
            {
                int index = ushortData.IndexOf(0xfff4, startIndex);
                startIndex = index + 1;

                ushort choicesNumber = ushortData[index + 1];
                for (int j = 0; j < choicesNumber; j++)
                {
                    int lineNumber = ushortData[index + 1 + choicesNumber + 1 + 1 + j * 2];
                    lineDescription.Add((lineNumber, "Choice 2"));
                }
            }


            List<string> outputStrings = [];

            lineDescription = lineDescription.OrderBy(x => x.line).ToList();

            for (int i = 0; i < lineDescription.Count; i++)
            {
                string lineText = $"{lineDescription[i].line.ToString("00000")}={lineDescription[i].description}";
                outputStrings.Add(lineText);
            }

            File.WriteAllLines(outputFile, outputStrings);
        }

        public static void ReadMycfOld(string inputFile, string outputFile)
        {
            byte[] input = File.ReadAllBytes(inputFile);

            List<List<ushort>> dialogdata = [];
            List<ushort> current = [];

            for (int i = 0xe8; i < input.Length - 1; i += 2)
            {
                ushort item = input.ReadShortLE(i);
                if (item == 0xffff)
                {
                    dialogdata.Add(current);
                    current = [];
                }
                else
                {
                    current.Add(item);
                }
            }

            List<List<ushort>> dialogs = [];
            ushort character;
            ushort count;
            int skipped = 0;

            for (int i = 0; i < dialogdata.Count; i++)
            {
                if (dialogdata[i].Count(x => x == 0xfffc) > 1)
                {
                    skipped++;
                    continue;
                }
                if (!dialogdata[i].Contains(0xfffc))
                {
                    skipped++;
                    continue;
                }
                //ushort textIndex = dialogdata[i].IndexOf();
                List<ushort> dialog = [];
                for (int j = 0; j < dialogdata[i].Count;)
                {

                    if (dialogdata[i][j] == 0xfffc)
                    {
                        _ = 0;
                    }
                    character = dialogdata[i][j];
                    j++;
                    if (j == dialogdata[i].Count)
                    {
                        break;
                    }
                    count = dialogdata[i][j];
                    j++;
                    if (count > 100)
                    {
                        break;
                    }
                    for (int k = 0; k < count; k++)
                    {
                        dialog.Add(character);
                        j++;
                    }
                }
                dialogs.Add(dialog);
            }

            List<string> outputStrings = [];

            for (int i = 0; i < dialogs.Count; i++)
            {
                for (int j = 0; j < dialogs[i].Count; j++)
                {
                    outputStrings.Add("//Talker = " + dialogs[i][j].ToString("X4"));
                }
                outputStrings.Add("");
            }

            File.WriteAllLines(outputFile, outputStrings);
        }


        public static void ReadMycfFolder(string inputFolder, string outputFolder)
        {
            Directory.CreateDirectory(outputFolder);
            foreach (var item in Directory.EnumerateFiles(inputFolder))
            {
                string name = Path.GetFileNameWithoutExtension(item);
                ReadMycf(item, $@"{outputFolder}\{name}.txt");
            }

            List<string> strings = [];
            foreach (var item in Directory.EnumerateFiles(outputFolder))
            {
                if (Path.GetFileNameWithoutExtension(item).Contains("file"))
                {
                    strings.AddRange(File.ReadAllLines(item));
                }
            }
            File.WriteAllLines($@"{outputFolder}\all.txt", strings);
        }


        public static void InsertTalker(string inputText, string inputTalker, string outputFile)
        {
            List<string> textStrings = new List<string>();
            using (StreamReader file1 = File.OpenText(inputText))
            {
                string? s;
                while ((s = file1.ReadLine()) != null)
                {
                    if (s != "" && !s.StartsWith("//"))
                    {
                        textStrings.Add(s);
                    }
                }
            }

            List<string> talkerStrings = new List<string>();
            using (StreamReader file1 = File.OpenText(inputTalker))
            {
                string? s;
                while ((s = file1.ReadLine()) != null)
                {
                    talkerStrings.Add(s);
                }
            }

            if (textStrings.Count != talkerStrings.Count) throw new Exception();

            using (StreamWriter output = File.CreateText(outputFile))
            {
                for (int i = 0; i < textStrings.Count; i++)
                {
                    if (textStrings[i] != "")
                    {
                        output.WriteLine(talkerStrings[i]);
                        output.WriteLine(textStrings[i]);
                        output.WriteLine("");
                    }
                }
            }
        }

        static int feffCount = 0;
        static int feffCount2 = 0;


        public static void ReadMycfNew(string inputFile, string outputFile)
        {
            string[] namesFile = File.ReadAllLines(@"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\диалоги\talker names.txt");
            Dictionary<ushort, string> namesDict = [];
            foreach (var item in namesFile)
            {
                namesDict.Add(ushort.Parse(item.Split()[1], System.Globalization.NumberStyles.HexNumber), item.Split()[2]);
            }
            byte[] input = File.ReadAllBytes(inputFile);

            List<ushort> ushortData = [];

            for (int i = 0; i < (input.Length - input.Length % 2); i += 2)
            {
                ushortData.Add(input.ReadShortLE(i));
            }

            List<(int line, string description)> lineDescription = [];

            int startIndex = 0;
            ushort? last = null;
            ushort[] exceptions = [0x0, 0x2, 0x4, 0x5, 0x0006, 0x7, 0xc, 0xa];
            for (int i = 0; i < ushortData.Count(x => x == 0xfffc); i++)
            {
                int index = ushortData.IndexOf(0xfffc, startIndex);
                startIndex = index + 1;

                int lineNumber = ushortData[index + 1];
                int pointer = index - 1;
                List<ushort> data = [];
                List<ushort> dataLog = [];
                for (int j = 0; pointer >= 0; j++)
                {
                    if (ushortData[pointer] == 0xfffe)
                    {
                        feffCount2++;
                        dataLog.Add(ushortData[pointer]);
                        ushort fffePointerData = input.ReadShortLE(ushortData[pointer + 1]);
                        if (fffePointerData < 0x0047)
                        {
                            if (!exceptions.Contains(fffePointerData))
                            {
                                feffCount++;
                                data.Add(fffePointerData);
                                break;
                            }
                        }
                    }
                    if (ushortData[pointer] == j)
                    {
                        dataLog.Add(ushortData[pointer - 1]);
                        if (ushortData[pointer - 1] < 0x0047)
                        {
                            if (!exceptions.Contains(ushortData[pointer - 1]))
                            {
                                data.Add(ushortData[pointer - 1]);
                                break;
                            }
                        }
                        pointer -= 2;
                        j = 0;
                    }
                    if ((ushortData[pointer] & 0xff00) == 0xff00)
                    {
                        break;
                    }
                    else
                    {
                        pointer--;
                    }
                }

                if (data.Count > 0)
                {
                    namesDict.TryGetValue(data[0], out string? value);
                    string talkerName = value ?? string.Join('/', data.Select(x => x.ToString("X4")));

                    lineDescription.Add((lineNumber, "Talker " + talkerName));
                    // lineDescription.Add((lineNumber, "Talker " + talkerName + "("+ string.Join('|', dataLog.Select(x => x.ToString("X4"))) + ")"));
                    last = data[0];
                }
                else
                {
                    if (last != null)
                    {
                        namesDict.TryGetValue(last.Value, out string? value);
                        string talkerName = value ?? last.Value.ToString("X4");

                        lineDescription.Add((lineNumber, "Talker " + talkerName + "?"));
                        //lineDescription.Add((lineNumber, "Talker " + talkerName + "?" + "(" + string.Join('|', dataLog.Select(x => x.ToString("X4"))) + ")"));
                    }
                    else
                    {
                        lineDescription.Add((lineNumber, "Talker" + "(" + string.Join('|', dataLog.Select(x => x.ToString("X4"))) + ")"));
                    }
                }
            }

            startIndex = 0;
            for (int i = 0; i < ushortData.Count(x => x == 0xfff5); i++)
            {
                int index = ushortData.IndexOf(0xfff5, startIndex);
                startIndex = index + 1;

                ushort choicesNumber = ushortData[index + 1];
                for (int j = 0; j < choicesNumber; j++)
                {
                    int lineNumber = ushortData[index + 1 + choicesNumber + 1 + j * 2];
                    lineDescription.Add((lineNumber, "Choice"));
                }
            }

            startIndex = 0;
            for (int i = 0; i < ushortData.Count(x => x == 0xfff4); i++)
            {
                int index = ushortData.IndexOf(0xfff4, startIndex);
                startIndex = index + 1;

                ushort choicesNumber = ushortData[index + 1];
                for (int j = 0; j < choicesNumber; j++)
                {
                    int lineNumber = ushortData[index + 1 + choicesNumber + 1 + 1 + j * 2];
                    lineDescription.Add((lineNumber, "Choice 2"));
                }
            }


            List<string> outputStrings = [];

            lineDescription = lineDescription.OrderBy(x => x.line).ToList();

            for (int i = 0; i < lineDescription.Count; i++)
            {
                string lineText = $"{lineDescription[i].line.ToString("00000")}={lineDescription[i].description}";
                outputStrings.Add(lineText);
            }

            File.WriteAllLines(outputFile, outputStrings);
        }
        public static void ReadMycfFolderNew(string inputFolder, string outputFolder)
        {
            Directory.CreateDirectory(outputFolder);
            foreach (var item in Directory.EnumerateFiles(inputFolder))
            {
                string name = Path.GetFileNameWithoutExtension(item);
                ReadMycfNew(item, $@"{outputFolder}\{name}.txt");
            }

            List<string> strings = [];
            foreach (var item in Directory.EnumerateFiles(outputFolder))
            {
                if (Path.GetFileNameWithoutExtension(item).Contains("file"))
                {
                    strings.AddRange(File.ReadAllLines(item));
                }
            }
            File.WriteAllLines($@"{outputFolder}\all.txt", strings);
            Console.WriteLine("feff " + feffCount);
            Console.WriteLine("feff2 " + feffCount2);
        }

        public static void ReadMycfNew2(string inputFile, string outputFile)
        {
            string[] namesFile = File.ReadAllLines(@"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\диалоги\talker names.txt");
            Dictionary<ushort, string> namesDict = [];
            foreach (var item in namesFile)
            {
                namesDict.Add(ushort.Parse(item.Split()[1], System.Globalization.NumberStyles.HexNumber), item.Split()[2]);
            }

            string[] itemsFile = File.ReadAllLines(@"C:\My files\Перевод игр\Rune Factory 3\Инструментарий\Text\text rus\file 7722.txt");
            Dictionary<int, string> itemsDict = [];
            foreach (var item in itemsFile)
            {
                itemsDict.Add(int.Parse(item[..item.IndexOf('=')]), item[(item.IndexOf('=') + 1)..]);
            }

            byte[] input = File.ReadAllBytes(inputFile);

            List<ushort> ushortData = [];

            for (int i = 0; i < (input.Length - input.Length % 2); i += 2)
            {
                ushortData.Add(input.ReadShortLE(i));
            }

            List<(int line, string description)> lineDescription = [];

            int startIndex = 0;
            ushort? last = null;
            ushort[] exceptions = [0x0, 0x2, 0x4, 0x5, 0x0006, 0x7, 0xc, 0xa];
            for (int i = 0; i < ushortData.Count(x => x == 0xfffc); i++)
            {
                int index = ushortData.IndexOf(0xfffc, startIndex);
                startIndex = index + 1;

                int lineNumber = ushortData[index + 1];
                int pointer = index - 1;
                List<ushort> data = [];
                List<ushort> items = [];
                List<ushort> dataLog = [];
                for (int j = 0; pointer >= 0; j++)
                {
                    if (ushortData[pointer] == 0xfffe)
                    {
                        feffCount2++;
                        dataLog.Add(ushortData[pointer]);
                        ushort fffePointerData = input.ReadShortLE(ushortData[pointer + 1]);
                        if (fffePointerData < 0x0047)
                        {
                            if (!exceptions.Contains(fffePointerData))
                            {
                                feffCount++;
                                data.Add(fffePointerData);
                                //break;
                            }
                        }
                        else if (fffePointerData == 0x00da)
                        {
                            feffCount++;
                            ushort metadataPointer = input.ReadShortLE(ushortData[pointer + 1] + 6);
                            //if (input[metadataPointer] ==0x18)
                            //{
                            Console.WriteLine(input[metadataPointer]);
                            ushort itemId = input.ReadShortLE(metadataPointer + 1);
                            items.Add(itemId);
                            //}
                        }
                        j = 0;
                        pointer -= 1;
                    }
                    if (ushortData[pointer] == j)
                    {
                        dataLog.Add(ushortData[pointer - 1]);
                        if (ushortData[pointer - 1] < 0x0047)
                        {
                            if (!exceptions.Contains(ushortData[pointer - 1]))
                            {
                                data.Add(ushortData[pointer - 1]);
                                //    break;
                            }
                        }
                        else if (ushortData[pointer - 1] == 0x00da)
                        {
                            ushort metadataPointer = ushortData[pointer + 2];
                            //if (input[metadataPointer] == 0x18)
                            //{
                            Console.WriteLine(input[metadataPointer]);
                            ushort itemId = input.ReadShortLE(metadataPointer + 1);
                            items.Add(itemId);
                            //}
                        }
                        pointer -= 2;
                        j = 0;
                    }
                    if ((ushortData[pointer] & 0xff00) == 0xff00)
                    {
                        break;
                    }
                    else
                    {
                        pointer--;
                    }
                }

                items.Reverse();

                if (data.Count > 0)
                {
                    namesDict.TryGetValue(data[0], out string? value);
                    string talkerName = value ?? string.Join('/', data.Select(x => x.ToString("X4")));

                    if (items.Count == 0)
                    {
                        lineDescription.Add((lineNumber, "Talker " + talkerName));
                    }
                    else
                    {
                        lineDescription.Add((lineNumber, "Talker " + talkerName + " Items " + string.Join(" ", items.Select(x => { itemsDict.TryGetValue(x, out string? value); return value ?? x.ToString(); }))));
                    }
                    // lineDescription.Add((lineNumber, "Talker " + talkerName + "("+ string.Join('|', dataLog.Select(x => x.ToString("X4"))) + ")"));
                    last = data[0];
                }
                else
                {
                    if (last != null)
                    {
                        namesDict.TryGetValue(last.Value, out string? value);
                        string talkerName = value ?? last.Value.ToString("X4");

                        if (items.Count == 0)
                        {
                            lineDescription.Add((lineNumber, "Talker " + talkerName + "?"));
                        }
                        else
                        {
                            lineDescription.Add((lineNumber, "Talker " + talkerName + "?" + " Items " + string.Join(" ", items.Select(x => { itemsDict.TryGetValue(x, out string? value); return value ?? x.ToString(); }))));
                        }
                        //lineDescription.Add((lineNumber, "Talker " + talkerName + "?" + "(" + string.Join('|', dataLog.Select(x => x.ToString("X4"))) + ")"));
                    }
                    else
                    {
                        lineDescription.Add((lineNumber, "Talker" + "(" + string.Join('|', dataLog.Select(x => x.ToString("X4"))) + ")"));
                    }
                }
            }

            {
                List<ushort> items = [];
                List<ushort> lines = [];

                for (int i = 0x74; i < ushortData.Count; i++)
                {
                    if (ushortData[i] == 0xffff)
                    {
                        if (items.Count != 0)
                        {


                            for (int j = 0; j < lines.Count; j++)
                            {
                                if (lineDescription.Where(x => x.line == lines[j]).Any())
                                {
                                    var description = lineDescription.Where(x => x.line == lines[j]).First();
                                    lineDescription.Remove(description);
                                    lineDescription.Add((description.line, description.description + " Items " + string.Join(" ", items.Select(x => { itemsDict.TryGetValue(x, out string? value); return value ?? x.ToString(); }))));
                                }
                            }
                        }
                        items = [];
                        lines = [];
                    }
                    else if (ushortData[i] == 0x00da)
                    {
                        ushort metadataPointer = ushortData[i + 3];
                        if (metadataPointer + 3 < input.Length)
                        {
                            ushort itemId = input.ReadShortLE(metadataPointer + 1);
                            items.Add(itemId);
                        }
                    }
                    else if (ushortData[i] == 0xfffe)
                    {
                        if (ushortData[i + 1] < ushortData.Count)
                        {
                            int index = ushortData.IndexOf(0xfffc, ushortData[i + 1]);
                            if (index > 0)
                            {
                                ushort lineNumber = ushortData[index + 1];
                                lines.Add(lineNumber);
                            }
                        }
                    }
                }
            }

            startIndex = 0;
            for (int i = 0; i < ushortData.Count(x => x == 0xfff5); i++)
            {
                int index = ushortData.IndexOf(0xfff5, startIndex);
                startIndex = index + 1;

                ushort choicesNumber = ushortData[index + 1];
                for (int j = 0; j < choicesNumber; j++)
                {
                    int lineNumber = ushortData[index + 1 + choicesNumber + 1 + j * 2];
                    lineDescription.Add((lineNumber, "Choice"));
                }
            }

            startIndex = 0;
            for (int i = 0; i < ushortData.Count(x => x == 0xfff4); i++)
            {
                int index = ushortData.IndexOf(0xfff4, startIndex);
                startIndex = index + 1;

                ushort choicesNumber = ushortData[index + 1];
                for (int j = 0; j < choicesNumber; j++)
                {
                    int lineNumber = ushortData[index + 1 + choicesNumber + 1 + 1 + j * 2];
                    lineDescription.Add((lineNumber, "Choice 2"));
                }
            }


            List<string> outputStrings = [];

            lineDescription = lineDescription.OrderBy(x => x.line).ToList();

            for (int i = 0; i < lineDescription.Count; i++)
            {
                string lineText = $"{lineDescription[i].line.ToString("00000")}={lineDescription[i].description}";
                outputStrings.Add(lineText);
            }

            File.WriteAllLines(outputFile, outputStrings);
        }
        public static void ReadMycfFolderNew2(string inputFolder, string outputFolder)
        {
            Directory.CreateDirectory(outputFolder);
            foreach (var item in Directory.EnumerateFiles(inputFolder))
            {
                string name = Path.GetFileNameWithoutExtension(item);
                ReadMycfNew2(item, $@"{outputFolder}\{name}.txt");
            }

            List<string> strings = [];
            foreach (var item in Directory.EnumerateFiles(outputFolder))
            {
                if (Path.GetFileNameWithoutExtension(item).Contains("file"))
                {
                    strings.AddRange(File.ReadAllLines(item));
                }
            }
            File.WriteAllLines($@"{outputFolder}\all.txt", strings);
            Console.WriteLine("feff " + feffCount);
            Console.WriteLine("feff2 " + feffCount2);
        }

        public static void SearchSameText(string inputFile, string outputFile)
        {
            string[] strings = File.ReadAllLines(inputFile);

            List<(int line, string text)> text = new();

            foreach (var item in strings)
            {
                if (!(item == "" || item.StartsWith("//")))
                {
                    text.Add((int.Parse(item[..(item.IndexOf("="))]), item[(item.IndexOf("=") + 1)..]));
                }
            }

            Dictionary<string, int> sameCount = new();
            foreach (var item in text)
            {
                if (sameCount.TryGetValue(item.text, out int value))
                {
                    sameCount[item.text] = value + 1;
                }
                else
                {
                    sameCount.Add(item.text, 1);
                }
            }

            List<string> output = sameCount.Where(x => x.Value > 1).OrderByDescending(x => x.Value).SelectMany(x => new List<string>() { x.Value.ToString(), text.Where(y => y.text == x.Key).First().line.ToString(), x.Key, "" }).ToList();

            File.WriteAllLines(outputFile, output);
        }

        public static void SearchSameText2(string inputFile, string outputFile, int start)
        {
            int treshold = 5;

            string[] strings = File.ReadAllLines(inputFile);

            List<(int line, string text)> text = new();

            foreach (var item in strings)
            {
                if (!(item == "" || item.StartsWith("//")))
                {
                    text.Add((int.Parse(item[..(item.IndexOf("="))]), item[(item.IndexOf("=") + 1)..]));
                }
            }

            List<(int line, int found, int len)> found = new();

            for (int i = start; i < text.Count;)
            {
                List<(int line, int found, int len)> foundtemp = new();
                for (int j = 0; j < start; j++)
                {
                    int len = 0;
                    for (int k = 0; ; k++)
                    {
                        if (text[i + k].text == text[j + k].text)
                        {
                            len++;
                        }
                        else break;
                    }
                    if (len >= treshold)
                    {
                        foundtemp.Add((i, j, len));
                    }
                }
                if (foundtemp.Count == 0)
                {
                    i++;
                }
                else
                {
                    var bestMatch = foundtemp.OrderByDescending(x => x.len).First();
                    found.Add(bestMatch);
                    i += bestMatch.len;
                }
            }

            List<string> output = new();
            foreach (var item in found)
            {
                output.Add(item.line.ToString());
                output.Add(item.found.ToString());
                for (int i = item.found; i < item.found + item.len; i++)
                {
                    output.Add(text[i].text);
                }
                output.Add("");
            }

            File.WriteAllLines(outputFile, output);
        }

        public static void PasteSameText(string inputFile, string outputFile, string rusFile, string newRusFile)
        {
            int treshold = 2;

            string[] strings = File.ReadAllLines(inputFile);

            List<(int line, string text)> text = new();

            foreach (var item in strings)
            {
                if (!(item == "" || item.StartsWith("//")))
                {
                    text.Add((int.Parse(item[..(item.IndexOf("="))]), item[(item.IndexOf("=") + 1)..]));
                }
            }

            List<(int line, int found, int len)> found = new();

            for (int i = 0; i < text.Count;)
            {
                List<(int line, int found, int len)> foundtemp = new();
                for (int j = 0; j < i; j++)
                {
                    int len = 0;
                    for (int k = 0; ; k++)
                    {
                        if (j + k >= i)
                        {
                            break;
                        }
                        if (text[i + k].text == text[j + k].text)
                        {
                            len++;
                        }
                        else break;
                    }
                    if (len >= treshold)
                    {
                        foundtemp.Add((i, j, len));
                    }
                }
                if (foundtemp.Count == 0)
                {
                    i++;
                }
                else
                {
                    var bestMatch = foundtemp.OrderByDescending(x => x.len).First();
                    found.Add(bestMatch);
                    i += bestMatch.len;
                }
            }

            List<string> output = new();
            foreach (var item in found)
            {
                output.Add(item.line.ToString());
                output.Add(item.found.ToString());
                for (int i = item.found; i < item.found + item.len; i++)
                {
                    output.Add(text[i].text);
                }
                output.Add("");
            }

            File.WriteAllLines(outputFile, output);

            string[] rusStrings = File.ReadAllLines(rusFile);
            List<(int line, string text)> rusText = new();

            foreach (var item in rusStrings)
            {
                if (!(item == "" || item.StartsWith("//")))
                {
                    rusText.Add((int.Parse(item[..(item.IndexOf("="))]), item[(item.IndexOf("=") + 1)..]));
                }
            }


            foreach (var item in found)
            {
                for (int i = 0; i < item.len; i++)
                {
                    for (int j = 0; j < rusStrings.Length; j++)
                    {
                        if (!(rusStrings[j] == "" || rusStrings[j].StartsWith("//")))
                        {
                            if (int.Parse(rusStrings[j][..(rusStrings[j].IndexOf("="))]) == item.line + i)
                            {
                                string newStr = rusStrings[j][..(rusStrings[j].IndexOf("=") + 1)] + rusText[item.found + i].text;
                                rusStrings[j] = newStr;
                                break;
                            }
                        }
                    }
                }
            }

            File.WriteAllLines(newRusFile, rusStrings);
        }

        public static int CountCharacters(string word)
        {
            word = word
                .Replace("...", "-")
                .Replace("＠ヒーロー＠", "--------")
                .Replace("＠嫁＠", "--------")
                .Replace("＠０＠", "----------")
                .Replace("＠１＠", "----------")
                .Replace("＠２＠", "----------")
                .Replace("＠３＠", "----------")
                .Replace("＠４＠", "----------")
                .Replace("＠５＠", "----------")
                .Replace("＠数字０＠", "----------")
                .Replace("＠数字１＠", "----------")
                .Replace("＠数字２＠", "----------")
                .Replace("＠数字３＠", "----------")
                .Replace("＠数字４＠", "----------")
                .Replace("＠数字５＠", "----------")
                .Replace("＠数字６＠", "----------")
                .Replace("＠数字７＠", "----------")
                .Replace("＠数字８＠", "----------")
                .Replace("＠数字９＠", "----------")
                .Replace("＠色＠", "")
                .Replace("＠黒＠", "")
                .Replace("＠小屋０＠", "--------")
                .Replace("＠小屋１＠", "--------")
                .Replace("＠小屋２＠", "--------")
                .Replace("＠小屋３＠", "--------")
                .Replace("＠小屋４＠", "--------")
                .Replace("＠キャラ０＠", "--------------------")
                .Replace("＠キャラ１＠", "--------------------")
                .Replace("＠キャラ２＠", "--------------------")
                .Replace("＠キャラ３＠", "--------------------")
                .Replace("＠マップ０＠", "------------------------------")
                .Replace("＠マップ１＠", "------------------------------")
                .Replace("＠マップ２＠", "------------------------------")
                .Replace("＠会話モン＠", "--------")
                .Replace("＠牧場＠", "--------")
                .Replace("＠カレンダー０＠", "------------------------------")
                .Replace("＠子供０＠", "--------")
                .Replace("＠アイテム０＠", "-------------------------")
                .Replace("＠アイテム１＠", "-------------------------")
                .Replace("＠アイテム２＠", "-------------------------")
                .Replace("＠アイテム３＠", "-------------------------")
                .Replace("＠アイテム４＠", "-------------------------")
                .Replace("＠アイテム５＠", "-------------------------")
                .Replace("＠アイテム６＠", "-------------------------")
                .Replace("＠アイテム７＠", "-------------------------")
                .Replace("＠アイテム８＠", "-------------------------")
                .Replace("＠アイテム９＠", "-------------------------");

            return word.Length;

            //＠ヒーロー＠ 8
            //＠嫁＠ 7 имя
            //＠０＠ 10
            //＠１＠ 10
            //＠２＠ 10
            //＠３＠ 10
            //＠４＠ 10
            //＠５＠ 10
            //＠数字０＠ 10 количество
            //＠数字１＠ 10
            //＠数字２＠ 10
            //＠数字３＠ 10
            //＠数字４＠ 10
            //＠数字５＠ 10
            //＠数字６＠ 10
            //＠数字７＠ 10
            //＠数字８＠ 10
            //＠数字９＠ 10
            //＠数字０＠ 
            //＠数字１＠
            //＠色＠ 0
            //＠黒＠ 0
            //＠小屋０＠ 8 имя монстра
            //＠小屋１＠ 8 
            //＠小屋２＠ 8
            //＠小屋３＠ 8
            //＠小屋４＠ 8
            //＠キャラ０＠ 20 имя (любое)
            //＠キャラ１＠ 20
            //＠キャラ２＠ 20
            //＠キャラ３＠ 20
            //＠マップ０＠ 30 место
            //＠マップ１＠ 30
            //＠マップ２＠ 30
            //＠会話モン＠ 8
            //＠牧場＠ 8 ферма
            //＠カレンダー０＠ 30 фестиваль
            // 1
            // 1
            // 1
            //＠子供０＠ 8 ребёнок
            //＠アイテム０＠ 25 предмет
            //＠アイテム１＠ 25
            //＠アイテム２＠ 25
            //＠アイテム３＠ 25
            //＠アイテム４＠ 25
            //＠アイテム５＠ 25
            //＠アイテム６＠ 25
            //＠アイテム７＠ 25
            //＠アイテム８＠ 25
            //＠アイテム９＠ 25
        }

        static void SplitIntoLinesFolder(string inputPath)
        {
            foreach (var file in Directory.EnumerateFiles(inputPath))
            {
                string newFileName = file[..file.LastIndexOf(".")] + " new.txt";
                SplitIntoLines(file, newFileName);
            }
        }

        static void SplitIntoLines(string inputPath, string outputPath)
        {
            int lines = 4;
            int maxCharNumber = 50;
            List<string> strings = new List<string>();

            using (StreamReader sr = File.OpenText(inputPath))
            {
                string s;
                while ((s = sr.ReadLine()) != null)
                {
                    strings.Add(s);
                }
            }

            for (int i = 0; i < strings.Count; i++)
            {
                if (!(strings[i].StartsWith("//") || strings[i] == ""))
                {
                    string left = strings[i][..6];
                    string right = strings[i][6..];
                    string? comment = null;
                    if (i > 0)
                    {
                        comment = strings[i - 1];
                    }
                    if (right.IndexOf("\\n") < 0 && comment?.StartsWith("//Выбор") == false)
                    {
                        string newStr = "";
                        string[] words = right.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                        int currentLines = 1;
                        int currentLength = 0;

                        for (int j = 0; j < words.Length; j++)
                        {
                            if (currentLength + CountCharacters(words[j]) + 1 > maxCharNumber)
                            {
                                newStr += "\\n" + words[j];
                                currentLines++;
                                currentLength = CountCharacters(words[j]);
                            }
                            else
                            {
                                if (currentLength != 0)
                                {
                                    newStr += " ";
                                    currentLength += 1;
                                }
                                newStr += words[j];
                                currentLength += CountCharacters(words[j]);
                            }
                        }
                        if (currentLines <= lines)
                        {
                            right = newStr;
                        }
                        else Console.WriteLine(left);
                    }
                    strings[i] = left + right;
                }
            }
            using (StreamWriter sw = File.CreateText(outputPath))
            {
                foreach (var item in strings)
                {
                    sw.WriteLine(item);
                }
            }
        }

        public static void CountWords(string inputFile, string output)
        {
            Dictionary<string, int> words = new Dictionary<string, int>();

            List<string> strings1 = new List<string>();

            using (StreamReader sr = File.OpenText(inputFile))
            {
                string s;
                while ((s = sr.ReadLine()) != null)
                {
                    strings1.Add(s);
                }
            }
            foreach (string item in strings1)
            {
                if (!item.StartsWith("//") && item != "")
                {
                    string text = item[6..]
                        .Replace("\\n", " ")
                        .Replace("＋", "")
                        .Replace(".", "")
                        .Replace("!", "")
                        .Replace("?", "")
                        .Replace(",", "")
                        .Replace("(", "")
                        .Replace(")", "")
                        .Replace("«", "")
                        .Replace("»", "")
                        .Replace("／", "");
                    string[] textWords = text.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                    foreach (var word in textWords)
                    {
                        if (!words.TryAdd(word, 1))
                        {
                            words[word] += 1;
                        }
                    }
                }
            }

            using (StreamWriter sw = File.CreateText(output))
            {
                foreach (var str in words.OrderBy(x => x.Value))
                {
                    sw.WriteLine(str.Key + " " + str.Value);
                }
            }

        }

        static void SplitIntoLines2(string inputPath, string outputPath)
        {
            int lines = 7;
            int maxCharNumber = 38;
            List<string> strings = new List<string>();

            using (StreamReader sr = File.OpenText(inputPath))
            {
                string s;
                while ((s = sr.ReadLine()) != null)
                {
                    strings.Add(s);
                }
            }

            for (int i = 0; i < strings.Count; i++)
            {
                if (!(strings[i].StartsWith("//") || strings[i] == ""))
                {
                    string left = strings[i][..6];
                    string right = strings[i][6..];
                    string? comment = null;
                    if (i > 0)
                    {
                        comment = strings[i - 1];
                    }
                    if (right.IndexOf("\\n") < 0 && comment?.StartsWith("//Выбор") != true)
                    {
                        string newStr = "";
                        string[] words = right.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                        int currentLines = 1;
                        int currentLength = 0;

                        for (int j = 0; j < words.Length; j++)
                        {
                            if (currentLength + CountCharacters(words[j]) + 1 > maxCharNumber)
                            {
                                newStr += "\\n" + words[j];
                                currentLines++;
                                currentLength = CountCharacters(words[j]);
                            }
                            else
                            {
                                if (currentLength != 0)
                                {
                                    newStr += " ";
                                    currentLength += 1;
                                }
                                newStr += words[j];
                                currentLength += CountCharacters(words[j]);
                            }
                        }
                        if (currentLines <= lines)
                        {
                            right = newStr;
                        }
                        else Console.WriteLine(left);
                    }
                    strings[i] = left + right;
                }
            }
            using (StreamWriter sw = File.CreateText(outputPath))
            {
                foreach (var item in strings)
                {
                    sw.WriteLine(item);
                }
            }
        }


        static void Main(string[] args)
        {
            if (true)
            {
                switch (args.Length)
                {
                    case 0:
                        Console.WriteLine("Rune Factory 3 translation tools");
                        Console.WriteLine("Usage:");
                        Console.WriteLine("AutoNewLine");
                        Console.WriteLine("RF3T.exe -anl textFile newTextFile");
                        Console.WriteLine("PackTextFolder");
                        Console.WriteLine("RF3T.exe -ptf textFolder gameTextFolder");
                        Console.WriteLine("PackTextPseudoarchiveFolder");
                        Console.WriteLine("RF3T.exe -ptp gameTextFolder datFile");
                        Console.WriteLine("PackTextMonoBehaviour");
                        Console.WriteLine("RF3T.exe -ptmb textFromMonobehaviourFile datFile");
                        Console.WriteLine("PackAsset");
                        Console.WriteLine("RF3T.exe -pa bundleFile datFile");
                        Console.WriteLine("UnpackTextPseudoarchiveFolder");
                        Console.WriteLine("RF3T.exe -utp datFile gameTextFolder");
                        Console.WriteLine("UnpackTextFolder");
                        Console.WriteLine("RF3T.exe -utf gameTextFolder textFolder");
                        break;
                    case 3:
                        switch (args[0])
                        {
                            case "-anl":
                                SplitIntoLines(args[1], args[2]);
                                Console.WriteLine("AutoNewLine complete");
                                break;
                            case "-ptf":
                                PackTextFolder(args[1], args[2]);
                                Console.WriteLine("PackTextFolder complete");
                                break;
                            case "-ptp":
                                PackTextPseudoarchiveFolder(args[1], args[2]);
                                Console.WriteLine("PackTextPseudoarchiveFolder complete");
                                break;
                            case "-ptmb":
                                PackTextMonoBehaviour(args[1], args[2]);
                                Console.WriteLine("PackTextMonoBehaviour complete");
                                break;
                            case "-pa":
                                PackAsset(args[1], args[2]);
                                Console.WriteLine("PackAsset complete");
                                break;
                            case "-utp":
                                UnpackTextPseudoarchive(args[1], args[2]);
                                Console.WriteLine("UnpackTextPseudoarchiveFolder complete");
                                break;
                            case "-utf":
                                UnpackTextFolder(args[1], args[2]);
                                Console.WriteLine("UnpackTextFolder complete");
                                break;
                            default:
                                Console.WriteLine("Wrong argument");
                                break;
                        }
                        break;
                    default:
                        Console.WriteLine("Wrong arguments count");
                        break;
                }
            }
            else
            {
                //UnpackTextPseudoarchive(@"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\rf3Archive_eng-CAB-f17fe7d3fe4fd5ecb924dfa2cb195590-6531742534647681148.dat", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\files");
                //UnpackText(@"C:\My files\Перевод игр\Rune Factory 3\попытка\Новая папка (3)\file 0028.TEXT", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Новая папка (3)\file 0028.txt");

                //PackText(@"C:\My files\Перевод игр\Rune Factory 3\попытка\Новая папка (3)\file0028.txt", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Новая папка (3)\file0028 1.TEXT");
                //PackTextPseudoarchive(@"C:\My files\Перевод игр\Rune Factory 3\попытка\Новая папка (4)\file 0028.TEXT", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Новая папка (4)\rf3Archive_eng-CAB-f17fe7d3fe4fd5ecb924dfa2cb195590-6531742534647681148.dat");

                //UnpackTextFolder(@"C:\My files\Перевод игр\Rune Factory 3\попытка\Новая папка (3)", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Новая папка (3) 1");


                //PackText(@"C:\My files\Перевод игр\Rune Factory 3\попытка\Новая папка (3) 1\file 0028.txt", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Текст\file 0028.TEXT");
                //PackTextPseudoarchive(@"C:\My files\Перевод игр\Rune Factory 3\попытка\Текст\file 0028.TEXT", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Текст\rf3Archive_eng-CAB-f17fe7d3fe4fd5ecb924dfa2cb195590-6531742534647681148.dat");
                //"C:\My files\Перевод игр\Rune Factory 3\попытка\Новая папка (4)\rf3Archive_eng-CAB-f17fe7d3fe4fd5ecb924dfa2cb195590-6531742534647681148.dat"

                //UnpackTextPseudoarchive(@"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\rf3Archive_fre-CAB-4bf83704f78373cd960c6168b9c8636c--3533345269708751530.dat", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\files fre");
                //UnpackTextFolder(@"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\files fre", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\text fre");

                //UnpackTextMonoBehaviour(@"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\ещё файлы\rf3TxtLoad_eng\rf3TxtLoad_eng-CAB-3e925015ee7bc956ec6f42853217459d-1618804152566204714.dat", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\ещё файлы\rf3TxtLoad_eng\rf3TxtLoad_eng.txt");
                //PackTextMonoBehaviour(@"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\ещё файлы\rf3TxtLoad_eng\rf3TxtLoad_eng.txt", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\ещё файлы\rf3TxtLoad_eng\rf3TxtLoad_eng-CAB-3e925015ee7bc956ec6f42853217459d-1618804152566204714 — копия.dat");
                //UnpackTextMonoBehaviourFolder(@"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\ещё файлы", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\ещё файлы текст");
                //PackTextMonoBehaviourFolder(@"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\вставка");

                //UnpackAssetBundle(@"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\ещё файлы рус\тест\4114e63e5fce669a6398fe9fcbe127d5.bundle");
                //PackAsset(@"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\ещё файлы рус\тест\Новая папка\b160e9f3c18c36b8d05ef55927a633fd.bundle", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\ещё файлы рус\тест\Новая папка\ANOTHER_CHOCOLAT_eng-CAB-4e30f0efc1601124b2e5889b0b83a843-66460860828543208.dat");

                //PackTextMonoBehaviourFolder(@"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\вставка");
                //PackAssetAll(@"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\ещё файлы рус", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\вставка");

                //PackTextFolder(@"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\вставка text rus\txt", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\вставка text rus\text");
                //PackTextPseudoarchiveFolder(@"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\вставка text rus\text", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\вставка text rus\rf3Archive_eng-CAB-f17fe7d3fe4fd5ecb924dfa2cb195590-6531742534647681148.dat");
                //PackAsset(@"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\вставка text rus\a9036f7aba00d24a5c91f74763979207.bundle", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\вставка text rus\rf3Archive_eng-CAB-f17fe7d3fe4fd5ecb924dfa2cb195590-6531742534647681148.dat");

                //UnpackMycf(@"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\диалоги\file 0022.MYCF", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\диалоги\file 0022_MYCF");
                //D:\Games\Rune Factory 3 Special\
                //CopyFiles(@"D:\Games\Rune Factory 3 Special\", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\диалоги\ends.txt", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\диалоги\files");
                //CopyFiles(@"D:\Games\Rune Factory 3 Special\", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\диалоги\ends 2.txt", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\диалоги\files 2");
                //ReadMycf(@"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\диалоги\file 0103.dat", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\диалоги\file 0103.txt");
                //ReadMycfFolder(@"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\диалоги\file 0022_MYCF", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\диалоги\file 0022_MYCF readed");

                //InsertTalker(@"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\text rus\file 0028.txt", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\диалоги\talkers вставить.txt", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\text rus\file 0028 with talker.txt");
                //InsertTalker(@"C:\My files\Перевод игр\Rune Factory 3\попытка\перевод 28\Сейчас перевожу\file 0028.txt", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\диалоги\talkers вставить.txt", @"C:\My files\Перевод игр\Rune Factory 3\попытка\перевод 28\Сейчас перевожу\file 0028 2.txt");
                //InsertTalker(@"C:\My files\Перевод игр\Rune Factory 3\попытка\перевод 28\анг\file 0028.txt", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\диалоги\talkers вставить.txt", @"C:\My files\Перевод игр\Rune Factory 3\попытка\перевод 28\анг\file 0028 talkers.txt");
                //ReadMycfFolderNew(@"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\диалоги\file 0022_MYCF", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\диалоги\file 0022_MYCF readed new");
                //InsertTalker(@"C:\My files\Перевод игр\Rune Factory 3\попытка\перевод 28\Сейчас перевожу\file 0028.txt", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\диалоги\talkers2.txt", @"C:\My files\Перевод игр\Rune Factory 3\попытка\перевод 28\Сейчас перевожу\file 0028 2.txt");


                //SearchSameText(@"C:\My files\Перевод игр\Rune Factory 3\попытка\перевод 28\анг\file 0028 talkers.txt", @"C:\My files\Перевод игр\Rune Factory 3\попытка\перевод 28\анг\file 0028 talkers same count.txt");
                //SearchSameText2(@"C:\My files\Перевод игр\Rune Factory 3\попытка\перевод 28\анг\file 0028 talkers.txt", @"C:\My files\Перевод игр\Rune Factory 3\попытка\перевод 28\анг\file 0028 talkers same count.txt", 27000);
                //PasteSameText(
                //    @"C:\My files\Перевод игр\Rune Factory 3\попытка\перевод 28\анг\file 0028 talkers.txt",
                //    @"C:\My files\Перевод игр\Rune Factory 3\попытка\перевод 28\анг\file 0028 talkers same count.txt",
                //    @"C:\My files\Перевод игр\Rune Factory 3\Инструментарий\Text\text rus\file 0028.txt",
                //    @"C:\My files\Перевод игр\Rune Factory 3\Инструментарий\Text\text rus\file 0028 pasted.txt"
                //);
                //PasteSameText(
                //    @"C:\My files\Перевод игр\Rune Factory 3\попытка\перевод 28\анг\file 0028 talkers.txt",
                //    @"C:\My files\Перевод игр\Rune Factory 3\попытка\перевод 28\анг\file 0028 talkers same count.txt",
                //    @"C:\My files\Перевод игр\Rune Factory 3\Инструментарий\Text\text rus\file 0028 pasted.txt",
                //    @"C:\My files\Перевод игр\Rune Factory 3\Инструментарий\Text\text rus\file 0028 pasted 2.txt"
                //);
                //PasteSameText(
                //    @"C:\My files\Перевод игр\Rune Factory 3\попытка\перевод 28\анг\file 0028 talkers.txt",
                //    @"C:\My files\Перевод игр\Rune Factory 3\попытка\перевод 28\анг\file 0028 talkers same count.txt",
                //    @"C:\My files\Перевод игр\Rune Factory 3\Инструментарий\Text\text rus\file 0028 pasted 2.txt",
                //    @"C:\My files\Перевод игр\Rune Factory 3\Инструментарий\Text\text rus\file 0028 pasted 3.txt"
                //);
                //PasteSameText(
                //    @"C:\My files\Перевод игр\Rune Factory 3\попытка\перевод 28\анг\file 0028 talkers.txt",
                //    @"C:\My files\Перевод игр\Rune Factory 3\попытка\перевод 28\анг\file 0028 talkers same count.txt",
                //    @"C:\My files\Перевод игр\Rune Factory 3\Инструментарий\Text\text rus\file 0028 pasted 3.txt",
                //    @"C:\My files\Перевод игр\Rune Factory 3\Инструментарий\Text\text rus\file 0028 pasted 4.txt"
                //);


                //CountWords(@"C:\My files\Перевод игр\Rune Factory 3\Инструментарий\Text\text rus\file 0028.txt", @"C:\My files\Перевод игр\Rune Factory 3\Инструментарий\Text\text rus\file 0028 words.txt");
                //SplitIntoLines(@"C:\My files\Перевод игр\Rune Factory 3\попытка\перевод 28\Сейчас перевожу\file 0028.txt", @"C:\My files\Перевод игр\Rune Factory 3\попытка\перевод 28\Сейчас перевожу\file 0028 newline.txt");
                //SplitIntoLines2(@"C:\My files\Перевод игр\Rune Factory 3\Инструментарий\Text\text rus\file 7723 newline.txt", @"C:\My files\Перевод игр\Rune Factory 3\Инструментарий\Text\text rus\file 7723 newline 2.txt");
                //SplitIntoLines2(@"C:\My files\Перевод игр\Rune Factory 3\Инструментарий\Text\text rus\file 7727 — копия.txt", @"C:\My files\Перевод игр\Rune Factory 3\Инструментарий\Text\text rus\file 7727 — копия 2.txt");
                //SplitIntoLines2(@"C:\My files\Перевод игр\Rune Factory 3\Инструментарий\Text\text rus\file 7732 — копия.txt", @"C:\My files\Перевод игр\Rune Factory 3\Инструментарий\Text\text rus\file 7732 — копия 2.txt");

                //PackTextFolder(@"C:\My files\Перевод игр\Rune Factory 3\Инструментарий\Text\text rus", @"C:\My files\Перевод игр\Rune Factory 3\Инструментарий\Files\text");
                //PackTextPseudoarchiveFolder(@"C:\My files\Перевод игр\Rune Factory 3\Инструментарий\Files\text", @"C:\My files\Перевод игр\Rune Factory 3\Инструментарий\Files\rf3Archive_eng-CAB-f17fe7d3fe4fd5ecb924dfa2cb195590-6531742534647681148.dat");
                //PackAsset(@"C:\My files\Перевод игр\Rune Factory 3\Инструментарий\Files\a9036f7aba00d24a5c91f74763979207.bundle", @"C:\My files\Перевод игр\Rune Factory 3\Инструментарий\Files\rf3Archive_eng-CAB-f17fe7d3fe4fd5ecb924dfa2cb195590-6531742534647681148.dat");

                //UnpackTextMonoBehaviour(@"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\настройки звука\SANTxtTitle_eng-CAB-d35093b5267cb21a45da49947fca606a-6367426497586585907.dat", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\настройки звука\SANTxtTitle_eng.txt");

                //ReadMycfFolderNew2(@"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\диалоги\file 0022_MYCF", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\диалоги\file 0022_MYCF readed new 2");

                //PackTextFolder(@"C:\My files\Перевод игр\Rune Factory 3\Инструментарий\Files\Temp\text rus", @"C:\My files\Перевод игр\Rune Factory 3\Инструментарий\Files\Temp\text");

                //UnpackTextMonoBehaviourFolder(@"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\ещё файлы", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\ещё файлы текст");

                //UnpackTextPseudoarchive(@"C:\My files\Перевод игр\Rune Factory 3\попытка\Switch\rf3archive\rf3Archive_eng-CAB-f17fe7d3fe4fd5ecb924dfa2cb195590-6531742534647681148.dat", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Switch\rf3archive\files switch");
                //UnpackTextFolder(@"C:\My files\Перевод игр\Rune Factory 3\попытка\Switch\rf3archive\files switch", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Switch\rf3archive\text switch");

                //string input = File.ReadAllText(@"C:\My files\Перевод игр\Rune Factory 3\попытка\Switch\file 0028.txt");
                //string pattern = @"[0-9]{5}=";
                //int count = 0;
                //string result = Regex.Replace(input, pattern, m => (count++).ToString("00000") + "=");
                //File.WriteAllText(@"C:\My files\Перевод игр\Rune Factory 3\попытка\Switch\file 0028.txt", result);

                //UnpackTextMonoBehaviourFolder(@"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\ещё файлы switch", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\ещё файлы текст switch");

                //PackTextMonoBehaviour(@"C:\My files\Перевод игр\Rune Factory 3\Инструментарий\Text\MonoBehaviour rus\ANOTHER_KARIN_eng-CAB-9d6ef15d42714d61eb665d6d137af039--3266736812989449515.txt", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\switch test\ANOTHER_KARIN_eng\ANOTHER_KARIN_eng-CAB-9d6ef15d42714d61eb665d6d137af039--3266736812989449515.dat");
                //PackAsset(@"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\switch test\ANOTHER_KARIN_eng\0ad69e7aa78a2e2e6b96cdea44a3a84d.bundle", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\switch test\ANOTHER_KARIN_eng\ANOTHER_KARIN_eng-CAB-9d6ef15d42714d61eb665d6d137af039--3266736812989449515.dat");

            }
        }
    }
}
