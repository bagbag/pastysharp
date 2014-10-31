using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using PastySharpClient.Annotations;

namespace PastySharp
{
    public class PastyRequest:INotifyPropertyChanged
    {
        private readonly string _server;
        private readonly int _port;
        private readonly PastyFileType _fileType;
        private readonly int _fileLength;
        private float _progress;

        public float Progress
        {
            get { return _progress; }
            set
            {
                if (value.Equals(_progress)) return;
                _progress = value;
                OnPropertyChanged();
            }
        }

        public PastyRequest(string server, int port, PastyFileType fileType, int fileLength)
        {
            _server = server;
            _port = port;
            _fileType = fileType;
            _fileLength = fileLength;
        }

        public PatyRequestUrlResponse RequestUrl()
        {
            var tcpClient = new TcpClient();
            tcpClient.NoDelay = true;

            var result = tcpClient.BeginConnect(_server, _port, null, null);

            result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(2.5));
            if (!tcpClient.Connected)
            {
                throw new Exception("Failed to connect.");
            }

            // we have connected
            tcpClient.EndConnect(result);

            string url;
            int statusCode;

            var tcpStream = tcpClient.GetStream();

            using (var binaryWriter = new BigEndianWriter(tcpStream, Encoding.UTF8, true))
            {
                binaryWriter.Write((byte) _fileType);
                binaryWriter.Write(_fileLength);
            }

            using (var binaryReader = new BigEndianReader(tcpStream, Encoding.UTF8, true))
            {
                statusCode = binaryReader.ReadByte();

                var urlLength = binaryReader.ReadUInt16();
                var urlBytes = binaryReader.ReadBytes(urlLength);
                url = Encoding.UTF8.GetString(urlBytes);
            }

            return new PatyRequestUrlResponse(tcpClient, tcpStream, url, statusCode);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="response"></param>
        /// <param name="fileName"></param>
        /// <param name="inputStream"></param>
        /// <param name="isNameEncrypted"></param>
        /// <param name="isDataEncrypted"></param>
        /// <returns>StatusCode</returns>
        public int SendData(PatyRequestUrlResponse response, string fileName, Stream inputStream, bool isNameEncrypted, bool isDataEncrypted)
        {
            var tcpClient = response._tcpClient;
            var tcpStream = response._tcpStream;
            int statusCode;

            try
            {
                using (var binaryWriter = new BigEndianWriter(tcpStream, Encoding.UTF8, true))
                {
                    binaryWriter.Write(isNameEncrypted);
                    binaryWriter.Write(isDataEncrypted);
                    binaryWriter.Write((short) fileName.Length);
                    binaryWriter.Write(Encoding.UTF8.GetBytes(fileName));
                }

                //inputStream.CopyTo(tcpStream);

                var buffer = new byte[1024];
                int totalBytesRead = 0;

                while (totalBytesRead < _fileLength)
                {
                    int bytesRead = inputStream.Read(buffer, 0, buffer.Length);
                    totalBytesRead += bytesRead;

                    tcpStream.Write(buffer, 0, bytesRead);

                    Progress = (float) totalBytesRead / (float) _fileLength;
                }

                tcpStream.Flush();

                while (!tcpStream.DataAvailable)
                    Thread.Sleep(10);

                using (var binaryReader = new BigEndianReader(tcpStream, Encoding.UTF8, true))
                {
                    statusCode = binaryReader.ReadByte();
                }
            }
            finally
            {
                tcpStream.Dispose();
                tcpClient.Close();
            }

            return statusCode;
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
