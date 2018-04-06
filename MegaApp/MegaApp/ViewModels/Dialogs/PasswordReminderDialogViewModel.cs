﻿using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using MegaApp.Classes;
using MegaApp.MegaApi;
using MegaApp.Services;

namespace MegaApp.ViewModels.Dialogs
{
    public class PasswordReminderDialogViewModel : BaseContentDialogViewModel
    {
        public PasswordReminderDialogViewModel() : base(true)
        {
            this.TitleText = ResourceService.AppMessages.GetString("AM_PasswordReminder_Title");
            this.DescriptionText = ResourceService.AppMessages.GetString("AM_PasswordReminder");

            this.CheckPasswordCommand = new RelayCommand(CheckPassword);
            this.SaveKeyButtonCommand = new RelayCommand(SaveKey);
            this.TestPasswordButtonCommand = new RelayCommand(TestPassword);

            this.DialogStyle = (Style)Application.Current.Resources["MegaAlertDialogStyle"];

            this.IsTestPasswordSelected = false;

            this.SecondaryButtonCommand = this.TestPasswordButtonCommand;
            this.SecondaryButtonState = true;
            this.SecondaryButtonText = this.TestPasswordText;
        }

        #region Commands

        /// <summary>
        /// Command invoked to check the password typed by the user
        /// </summary>
        public ICommand CheckPasswordCommand { get; }

        /// <summary>
        /// Command invoked when the user select the "Backup Recovery key" option
        /// </summary>
        public ICommand SaveKeyButtonCommand { get; }

        /// <summary>
        /// Command invoked when the user select the "Test Password" option
        /// </summary>
        public ICommand TestPasswordButtonCommand { get; }

        #endregion

        #region Events

        /// <summary>
        /// Event invocator method called when the user closes the dialog using 
        /// the close button of the top-right corner of the dialog.
        /// </summary>
        protected override void OnCloseDialog()
        {
            // If user has checked the "Don't show me again" box
            if (!this.IsTestPasswordSelected && this.DoNotShowAgain)
            {
                SdkService.MegaSdk.passwordReminderDialogBlocked();
                if (this.AtLogout)
                    SdkService.MegaSdk.logout(new LogOutRequestListener());
            }
            // If the user has checked the password successfully
            else if (this.passwordChecked)
            {
                SdkService.MegaSdk.passwordReminderDialogSucceeded();
                if (this.AtLogout)
                    SdkService.MegaSdk.logout(new LogOutRequestListener());
            }
            else
            {
                SdkService.MegaSdk.passwordReminderDialogSkipped();

                // Only log out if the user has saved the recovery key
                if (this.AtLogout && this.recoveryKeySaved)
                    SdkService.MegaSdk.logout(new LogOutRequestListener());
            }

            base.OnCloseDialog();
        }
        
        #endregion

        #region Methods

        /// <summary>
        /// Check the password typed by the user
        /// </summary>
        private void CheckPassword()
        {
            if (string.IsNullOrWhiteSpace(this.Password)) return;

            this.ControlState = false;
            this.SecondaryButtonState = false;

            this.passwordChecked = SdkService.MegaSdk.checkPassword(this.Password);
            if (!this.passwordChecked)
            {
                this.ControlState = true;
                this.WarningColor = (SolidColorBrush)Application.Current.Resources["MegaRedColorBrush"];
                this.WarningText = ResourceService.AppMessages.GetString("AM_TestPasswordWarning");
                return;
            }

            if(!this.AtLogout)
                this.DialogStyle = (Style)Application.Current.Resources["MegaContentDialogStyle"];

            this.HasCloseButton = false;
            this.WarningColor = new SolidColorBrush(UiService.GetColorFromHex("#00C0A5"));
            this.WarningText = ResourceService.AppMessages.GetString("AM_TestPasswordSuccess");
            this.SecondaryButtonCommand = this.CloseCommand;
            this.SecondaryButtonState = true;

            this.SecondaryButtonText = this.AtLogout ? 
                ResourceService.UiResources.GetString("UI_Logout") :
                ResourceService.UiResources.GetString("UI_Close");
        }
        
        /// <summary>
        /// Backup the Recovery key
        /// </summary>
        private async void SaveKey()
        {
            var saveKeyCommand = SettingsService.RecoveryKeySetting.SaveKeyCommand as RelayCommandAsync<bool>;
            if (saveKeyCommand == null) return;

            if (!saveKeyCommand.CanExecute(null)) return;
            this.recoveryKeySaved = await saveKeyCommand.ExecuteAsync(null);
            if (!this.recoveryKeySaved) return;

            // If the recovery key has been successfully saved close the dialog
            if (!this.CloseCommand.CanExecute(null)) return;
            this.CloseCommand.Execute(null);
        }

