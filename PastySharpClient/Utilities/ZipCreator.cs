using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using PastySharpClient.Annotations;

namespace PastySharpClient
{
    internal class ZipCreator : INotifyPropertyChanged
    {
        private float _progress;

        public float Progress
        {
            get { return _progress; }
            private set
            {
                _progress = value;
                OnPropertyChanged();
            }
        }

        internal void CreateZip(string baseDir, string[] files, string[] folders, Stream outputStream)
        {
            try
            {
                var baseDirLength = baseDir.EndsWith("\\") ? baseDir.Length : baseDir.Length + 1;

                var totalLength = files.Sum(s => new FileInfo(s).Length);
                totalLength += folders.Length;

                if (totalLength > Settings.MAX_FILE_SIZE)
                    throw new Exception(string.Format("data exceeds maximum size of {0}", Helpers.LengthToReadableString(Settings.MAX_FILE_SIZE)));

                long workDone = 0;

                using (var zipOutStream = new ZipOutputStream(outputStream))
                {
                    zipOutStream.IsStreamOwner = false;
                    zipOutStream.UseZip64 = UseZip64.Off;
                    zipOutStream.SetLevel(Settings.Instance.ZipCompressLevel);

                    foreach (var dir in folders)
                    {
                        var entry = new ZipEntry(dir.Substring(baseDirLength) + "\\");
                        zipOutStream.PutNextEntry(entry);
                        zipOutStream.CloseEntry();
                        workDone++;
                    }

                    foreach (var file in files)
                    {
                        var entry = new ZipEntry(file.Substring(baseDirLength));

                        using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                        {
                            entry.Size = fs.Length;
                            zipOutStream.PutNextEntry(entry);

                            var buffer = new byte[1024];
                            var totalBytesRead = 0;

                            while (totalBytesRead < fs.Length)
                            {
                                var bytesRead = fs.Read(buffer, 0, buffer.Length);
                                totalBytesRead += bytesRead;

                                zipOutStream.Write(buffer, 0, bytesRead);

                                workDone += bytesRead;
                                Progress = (float) workDone / (float) totalLength;
                            }
                        }

                        zipOutStream.CloseEntry();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
