using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PastySharpClient
{
    public static class Helpers
    {
        public enum ImageCodecs
        {
            [FileExtension("png")]
            PNG,
            [FileExtension("jpg")]
            JPEG
        }

        public sealed class FileExtensionAttribute : Attribute
        {
            private readonly string _value;

            public FileExtensionAttribute(string value)
            {
                _value = value;
            }

            public string Value
            {
                get { return _value; }
            }
        }

        static readonly string[] _sizes = { "B", "KB", "MB", "GB" };
        public static string LengthToReadableString(long length)
        {
            int order = 0;
            while (length >= 1024 && order + 1 < _sizes.Length)
            {
                order++;
                length = length / 1024;
            }

            return string.Format("{0:0.##} {1}", length, _sizes[order]);
        }

        internal static KeyValuePair<string[],string[]> GetAllFilesAndFolders(string[] filesFolders)
        {
            var files = new List<string>();
            var folders = new List<string>();

            foreach (var drop in filesFolders)
            {
                if (Directory.Exists(drop))
                {
                    folders.Add(drop);
                    folders.AddRange(Directory.GetDirectories(drop, "*", SearchOption.AllDirectories));
                    files.AddRange(Directory.GetFiles(drop, "*", SearchOption.AllDirectories));
                }
                else if (File.Exists(drop))
                {
                    files.Add(drop);
                }
            }

            return new KeyValuePair<string[], string[]>(files.ToArray(), folders.ToArray());
        }

        internal static void WriteText(string text)
        {
            var inputData = new INPUT[text.Length * 2];

            var counter = 0;
            foreach (var c in text)
            {
                var inputDown = new INPUT();
                inputDown.type = 1;
                inputDown.ki.wScan = (short)c;
                inputDown.ki.dwFlags = NativeMethods.KEYEVENTF_UNICODE;
                inputDown.ki.time = 0;
                inputDown.ki.dwExtraInfo = IntPtr.Zero;

                var inputUp = new INPUT();
                inputUp.type = 1;
                inputUp.ki.wScan = (short)c;
                inputUp.ki.dwFlags = NativeMethods.KEYEVENTF_KEYUP | NativeMethods.KEYEVENTF_UNICODE;
                inputUp.ki.time = 0;
                inputUp.ki.dwExtraInfo = IntPtr.Zero;

                inputData[counter++] = inputDown;
                inputData[counter++] = inputUp;
            }

            NativeMethods.SendInput((uint)inputData.Length, inputData, Marshal.SizeOf(typeof(INPUT)));
        }
    }
}
