using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace PastySharpClient
{
    public class Settings
    {
        internal const long MAX_FILE_SIZE = 1024 * 1024 * 25;
        internal const string SERVER = "psty.de";
        internal const int PORT = 12345;

        private static Settings _instance;
        public static Settings Instance
        {
            get
            {
                if (_instance == null) CreateInstance();
                return _instance;
            }
        }

        private Helpers.ImageCodecs _imageCodec = Helpers.ImageCodecs.PNG;
        private bool _interlaced = true;
        private int _jpegQualityLevel = 75;
        private int _zipCompressLevel = 3;
        private bool _encryption = true;

        public Helpers.ImageCodecs ImageCodec { get { return _imageCodec; } set { _imageCodec = value; } }

        public bool Interlaced { get { return _interlaced; } set { _interlaced = value; } }

        public int JPEGQualityLevel { get { return _jpegQualityLevel; } set { _jpegQualityLevel = value > 100 ? 100 : (value < 1 ? 1 : value); } }

        public int ZipCompressLevel { get { return _zipCompressLevel; } set { _zipCompressLevel = value > 9 ? 9 : (value < 1 ? 1 : value); } }

        public bool Encryption { get { return _encryption; } set { _encryption = value; } }

        private static string ExecutablePath { get { return "\"" + Assembly.GetExecutingAssembly().Location + "\""; } }
        [XmlIgnore]
        public bool Autostart
        {
            get
            {
                var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);

                if (key == null)
                    return false;

                var value = key.GetValue("PastySharp", null);

                if (value == null)
                    return false;

                if (key.GetValueKind("PastySharp") != RegistryValueKind.String || (value as string) != ExecutablePath + " --start-minimized")
                    key.SetValue("PastySharp", ExecutablePath + " --start-minimized", RegistryValueKind.String);

                return true;
            }
            set
            {
                var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);

                if (key == null)
                    return;

                if (value)
                    key.SetValue("PastySharp", ExecutablePath + " --start-minimized", RegistryValueKind.String);
                else
                    key.DeleteValue("PastySharp", false);
            }
        }

        private static string SettingsPath { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PastySharp", "settings.xml"); } } 

        private static void CreateInstance()
        {
            try
            {
                if (!File.Exists(SettingsPath)) return;

                using (var fileStream = new FileStream(SettingsPath, FileMode.Open, FileAccess.Read))
                {
                    var serializer = new XmlSerializer(typeof (Settings));
                    _instance = serializer.Deserialize(fileStream) as Settings;
                }
            }
            catch
            {
                //just don't throw an error
            }
            finally
            {
                if (_instance == null) _instance = new Settings();
            }
        }

        public static void Save()
        {
            try
            {
                var dir = Path.GetDirectoryName(SettingsPath);
                if (string.IsNullOrWhiteSpace(dir)) return;

                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                using (var fileStream = new FileStream(SettingsPath, FileMode.Create, FileAccess.Write))
                {
                    var serializer = new XmlSerializer(typeof(Settings));
                    serializer.Serialize(fileStream, Instance);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
