using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using MahApps.Metro;
using PastySharpClient.ViewModels;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace PastySharpClient
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly KeyboardHook _pasteHotKey = new KeyboardHook();
        internal MainWindowViewModel _mainWindowViewModel = new MainWindowViewModel();
        private readonly NotifyIcon _notifyIcon = new NotifyIcon();

        private readonly List<PastyRequestWindow> _windows = new List<PastyRequestWindow>();

        public MainWindow()
        {
            var updateThread = new Thread(CheckForUpdate);
            updateThread.IsBackground = true;
            updateThread.Start();

            DataContext = _mainWindowViewModel;
            ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent("Green"), ThemeManager.GetAppTheme("BaseDark"));

            InitializeComponent();

            try
            {
                _pasteHotKey.RegisterHotKey(ModifierKeys.Control | ModifierKeys.Shift, Keys.V);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + "PastySharp already running?", "PastySharp");
                Environment.Exit(1);
            }
            _pasteHotKey.KeyPressed += OnPasteHotKeyPressed;


            _notifyIcon.MouseClick += (sender, args) =>
                                      {
                                          switch (Visibility)
                                          {
                                              case Visibility.Visible:
                                                  Hide();
                                                  break;
                                              case Visibility.Hidden:
                                                  Show();
                                                  WindowState = WindowState.Normal;
                                                  Focus();
                                                  Activate();
                                                  break;
                                          }
                                      };
            _notifyIcon.Icon = new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("PastySharpClient.Resources.pastysharp2.ico"));
            _notifyIcon.Text = "PastySharp";
            _notifyIcon.ContextMenu = new ContextMenu(new[] {new MenuItem("Exit", (sender, args) => Close())});
            _notifyIcon.Visible = true;

            Stats.StatsUrl = "http://pasty.batrick.de/stats";
            Stats.TrackAction(Stats.TrackActivity.AppStart);
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (Environment.GetCommandLineArgs().Contains("--start-minimized")) WindowState = WindowState.Minimized;
        }

        private void OnPasteHotKeyPressed(object sender, KeyPressedEventArgs keyPressedEventArgs)
        {
            var window = new PastyRequestWindow();
            window.Run();

            _windows.Add(window);

            Stats.TrackAction(Stats.TrackActivity.Paste);
        }

        private void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            foreach (var pastyRequestWindow in _windows)
            {
                pastyRequestWindow.Close();
            }
        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            Stats.TrackAction(Stats.TrackActivity.AppTerm);
            Settings.Save();

            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
        }

        private void MetroWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                Hide();
        }

        private void CheckForUpdate()
        {
            try
            {
                var newest = new WebClient().DownloadString("http://bagbag.github.io/pasty/psversion.txt");
                var current = Assembly.GetExecutingAssembly().GetName().Version.ToString();

                if (newest != current)
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (MessageBox.Show("Open browser?", "Update available",MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            Process.Start("https://github.com/bagbag/pasty/releases");
                        }
                    });
                }
            }
            catch
            {
                //do nothing
            }
        }
    }
}
