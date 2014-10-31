using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using PastySharpClient.Annotations;
using PastySharpClient.Utilities;

namespace PastySharpClient.ViewModels
{
    class PastyRequestWindowViewModel : INotifyPropertyChanged
    {
        readonly PastyRequestHelper _helper = new PastyRequestHelper();

        private float _progress;
        private string _progressString;
        private string _state;
        private bool _done;
        private Exception _exception;
        private string _filename;

        internal Thread _workerThread { get { return _helper._workerThread; } }

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

        public string ProgressString
        {
            get { return _progressString; }
            private set
            {
                if (Equals(value, _progressString)) return;
                _progressString = value;
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
            set
            {
                if (value.Equals(_done)) return;
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
                OnPropertyChanged();
            }
        }

        public PastyRequestWindowViewModel()
        {
            _helper.PropertyChanged += _helper_PropertyChanged;
        }

        private void _helper_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Progress":
                    ProgressString = string.Format("{0:0.00} %", _helper.Progress * 100);
                    Progress = _helper.Progress;
                    break;
                case "State":
                    State = _helper.State;
                    break;
                case "Filename":
                    Filename = _helper.Filename;
                    break;
                case "Done":
                        Done = _helper.Done;
                    break;
                case "Exception":
                    State = _helper.Exception.Message;
                    Exception = _helper.Exception;
                    break;
            }
        }

        internal void DoYourThingAsync()
        {
            _helper.DoYourThingAsync();
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
