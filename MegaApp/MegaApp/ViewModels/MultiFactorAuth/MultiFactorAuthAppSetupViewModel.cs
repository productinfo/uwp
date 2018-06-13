﻿using System;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Services;
using MegaApp.Views;

namespace MegaApp.ViewModels.MultiFactorAuth
{
    public class MultiFactorAuthAppSetupViewModel : BasePageViewModel
    {
        public MultiFactorAuthAppSetupViewModel()
        {
            this.CopySeedCommand = new RelayCommand(this.CopySeed);
            this.VerifyCommand = new RelayCommand(this.Verify);

            this.Initialize();
        }

        #region Commands

        public ICommand CopySeedCommand { get; }
        public ICommand VerifyCommand { get; }

        #endregion

        private async void Initialize()
        {
            var multiFactorAuthGetCode = new MultiFactorAuthGetCodeRequestListenerAsync();
            this.MultiFactorAuthCode = await multiFactorAuthGetCode.ExecuteAsync(() =>
                SdkService.MegaSdk.multiFactorAuthGetCode(multiFactorAuthGetCode));
        }

        private async void CopySeed()
        {
            try
            {
                var data = new DataPackage();
                data.SetText(this.MultiFactorAuthCode);

                Clipboard.SetContent(data);

                ToastService.ShowTextNotification(
                    ResourceService.AppMessages.GetString("AM_MFA_SeedCopied_Title"),
                    ResourceService.AppMessages.GetString("AM_MFA_SeedCopied"));
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, e.Message, e);
                await DialogService.ShowAlertAsync(
                    ResourceService.AppMessages.GetString("AM_MFA_SeedCopiedFailed_Title"),
                    ResourceService.AppMessages.GetString("AM_MFA_SeedCopiedFailed"));
            }
        }

        private async void Verify()
        {
            if (string.IsNullOrWhiteSpace(this.VerifyCode)) return;

            var enableMultiFactorAuth = new MultiFactorAuthEnableRequestListenerAsync();
            var result = await enableMultiFactorAuth.ExecuteAsync(() =>
                SdkService.MegaSdk.multiFactorAuthEnable(this.VerifyCode, enableMultiFactorAuth));

            if (!result) return;

            DialogService.ShowMultiFactorAuthEnabledDialog();

            NavigateService.Instance.Navigate(typeof(SettingsPage), false,
                NavigationObject.Create(typeof(MultiFactorAuthAppSetupViewModel),
                NavigationActionType.SecuritySettings));
        }

        #region Properties

        private string _multiFactorAuthCode;
        /// <summary>
        /// Code or seed needed to enable the Multi-Factor Authentication
        /// </summary>
        public string MultiFactorAuthCode
        {
            get { return _multiFactorAuthCode; }
            set { SetField(ref _multiFactorAuthCode, value); }
        }

        private string _verifyCode;
        /// <summary>
        /// Code typed by the user to verify that the Multi-Factor Authentication is working
        /// </summary>
        public string VerifyCode
        {
            get { return _verifyCode; }
            set { SetField(ref _verifyCode, value); }
        }

        #endregion

        #region UiResources

        public string CopySeedText => ResourceService.UiResources.GetString("UI_CopySeed");
        public string ManuallySetupStep1Text => ResourceService.UiResources.GetString("UI_MFA_ManuallySetupStep1");
        public string ManuallySetupStep2Text => ResourceService.UiResources.GetString("UI_MFA_ManuallySetupStep2");
        public string ManuallySetupStep2DescriptionText => ResourceService.UiResources.GetString("UI_MFA_ManuallySetupStep2_Description");
        public string SectionNameText => ResourceService.UiResources.GetString("UI_SecuritySettings");
        public string SixDigitCodeText => ResourceService.UiResources.GetString("UI_SixDigitCode");
        public string TwoFactorAuthText => ResourceService.UiResources.GetString("UI_TwoFactorAuth");
        public string VerifyText => ResourceService.UiResources.GetString("UI_Verify");

        #endregion
    }
}
