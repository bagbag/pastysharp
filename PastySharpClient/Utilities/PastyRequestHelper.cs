using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using PastySharp;
using PastySharpClient.Annotations;

namespace PastySharpClient.Utilities
{
    internal class PastyRequestHelper : INotifyPropertyChanged
    {
        private float _progress;
        private bool _done;
        private Exception _exception;
        private string _state;

        internal Thread _workerThread;
        private string _filename;

        public float Progress
        {
            get { return _progress; }
            private set
            {
                if (Equals(value, _progress)) return;
                _progress = value;
                OnPropertyChanged();
            }
        }

        public string State
        {
            get { return _state; }
            set
            {
                if (value == _state) return;
                _state = value;
                OnPropertyChanged();
            }
        }

        public string Filename
        {
            get { return _filename; }
            set
            {
                if (value == _filename) return;
                _filename = value;
                OnPropertyChanged();
            }
        }

        public bool Done
        {
            get { return _done; }
            private set
            {
                if (Equals(value, _done)) return;
                _done = value;
                OnPropertyChanged();
            }
        }

        public Exception Exception
        {
            get { return _exception; }
            set
            {
                if (Equals(value, _exception)) return;
                _exception = value;
                Progress = 0;
                OnPropertyChanged();
            }
        }

        internal void DoYourThingAsync()
        {
            _workerThread = new Thread(() =>
                                       {
                                           try
                                           {
                                               Stream dataStream;
                                               int dataLength;
                                               string filename;
                                               PastyFileType pastyFileType;
                                               bool isEncrypted;
                                               string password;

                                               if (!PrepareData(out dataStream, out dataLength, out filename, out pastyFileType,out isEncrypted,out password)) throw new Exception("something's wrong");

                                               Filename = filename;

                                               if (dataLength > Settings.MAX_FILE_SIZE)
                                                   throw new Exception(string.Format("data exceeds maximum size of {0}", Helpers.LengthToReadableString(Settings.MAX_FILE_SIZE)));

                                               State = "Connecting to Server...";
                                               var pastyRequest = new PastyRequest(Settings.SERVER, Settings.PORT, pastyFileType, dataLength);

                                               var response = pastyRequest.RequestUrl();

                                               if (response.StatusCode != 0)
                                                   return;

                                               Console.WriteLine("{0}: {1}", response.StatusCode, response.Url);

                                               Helpers.WriteText(response.Url + (isEncrypted ? "#" + password : ""));

                                               dataStream.Seek(0, SeekOrigin.Begin);
                                               pastyRequest.PropertyChanged += (sender, args) =>
                                                                               {
                                                                                   if (args.PropertyName == "Progress")
                                                                                       Progress = pastyRequest.Progress;
                                                                               };
                                               State = "Uploading Data...";

                                               var statusCode = pastyRequest.SendData(response, filename, dataStream, false, isEncrypted);

                                               if (statusCode != 0)
                                                   throw new Exception("Server respond with " + statusCode);

                                               State = "Done";
                                               Done = true;
                                           }
                                           catch (Exception ex)
                                           {
                                               Exception = ex;
                                           }
                                       });
            _workerThread.SetApartmentState(ApartmentState.STA);
            _workerThread.IsBackground = true;
            _workerThread.Start();
        }

