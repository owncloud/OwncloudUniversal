using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Devices.Power;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.System.Display;
using Windows.System.Power;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using OwncloudUniversal.Synchronization;
using OwncloudUniversal.Synchronization.LocalFileSystem;
using OwncloudUniversal.OwnCloud;
using OwncloudUniversal.Synchronization.Configuration;
using OwncloudUniversal.Synchronization.Processing;
using Template10.Mvvm;
using Windows.ApplicationModel.ExtendedExecution;

namespace OwncloudUniversal.Services
{
    class SynchronizationService
    {
        private BackgroundSyncProcess _worker;
        private ExtendedExecutionSession _executionSession;
        private PauseTokenSource _pauseTokenSource;

        private static SynchronizationService _instance;

        public static SynchronizationService GetInstance()
        {
            if (_instance == null)
                _instance = new SynchronizationService();
            return _instance;
        }

        private SynchronizationService()
        {
            _Initialize();
        }

        private void _Initialize()
        {
            var fileSystem = new FileSystemAdapter(false, null);
            var webDav = new WebDavAdapter(false, Configuration.ServerUrl, Configuration.Credential, fileSystem);
            fileSystem.LinkedAdapter = webDav;
            _worker = new BackgroundSyncProcess(fileSystem, webDav, false);
        }

        public async Task StartSyncProcess()
        {
            if (_pauseTokenSource?.IsPaused ?? false)
            {
                await ResumeAsync();
                return;
            }

            var run = true;
            if (!ChargerAndNetworkAvailable())
            {
                run = await GetWarningDialogResultAsync();
            }
            if (run)
            {
                await Task.Factory.StartNew(async () =>
                {
                    _executionSession = await RequestExtendedExecutionAsync();
                    _pauseTokenSource = new PauseTokenSource();
                    DisplayRequest displayRequest = null;
                    try
                    {
                        await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                            CoreDispatcherPriority.Normal, () =>
                            {
                                displayRequest = new DisplayRequest();
                                displayRequest.RequestActive();
                            });
                        _Initialize();
                        await _worker.Run(_pauseTokenSource);
                    }
                    catch (Exception e)
                    {
                        if (Windows.ApplicationModel.Core.CoreApplication.Views.Count > 0)
                        {
                            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                ExecutionContext.Instance.Status = ExecutionStatus.Error;
                            });
                        }
                        await LogHelper.Write(e.Message);
                        await LogHelper.Write(e.StackTrace);
                    }
                    finally
                    {
                        await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                            CoreDispatcherPriority.Normal, () =>
                            {
                                displayRequest?.RequestRelease();
                                ExecutionContext.Instance.IsPaused = false;
                            });
                        ClearExecutionSession(_executionSession);
                    }
                });
            }
        }

        private bool ChargerAndNetworkAvailable()
        {
            var result = true;
            var battery = Battery.AggregateBattery.GetReport();
            if (battery.Status == BatteryStatus.Discharging)
                result = false;
            var connectionCost = NetworkInformation.GetInternetConnectionProfile()?.GetConnectionCost();
            if (!(connectionCost?.NetworkCostType == NetworkCostType.Unknown || connectionCost?.NetworkCostType == NetworkCostType.Unrestricted))
            {
                result = false;
            }
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                result = false;
            }
            return result;
        }

        private async Task<bool> GetWarningDialogResultAsync()
        {
            var dialog = new ContentDialog
            {
                Content = App.ResourceLoader.GetString("SyncWarning"),
                PrimaryButtonText = App.ResourceLoader.GetString("yes"),
                SecondaryButtonText = App.ResourceLoader.GetString("no")
            };
            var result = await dialog.ShowAsync();
            return result == ContentDialogResult.Primary;            
        }

        private async Task ResumeAsync()
        {
            _pauseTokenSource.IsPaused = false;
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                        CoreDispatcherPriority.Normal, () =>
                        {
                            ExecutionContext.Instance.IsPaused = false;
                            ExecutionContext.Instance.Status = ExecutionStatus.Active;
                        });
            _executionSession = await RequestExtendedExecutionAsync();
            return;
        }

        private async Task<ExtendedExecutionSession> RequestExtendedExecutionAsync()
        {
            var session = new ExtendedExecutionSession();
            session.Reason = ExtendedExecutionReason.Unspecified;
            session.Revoked += SessionRevoked;
            ExtendedExecutionResult result = await session.RequestExtensionAsync();
            if (result == ExtendedExecutionResult.Allowed)
                return session;
            return null;
        }

        private void ClearExecutionSession(ExtendedExecutionSession session)
        {
            if (session != null)
            {
                session.Revoked -= SessionRevoked;
                session.Dispose();
            }
        }

        private async void SessionRevoked(object sender, ExtendedExecutionRevokedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine($"Extended execution session was revoked reaseon: {args.Reason}");
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ExecutionContext.Instance.IsPaused = true;
            });
            _pauseTokenSource.IsPaused = true;
            ToastHelper.SendToast(App.ResourceLoader.GetString("TransferCancelled"));
            ClearExecutionSession(_executionSession);
        }
    }
}
