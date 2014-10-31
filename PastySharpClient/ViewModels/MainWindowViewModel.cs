using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using PastySharpClient.Annotations;

namespace PastySharpClient.ViewModels
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        private Visibility _pngSettingsVisibility;
        private Visibility _jpegSettingsVisibility;

        public string[] ImageCodecs { get; private set; }

        public string ImageCodec
        {
            get
            {
                return Settings.Instance.ImageCodec.ToString();
            }
            set
            {
                Settings.Instance.ImageCodec = (Helpers.ImageCodecs)Enum.Parse(typeof(Helpers.ImageCodecs), value);

                switch (Settings.Instance.ImageCodec)
                {
                    case Helpers.ImageCodecs.PNG:
                        JPEGSettingsVisibility = Visibility.Collapsed;
                        PNGSettingsVisibility = Visibility.Visible;
                        break;
                    case Helpers.ImageCodecs.JPEG:
                        PNGSettingsVisibility = Visibility.Collapsed;
                        JPEGSettingsVisibility = Visibility.Visible;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public Visibility PNGSettingsVisibility
        {
            get { return _pngSettingsVisibility; }
            set
            {
                if (value == _pngSettingsVisibility) return;
                _pngSettingsVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility JPEGSettingsVisibility
        {
            get { return _jpegSettingsVisibility; }
            set
            {
                if (value == _jpegSettingsVisibility) return;
                _jpegSettingsVisibility = value;
                OnPropertyChanged();
            }
        }

        public bool Interlaced
        {
            get { return Settings.Instance.Interlaced; }
            set
            {
                if (value.Equals(Settings.Instance.Interlaced)) return;
                Settings.Instance.Interlaced = value;
                OnPropertyChanged();
            }
        }

        public double JPEGQuality
        {
            get { return Settings.Instance.JPEGQualityLevel; }
            set
            {
                if (value.Equals(Settings.Instance.JPEGQualityLevel)) return;
                Settings.Instance.JPEGQualityLevel = Convert.ToInt32(value);
                OnPropertyChanged();
            }
        }

        public int ZipCompressionLevel
        {
            get { return Settings.Instance.ZipCompressLevel; }
            set
            {
                if (value == Settings.Instance.ZipCompressLevel) return;
                Settings.Instance.ZipCompressLevel = value;
                OnPropertyChanged();
            }
        }

        public bool Encryption
        {
            get { return Settings.Instance.Encryption; }
            set
            {
                if (value.Equals(Settings.Instance.Encryption)) return;
                Settings.Instance.Encryption = value;
                OnPropertyChanged();
            }
        }

        public bool Autostart
        {
            get { return Settings.Instance.Autostart; }
            set
            {
                if (value.Equals(Settings.Instance.Autostart)) return;
                Settings.Instance.Autostart = value;
                OnPropertyChanged();
            }
        }

        public MainWindowViewModel()
        {
            ImageCodecs = Enum.GetNames(typeof (Helpers.ImageCodecs));
            ImageCodec = Settings.Instance.ImageCodec.ToString();
            ZipCompressionLevel = Settings.Instance.ZipCompressLevel;
            JPEGQuality = Settings.Instance.JPEGQualityLevel;
            Encryption = Settings.Instance.Encryption;
            Autostart = Settings.Instance.Autostart;
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