        private bool PrepareData(out Stream outStream, out int dataLength, out string filename, out PastyFileType pastyFileType, out bool isEncrypted, out string password)
        {
            outStream = null;
            filename = null;
            pastyFileType = PastyFileType.File;
            dataLength = 0;
            isEncrypted = false;
            password = null;

            if (Clipboard.ContainsImage())
            {
                State = "Getting Image from Clipboard";
                var image = Clipboard.GetImage();
                if (image == null) return false;

                var codec = Settings.Instance.ImageCodec;

                var field = codec.GetType().GetField(codec.ToString());
                var attribute = Attribute.GetCustomAttribute(field, typeof(Helpers.FileExtensionAttribute)) as Helpers.FileExtensionAttribute;
                filename = string.Format("Screenshot {0}.{1}", DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"), attribute.Value);
                Filename = filename;

                BitmapEncoder encoder;

                switch (codec)
                {
                    case Helpers.ImageCodecs.PNG:
                        encoder = new PngBitmapEncoder
                                  {
                                      Interlace = Settings.Instance.Interlaced ? PngInterlaceOption.On : PngInterlaceOption.Off
                                  };
                        break;
                    case Helpers.ImageCodecs.JPEG:
                        encoder = new JpegBitmapEncoder
                                  {
                                      QualityLevel = Settings.Instance.JPEGQualityLevel
                                  };
                        break;
                    default:

                        throw new ArgumentOutOfRangeException();
                }

                //encoder.Metadata.ApplicationName = "PastySharp";
                //encoder.Metadata.Author = new ReadOnlyCollection<string>(new[] { "PastySharp" });
                //encoder.Metadata.Comment = "Created with PastySharp";
                //encoder.Metadata.DateTaken = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH-mm-ss UTC");

                encoder.Frames.Add(BitmapFrame.Create(image));

                State = "Encoding Image";
                var ms = new MemoryStream();
                encoder.Save(ms);
                ms.Seek(0, SeekOrigin.Begin);
                outStream = ms;

            }
            else if (Clipboard.ContainsFileDropList())
            {
                var dropList = Clipboard.GetFileDropList();

                if (dropList.Count == 1 && File.Exists(dropList[0]))
                {
                    filename = Path.GetFileName(dropList[0]);
                    Filename = filename;
                    var fs = new FileStream(dropList[0], FileMode.Open, FileAccess.Read);

                    outStream = fs;
                }
                else
                {
                    string baseDir = dropList[0].Substring(0, dropList[0].LastIndexOf('\\'));
                    filename = Path.GetFileName(dropList.Count == 1 ? dropList[0] : baseDir) + ".zip";
                    Filename = filename;

                    var zipCreator = new ZipCreator();
                    zipCreator.PropertyChanged += (sender, args) =>
                                                  {
                                                      if (args.PropertyName == "Progress")
                                                          Progress = zipCreator.Progress;
                                                  };

                    var dropArray = new string[dropList.Count];
                    dropList.CopyTo(dropArray, 0);

                    var filesFolders = Helpers.GetAllFilesAndFolders(dropArray);

                    var ms = new MemoryStream();
                    State = "Creating zip";
                    zipCreator.CreateZip(baseDir, filesFolders.Key, filesFolders.Value, ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    outStream = ms;
                }
            }
            else if (Clipboard.ContainsText())
            {
                string text = Clipboard.GetText();

                Uri url;
                if (Uri.TryCreate(text, UriKind.Absolute, out url))
                {
                    //writer.WriteLine("<html>");
                    //writer.WriteLine("<head>");
                    //writer.WriteLine("<title>Pasty Redirect</title>");
                    //writer.WriteLine("</head>");
                    //writer.WriteLine("<body>");
                    //writer.WriteLine("<meta http-equiv=\"refresh\" content=\"3; URL={0}\">", text);
                    //writer.WriteLine("You are getting redirected to <a href=\"{0}\">{0}</a>  in 3 seconds.", text);
                    //writer.WriteLine("</body>");
                    //writer.Write("</html>");
                    //filename = "data.html";

                    pastyFileType = PastyFileType.Link;
                    filename = "link";
                }
                else
                {
                    filename = "text.txt";
                }

                var data = Encoding.UTF8.GetBytes(text);
                var ms = new MemoryStream();
                ms.Write(data, 0, data.Length);
                ms.Seek(0, SeekOrigin.Begin);
                outStream = ms;
            }

            if (Settings.Instance.Encryption)
            {
                var pastyCryptionStream = new PastyCryptionStream();
                password = pastyCryptionStream.Password;
                pastyCryptionStream.PropertyChanged += (sender, args) =>
                                                       {
                                                           if (args.PropertyName == "CopyProgress")
                                                               Progress = pastyCryptionStream.CopyProgress;
                                                       };
                State = "Encrypting...";
                pastyCryptionStream.CopyFromStream(outStream);
                outStream = pastyCryptionStream;
                isEncrypted = true;
            }

            dataLength = (int)outStream.Length;

            return true;
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
