﻿using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using mega;
using MegaApp.Classes;
using MegaApp.MegaApi;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    /// <summary>
    /// Viewmodel to display transfers in a list
    /// </summary>
    public class TransferListViewModel: BaseSdkViewModel
    {
        public TransferListViewModel(MTransferType type) : base(SdkService.MegaSdk)
        {
            this.Type = type;
            this.ItemCollection = new CollectionViewModel<TransferObjectModel>(this.MegaSdk);

            switch (this.Type)
            {
                case MTransferType.TYPE_DOWNLOAD:
                    this.Description = ResourceService.UiResources.GetString("UI_Downloads");
                    this.EmptyStateHeaderText = ResourceService.EmptyStates.GetString("ES_DownloadsHeader"); ;
                    this.EmptyStateSubHeaderText = ResourceService.EmptyStates.GetString("ES_DownloadsSubHeader");
                    this.CancelTransfersTitleText = ResourceService.UiResources.GetString("UI_CancelDownloads");
                    this.CancelTransfersDescriptionText = ResourceService.AppMessages.GetString("AM_CancelDownloadsQuestion");
                    this.ItemCollection.Items = TransferService.MegaTransfers.Downloads;
                    break;

                case MTransferType.TYPE_UPLOAD:
                    this.Description = ResourceService.UiResources.GetString("UI_Uploads");
                    this.EmptyStateHeaderText = ResourceService.EmptyStates.GetString("ES_UploadsHeader"); ;
                    this.EmptyStateSubHeaderText = ResourceService.EmptyStates.GetString("ES_UploadsSubHeader");
                    this.CancelTransfersTitleText = ResourceService.UiResources.GetString("UI_CancelUploads");
                    this.CancelTransfersDescriptionText = ResourceService.AppMessages.GetString("AM_CancelUploadsQuestion");
                    this.ItemCollection.Items = TransferService.MegaTransfers.Uploads;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            this.IsCompletedTransfersList = false;
            this.PauseOrResumeCommand = new RelayCommand(PauseOrResumeTransfers);
            this.CancelCommand = new RelayCommand(CancelTransfers);
            this.CleanCommand = new RelayCommand<bool>(UpdateTransfers);

            this.ItemCollection.Items.CollectionChanged += ItemsOnCollectionChanged;
        }

        public TransferListViewModel() : base(SdkService.MegaSdk)
        {
            this.ItemCollection = new CollectionViewModel<TransferObjectModel>(this.MegaSdk);

            this.Description = ResourceService.UiResources.GetString("UI_Completed");
            this.EmptyStateHeaderText = ResourceService.EmptyStates.GetString("ES_CompletedTransfersHeader"); ;
            this.EmptyStateSubHeaderText = ResourceService.EmptyStates.GetString("ES_CompletedTransfersSubHeader");
            this.IsCompletedTransfersList = true;
            this.ItemCollection.Items = TransferService.MegaTransfers.Completed;
            this.CleanCommand = new RelayCommand(CleanCompletedTransfers);

            this.ItemCollection.Items.CollectionChanged += ItemsOnCollectionChanged;
        }

        private void ItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("IsEmpty");
        }

        public async void PauseOrResumeTransfers()
        {
            await PauseOrResumeTransfersAsync();
        }

        public void UpdateTransfers(bool cleanTransfers = false)
        {
            TransferService.UpdateMegaTransferList(TransferService.MegaTransfers, this.Type, cleanTransfers);
        }

        private void CleanCompletedTransfers()
        {
            TransferService.MegaTransfers.Completed.Clear();
        }

        private async Task PauseOrResumeTransfersAsync()
        {
            var playPauseStatus = !AreTransfersPaused;
            
            var pauseTransfers = new PauseTransfersRequestListenerAsync();
            var result = await pauseTransfers.ExecuteAsync(() =>
            {
                this.MegaSdk.pauseTransfersDirection(playPauseStatus,
                    (int)this.Type, pauseTransfers);
            });

            if (!result) return;

            OnPropertyChanged("AreTransfersPaused");
        }

        /// <summary>
        /// Cancel all transfers of the current type.        
        /// </summary>
        public async void CancelTransfers()
        {
            var result = await DialogService.ShowOkCancelAsync(
                this.CancelTransfersTitleText,
                this.CancelTransfersDescriptionText);

            if (!result) return;

            // Use a temp list to avoid InvalidOperationException
            var transfers = ItemCollection.Items.ToList();
            foreach (var transfer in transfers)
            {
                // If the transfer is an upload and is being prepared (copying file to the upload temporary folder)
                if (this.Type == MTransferType.TYPE_UPLOAD && transfer?.PreparingUploadCancelToken != null)
                {
                    transfer.PreparingUploadCancelToken.Cancel();
                }
                // If the transfer is ready but not started for some reason
                else if (transfer?.IsBusy == false && transfer?.TransferState == MTransferState.STATE_NONE)
                {
                    LogService.Log(MLogLevel.LOG_LEVEL_INFO, string.Format("Transfer ({0}) canceled: {1}",
                        this.Type == MTransferType.TYPE_UPLOAD? "UPLOAD" : "DOWNLOAD", transfer.DisplayName));                    
                    transfer.TransferState = MTransferState.STATE_CANCELLED;
                }
            }

            this.MegaSdk.cancelTransfers((int)this.Type);
        }

        public override void UpdateNetworkStatus()
        {
            base.UpdateNetworkStatus();
            foreach (var item in this.ItemCollection.Items)
                item.UpdateNetworkStatus();
        }

        #region Commands

        public ICommand PauseOrResumeCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand CleanCommand { get; }

        #endregion

        #region Properties

        public string Description { get; }

        public string EmptyStateHeaderText { get; }
        public string EmptyStateSubHeaderText { get; }

        public MTransferType Type { get; set; }

        private CollectionViewModel<TransferObjectModel> _itemCollection;
        public CollectionViewModel<TransferObjectModel> ItemCollection
        {
            get { return _itemCollection; }
            set { SetField(ref _itemCollection, value); }
        }

        public bool IsEmpty => (ItemCollection.Items.Count == 0);

        private bool _isCompletedTransfersList;
        public bool IsCompletedTransfersList
        {
            get { return _isCompletedTransfersList; }
            set { SetField(ref _isCompletedTransfersList, value); }
        }

        public bool AreTransfersPaused => this.IsCompletedTransfersList ? 
            false : this.MegaSdk.areTransfersPaused((int)this.Type);

        #endregion

        #region Ui_Resources

        public string PauseAllText => ResourceService.UiResources.GetString("UI_PauseAll");
        public string CancelAllText => ResourceService.UiResources.GetString("UI_CancelAll");
        public string ResumeAllText => ResourceService.UiResources.GetString("UI_ResumeAll");
        public string ClearAllText => ResourceService.UiResources.GetString("UI_ClearAll");
        public string CancelTransfersTitleText { get; }
        public string CancelTransfersDescriptionText { get; }

        #endregion

        #region VisualResources

        public string CancelPathData => ResourceService.VisualResources.GetString("VR_CancelPathData");
        public string PausePathData => ResourceService.VisualResources.GetString("VR_PausePathData");
        public string ResumePathData => ResourceService.VisualResources.GetString("VR_PlayPathData");
        public string CleanPathData => ResourceService.VisualResources.GetString("VR_RubbishBinPathData");

        #endregion
    }
}
