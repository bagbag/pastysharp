using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Encryption;
using PastySharpClient.Annotations;

namespace PastySharpClient
{
    internal class PastyCryptionStream : Stream, INotifyPropertyChanged
    {
        // ReSharper disable once InconsistentNaming
        private static readonly byte[] SALT = {0, 0, 0, 0, 0, 0, 0, 0};

        private readonly CryptoStream _aesStream;
        private readonly MemoryStream _outStream = new MemoryStream();
        private float _copyProgress;

        public float CopyProgress
        {
            get { return _copyProgress; }
            set
            {
                if (value.Equals(_copyProgress)) return;
                _copyProgress = value;
                OnPropertyChanged();
            }
        }

        public string Password { get; private set; }

        internal PastyCryptionStream()
        {
            var rng = new RNGCryptoServiceProvider();

            var randomBytes = new byte[15];
            rng.GetBytes(randomBytes);
            Password = Convert.ToBase64String(randomBytes).Replace("=", "s").Replace("+", "m").Replace("/", "q");

            var deriveBytes = new Rfc2898DeriveBytes(Password, SALT, 10);

            var key = deriveBytes.GetBytes(32);
            var iv = deriveBytes.GetBytes(16);

            var aes = Aes.Create();
            // ReSharper disable once PossibleNullReferenceException
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;

            _aesStream = new CryptoStream(_outStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
        }

        public void FlushFinalBlock()
        {
            _aesStream.FlushFinalBlock();
        }

        public void CopyFromStream(Stream inStream, bool flushFinalBlock = true)
        {
            var buffer = new byte[1024];
            var totalBytesRead = 0;

            while (totalBytesRead < inStream.Length)
            {
                int bytesRead = inStream.Read(buffer, 0, buffer.Length);
                totalBytesRead += bytesRead;

                _aesStream.Write(buffer, 0, bytesRead);

                CopyProgress = (float)totalBytesRead / (float)inStream.Length;
            }

            _aesStream.Flush();

            if (flushFinalBlock)
                _aesStream.FlushFinalBlock();

            _outStream.Flush();
        }

        public override void Flush()
        {
            _aesStream.Flush();
            _outStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _outStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _outStream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_aesStream.HasFlushedFinalBlock) throw new Exception("Already flushed final block");
            _aesStream.Write(buffer, offset, count);
        }

        public override bool CanRead { get { return true; } }

        public override bool CanSeek { get { return true; } }

        public override bool CanWrite { get { return !_aesStream.HasFlushedFinalBlock; } }

        public override long Length { get { return _outStream.Length; } }

        public override long Position { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
        public event PropertyChangedEventHandler PropertyChanged;


        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
