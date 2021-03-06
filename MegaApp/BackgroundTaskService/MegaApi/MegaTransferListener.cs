﻿using System;
using System.Threading;
using System.Threading.Tasks;
using BackgroundTaskService.Services;
using mega;

namespace BackgroundTaskService.MegaApi
{
    internal class MegaTransferListener : MTransferListenerInterface
    {
        // Helper Timer
        private Timer _timer;
        // Event raised so that the task agent can abort itself when storage quota is exceeded
        public event EventHandler StorageQuotaExceeded;
        // Event raised so that the task agent can abort itself when transfer quota is exceeded
        public event EventHandler TransferQuotaExceeded;

        private TaskCompletionSource<string> _tcs;
        private string _dateSetting;

        public async Task<string> ExecuteAsync(Action action, string dateSetting)
        {
            _dateSetting = dateSetting;
            _tcs = new TaskCompletionSource<string>();

            action.Invoke();

            return await _tcs.Task;
        }

        protected virtual void OnStorageQuotaExceeded(EventArgs e)
        {
            StorageQuotaExceeded?.Invoke(this, e);
        }

        protected virtual void OnTransferQuotaExceeded(EventArgs e)
        {
            TransferQuotaExceeded?.Invoke(this, e);
        }

        public bool onTransferData(MegaSDK api, MTransfer transfer, byte[] data)
        {
            return false;
        }

        public async void onTransferFinish(MegaSDK api, MTransfer transfer, MError e)
        {
            _timer?.Dispose();

            //Storage overquota error
            if (e.getErrorCode() == MErrorType.API_EGOINGOVERQUOTA || e.getErrorCode() == MErrorType.API_EOVERQUOTA)
            {
                //Stop the Camera Upload Service
                LogService.Log(MLogLevel.LOG_LEVEL_INFO, 
                    string.Format("Storage quota exceeded ({0}) - Disabling CAMERA UPLOADS service", e.getErrorCode().ToString()));
                OnStorageQuotaExceeded(EventArgs.Empty);
                _tcs.TrySetResult(e.getErrorString());
                return;
            }

            try
            {
                if (e.getErrorCode() == MErrorType.API_OK)
                {
                    ulong mtime = api.getNodeByHandle(transfer.getNodeHandle()).getModificationTime();
                    var fileDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(Convert.ToDouble(mtime));
                    await SettingsService.SaveSettingToFileAsync(_dateSetting, fileDate.ToLocalTime());

                    _tcs.TrySetResult(null);
                }
                else
                {
                    // An error occured. Log and process it.
                    switch (e.getErrorCode())
                    {
                        case MErrorType.API_EFAILED:
                        case MErrorType.API_EEXIST:
                        case MErrorType.API_EARGS:
                        case MErrorType.API_EREAD:
                        case MErrorType.API_EWRITE:
                            {
                                // Error will be loggend by caller
                                _tcs.TrySetResult(e.getErrorString());
                                break;
                            }
                    }
                    _tcs.TrySetResult(null);
                }
            }
            catch (Exception ex)
            {
                _tcs.TrySetResult(ex.Message);
                // Setting could not be saved. Just continue the run
            }
        }

        public void onTransferStart(MegaSDK api, MTransfer transfer)
        {
            _timer = new Timer(state =>
            {
                api.retryPendingConnections();
            }, null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(5));
        }

        public void onTransferTemporaryError(MegaSDK api, MTransfer transfer, MError e)
        {
            // Transfer overquota error
            if (e.getErrorCode() == MErrorType.API_EOVERQUOTA)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_INFO, "Transfer quota exceeded (API_EOVERQUOTA)");
                OnTransferQuotaExceeded(EventArgs.Empty);
                _tcs.TrySetResult(e.getErrorString());
            }
        }

        public void onTransferUpdate(MegaSDK api, MTransfer transfer)
        {
            // Do nothing
        }
    }
}