        /// <summary>
        /// Set the test password button state.
        /// </summary>
        private void SetTestPasswordButtonState()
        {
            var enabled = !string.IsNullOrWhiteSpace(this.Password);
            OnUiThread(() => this.SecondaryButtonState = enabled);
        }

        /// <summary>
        /// Change the UI of the dialog to allow the user check the password
        /// </summary>
        private void TestPassword()
        {
            this.DoNotShowAgain = false;
            this.IsTestPasswordSelected = true;
            this.TitleText = ResourceService.AppMessages.GetString("AM_TestPassword_Title");
            this.DescriptionText = ResourceService.AppMessages.GetString("AM_TestPassword");
            this.SecondaryButtonCommand = this.CheckPasswordCommand;
            SetTestPasswordButtonState();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Indicates if the dialog is being displayed in a log out scenario
        /// </summary>
        public bool AtLogout;

        /// <summary>
        /// Indicates if the user has checked the password successfully
        /// </summary>
        private bool passwordChecked = false;

        /// <summary>
        /// Indicates if the user has saved the recovery key successfully
        /// </summary>
        private bool recoveryKeySaved = false;

        private string _titleText;
        /// <summary>
        /// Title of the dialog
        /// </summary>
        public string TitleText
        {
            get { return _titleText; }
            set { SetField(ref _titleText, value); }
        }

        private string _descriptionText;
        /// <summary>
        /// Description of the dialog
        /// </summary>
        public string DescriptionText
        {
            get { return _descriptionText; }
            set { SetField(ref _descriptionText, value); }
        }

        private Style _dialogStyle;
        /// <summary>
        /// Style of the dialog
        /// </summary>
        public Style DialogStyle
        {
            get { return _dialogStyle; }
            set { SetField(ref _dialogStyle, value); }
        }

        private bool _doNotShowAgain;
        /// <summary>
        /// Store the "Don't show me again" checkbox value
        /// </summary>
        public bool DoNotShowAgain
        {
            get { return _doNotShowAgain; }
            set { SetField(ref _doNotShowAgain, value); }
        }

        /// <summary>
        /// Flag to store if the user has selected the "Test Password" option
        /// </summary>
        private bool _isTestPasswordSelected;
        public bool IsTestPasswordSelected
        {
            get { return _isTestPasswordSelected; }
            set { SetField(ref _isTestPasswordSelected, value); }
        }

        private string _password;
        /// <summary>
        /// Store the password typed by the user
        /// </summary>
        public string Password
        {
            get { return _password; }
            set
            {
                SetField(ref _password, value);
                SetTestPasswordButtonState();
            }
        }

        private ICommand _secondaryButtonCommand;
        /// <summary>
        /// Command of the secondary button
        /// </summary>
        public ICommand SecondaryButtonCommand
        {
            get { return _secondaryButtonCommand; }
            set { SetField(ref _secondaryButtonCommand, value); }
        }

        private bool _secondaryButtonState;
        /// <summary>
        /// State (enabled/disabled) of the secondary button
        /// </summary>
        public bool SecondaryButtonState
        {
            get { return _secondaryButtonState; }
            set { SetField(ref _secondaryButtonState, value); }
        }

        private string _secondaryButtonText;
        /// <summary>
        /// Label of the secondary button
        /// </summary>
        public string SecondaryButtonText
        {
            get { return _secondaryButtonText; }
            set { SetField(ref _secondaryButtonText, value); }
        }

        private SolidColorBrush _warningColor;
        /// <summary>
        /// Color of the warning message (succeeded/failed)
        /// </summary>
        public SolidColorBrush WarningColor
        {
            get { return _warningColor; }
            set { SetField(ref _warningColor, value); }
        }

        private string _warningText;
        /// <summary>
        /// Warning message (succeeded/failed)
        /// </summary>
        public string WarningText
        {
            get { return _warningText; }
            set { SetField(ref _warningText, value); }
        }

        #endregion

        #region UiResources

        public string BackupRecoveryKeyText => ResourceService.UiResources.GetString("UI_BackupRecoveryKey");
        public string CurrentPasswordText => ResourceService.UiResources.GetString("UI_CurrentPassword");
        public string DoNotShowMeAgainText => ResourceService.UiResources.GetString("UI_DoNotShowMeAgain");
        public string TestPasswordText => ResourceService.UiResources.GetString("UI_TestPassword");

        #endregion
    }
}
