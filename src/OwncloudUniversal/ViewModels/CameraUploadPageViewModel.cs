using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using OwncloudUniversal.Synchronization.Configuration;
using Template10.Mvvm;

namespace OwncloudUniversal.ViewModels
{
    class CameraUploadPageViewModel : ViewModelBase
    {
        private readonly InstantUploadRegistration _registration;

        public CameraUploadPageViewModel()
        {
            _registration = new InstantUploadRegistration();
        }

        public bool Enabled
        {
            get => _registration.Enabled;
            set
            {
                _registration.Enabled = value;
                RaisePropertyChanged();
            }
        }
    }
}
