using System.IO;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

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
                string name = item[(item.LastIndexOf("\\") + 1)..item.LastIndexOf(".")];
                string path = item[..item.LastIndexOf("\\")];

                UnpackText(item, $@"{outputFolder}\{name}.txt");
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


        static void Main(string[] args)
        {
            //UnpackTextPseudoarchive(@"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\rf3Archive_eng-CAB-f17fe7d3fe4fd5ecb924dfa2cb195590-6531742534647681148.dat", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Архив с текстом\files");
            //UnpackText(@"C:\My files\Перевод игр\Rune Factory 3\попытка\Новая папка (3)\file0028.TEXT", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Новая папка (3)\file0028.txt");

            //PackText(@"C:\My files\Перевод игр\Rune Factory 3\попытка\Новая папка (3)\file0028.txt", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Новая папка (3)\file0028 1.TEXT");
            //PackTextPseudoarchive(@"C:\My files\Перевод игр\Rune Factory 3\попытка\Новая папка (4)\file 0028.TEXT", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Новая папка (4)\rf3Archive_eng-CAB-f17fe7d3fe4fd5ecb924dfa2cb195590-6531742534647681148.dat");

            //UnpackTextFolder(@"C:\My files\Перевод игр\Rune Factory 3\попытка\Новая папка (3)", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Новая папка (3) 1");


            //PackText(@"C:\My files\Перевод игр\Rune Factory 3\попытка\Новая папка (3) 1\file 0028.txt", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Новая папка (4)\file 0028.TEXT");
            //PackTextPseudoarchive(@"C:\My files\Перевод игр\Rune Factory 3\попытка\Новая папка (4)\file 0028.TEXT", @"C:\My files\Перевод игр\Rune Factory 3\попытка\Новая папка (4)\rf3Archive_eng-CAB-f17fe7d3fe4fd5ecb924dfa2cb195590-6531742534647681148.dat");
            //"C:\My files\Перевод игр\Rune Factory 3\попытка\Новая папка (4)\rf3Archive_eng-CAB-f17fe7d3fe4fd5ecb924dfa2cb195590-6531742534647681148.dat"
        }
    }
}
