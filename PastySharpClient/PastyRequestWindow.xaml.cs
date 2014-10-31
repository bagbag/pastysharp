using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using PastySharpClient.ViewModels;

namespace PastySharpClient
{
    /// <summary>
    /// Interaktionslogik für PastyRequestWindow.xaml
    /// </summary>
    public partial class PastyRequestWindow
    {
        private readonly PastyRequestWindowViewModel _viewModel = new PastyRequestWindowViewModel();

        public PastyRequestWindow()
        {
            DataContext = _viewModel;
            InitializeComponent();
            AllowsTransparency = true;
            _viewModel.PropertyChanged += ViewModelOnPropertyChanged;
        }

        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Done":
                    Task.Run(() =>
                             {
                                 Thread.Sleep(750);
                                 Dispatcher.Invoke(Close);
                             });
                    break;
                case "Exception":
                    Task.Run(() =>
                             {
                                 Thread.Sleep(2500);
                                 Dispatcher.Invoke(Close);
                             });
                    break;
            }
        }

        public void Run()
        {
            Show();
            _viewModel.DoYourThingAsync();
        }

        public new void Show()
        {
            Left = SystemParameters.WorkArea.Width - (Width + 5);
            Top = SystemParameters.WorkArea.Height;
            base.Show();

            var animation = new DoubleAnimation(SystemParameters.WorkArea.Width,
                                                SystemParameters.WorkArea.Height - (Height + 5),
                                                new Duration(TimeSpan.FromSeconds(1)));

            animation.EasingFunction = new CircleEase();

            BeginAnimation(TopProperty, animation);
        }

        public new void Close()
        {
            if (_viewModel._workerThread.ThreadState == ThreadState.Running)
                _viewModel._workerThread.Abort();
            if (_viewModel._workerThread.ThreadState == ThreadState.Running)
#pragma warning disable 618
                _viewModel._workerThread.Suspend();
#pragma warning restore 618

            var animation = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(0.5)));
            animation.Completed += (sender, args) => base.Close();
            BeginAnimation(OpacityProperty, animation);
        }

        private void MetroWindow_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
