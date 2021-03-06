﻿using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Extensions;
using MegaApp.UserControls;
using MegaApp.ViewModels;
using MegaApp.ViewModels.Contacts;
using MegaApp.ViewModels.Dialogs;
using MegaApp.ViewModels.SharedFolders;
using MegaApp.Views;
using MegaApp.Views.Dialogs;

namespace MegaApp.Services
{
    /// <summary>
    /// Service to display Dialogs to the user
    /// </summary>
    internal static class DialogService
    {
        #region Properties

        /// <summary>
        /// Storage the instance of the <see cref="AwaitEmailConfirmationDialog"/>
        /// </summary>
        private static AwaitEmailConfirmationDialog awaitEmailConfirmationDialog;

        /// <summary>
        /// Instance of the input dialog displayed
        /// </summary>
        private static InputDialog InputDialogInstance;

        /// <summary>
        /// Instance of the MFA code input dialog displayed
        /// </summary>
        private static MultiFactorAuthCodeInputDialog MultiFactorAuthCodeInputDialogInstance;

        #endregion

        #region Methods

        /// <summary>
        /// Check if there is any dialog visible
        /// </summary>
        /// <returns>TRUE if there is any dialog visible or FALSE in other case</returns>
        public static bool IsAnyDialogVisible()
        {
            var popups = VisualTreeHelper.GetOpenPopups(Window.Current);
            foreach (var popup in popups)
            {
                if (popup.Child is ContentDialog)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Show an Alert Dialog that can be dismissed by a button.
        /// </summary>
        /// <param name="title">Title of the dialog.</param>
        /// <param name="message">Content message of the dialog.</param>
        public static async Task ShowAlertAsync(string title, string message)
        {
            var dialog = new AlertDialog(title, message);
            await dialog.ShowAsyncQueue();
        }

        /// <summary>
        /// Show an Alert Dialog that can be dismissed by a button.
        /// </summary>
        /// <param name="title">Title of the dialog.</param>
        /// <param name="message">Content message of the dialog.</param>
        /// <param name="button">Label of the dialog button.</param>
        public static async Task ShowAlertAsync(string title, string message, string button)
        {
            var dialog = new AlertDialog(title, message, button);
            await dialog.ShowAsyncQueue();
        }

        /// <summary>
        /// Show a dialog with a message and "Ok"/"Cancel" buttons.
        /// </summary>
        /// <param name="title">Title of the dialog.</param>
        /// <param name="message">Message of the dialog.</param>
        /// <returns>True if the "Ok" button is pressed, else False.</returns>
        public static async Task<bool> ShowOkCancelAsync(string title, string message)
        {
            var dialog = new TwoButtonsDialog(title, message);
            return await dialog.ShowAsyncQueueBool();
        }

        /// <summary>
        /// Show a dialog with a message and two buttons.
        /// </summary>
        /// <param name="title">Title of the dialog.</param>
        /// <param name="message">Message of the dialog.</param>
        /// <param name="dialogType">A <see cref="TwoButtonsDialogType"/> value that indicates the buttons to display.</param>
        /// <param name="primaryButton">Label for the primary button.</param>
        /// <param name="secondaryButton">Label for the secondary button.</param>
        /// <returns>True if the primary button is pressed, else False.</returns>
        public static async Task<bool> ShowOkCancelAsync(string title, string message,
            TwoButtonsDialogType dialogType, string primaryButton = null, string secondaryButton = null)
        {
            var dialog = new TwoButtonsDialog(title, message, null,
                dialogType, primaryButton, secondaryButton);
            return await dialog.ShowAsyncQueueBool();
        }

        /// <summary>
        /// Show a dialog with a message, a warning and "Ok"/"Cancel" buttons.
        /// </summary>
        /// <param name="title">Title of the dialog.</param>
        /// <param name="message">Message of the dialog.</param>
        /// <param name="warning">Warning of the dialog.</param>
        /// <returns>True if the "Ok" button is pressed, else False.</returns>
        public static async Task<bool> ShowOkCancelAsync(string title, string message, string warning)
        {
            var dialog = new TwoButtonsDialog(title, message, warning);
            return await dialog.ShowAsyncQueueBool();
        }

        /// <summary>
        /// Show a dialog with a message, a warning and two buttons.
        /// </summary>
        /// <param name="title">Title of the dialog</param>
        /// <param name="message">Message of the dialog</param>
        /// <param name="warning">Warning of the dialog</param>
        /// <param name="dialogType">A <see cref="TwoButtonsDialogType"/> value that indicates the buttons to display.</param>
        /// <param name="primaryButton">Label for the primary button.</param>
        /// <param name="secondaryButton">Label for the secondary button.</param>
        /// <returns>True if the primary button is pressed, else False.</returns>
        public static async Task<bool> ShowOkCancelAsync(string title, string message, string warning,
            TwoButtonsDialogType dialogType, string primaryButton = null, string secondaryButton = null)
        {
            var dialog = new TwoButtonsDialog(title, message, warning,
                dialogType, primaryButton, secondaryButton);
            return await dialog.ShowAsyncQueueBool();
        }

        /// <summary>
        /// Show an standard input dialog.
        /// </summary>
        /// <param name="title">Title of the input dialog.</param>
        /// <param name="message">Message of the input dialog.</param>
        /// <param name="settings">Input dialog behavior/option settings.</param>
        public static async Task<string> ShowInputDialogAsync(string title, string message,
            InputDialogSettings settings = null)
        {
            var dialog = InputDialogInstance = 
                new InputDialog(title, message, null, null, settings);
            var result = await dialog.ShowAsyncQueueBool();
            return result ? dialog.ViewModel.InputText : null;
        }

        /// <summary>
        /// Show an standard input dialog.
        /// </summary>
        /// <param name="title">Title of the input dialog.</param>
        /// <param name="message">Message of the input dialog.</param>
        /// <param name="primaryButton">Label of the primary button of the input dialog.</param>
        /// <param name="secondaryButton">Label of the secondary button of the input dialog.</param>
        /// <param name="settings">Input dialog behavior/option settings.</param>
        public static async Task<string> ShowInputDialogAsync(string title, string message,
            string primaryButton, string secondaryButton, InputDialogSettings settings = null)
        {
            var dialog = InputDialogInstance = 
                new InputDialog(title, message, primaryButton, secondaryButton, settings);
            var result = await dialog.ShowAsyncQueueBool();
            return result ? dialog.ViewModel.InputText : null;
        }

        /// <summary>
        /// Show an input dialog which also executes an action.
        /// </summary>
        /// <param name="title">Title of the input dialog.</param>
        /// <param name="message">Message of the input dialog.</param>
        /// <param name="dialogAction">Action to do by the primary button.</param>
        /// <param name="settings">Input dialog behavior/option settings.</param>
        /// <returns>The dialog action result as <see cref="bool"/> value.</returns>
        public static async Task<bool> ShowInputActionDialogAsync(string title, string message,
            Func<string, bool> dialogAction, InputDialogSettings settings = null)
        {
            var dialog = InputDialogInstance = 
                new InputDialog(title, message, dialogAction, null, null, settings);
            return await dialog.ShowAsyncQueueBool();
        }

        /// <summary>
        /// Show an input dialog which also executes an action.
        /// </summary>
        /// <param name="title">Title of the input dialog.</param>
        /// <param name="message">Message of the input dialog.</param>
        /// <param name="primaryButton">Label of the primary button of the input dialog.</param>
        /// <param name="secondaryButton">Label of the secondary button of the input dialog.</param>
        /// <param name="dialogAction">Action to do by the primary button.</param>
        /// <param name="settings">Input dialog behavior/option settings.</param>
        /// <returns>The dialog action result as <see cref="bool"/> value.</returns>
        public static async Task<bool> ShowInputActionDialogAsync(string title, string message,
            string primaryButton, string secondaryButton, Func<string, bool> dialogAction,
            InputDialogSettings settings = null)
        {
            var dialog = InputDialogInstance = 
                new InputDialog(title, message, dialogAction, primaryButton, secondaryButton, settings);
            return await dialog.ShowAsyncQueueBool();
        }

        /// <summary>
        /// Show an input dialog which also executes an async action.
        /// </summary>
        /// <param name="title">Title of the input dialog.</param>
        /// <param name="message">Message of the input dialog.</param>
        /// <param name="dialogActionAsync">Async action to do by the primary button.</param>
        /// <param name="settings">Input dialog behavior/option settings.</param>
        /// <returns>The dialog action result as <see cref="bool"/> value.</returns>
        public static async Task<bool> ShowInputAsyncActionDialogAsync(string title, string message,
            Func<string, Task<bool>> dialogActionAsync, InputDialogSettings settings = null)
        {
            var dialog = InputDialogInstance = 
                new InputDialog(title, message, dialogActionAsync, null, null, settings);
            return await dialog.ShowAsyncQueueBool();
        }

        /// <summary>
        /// Show an input dialog which also executes an async action.
        /// </summary>
        /// <param name="title">Title of the input dialog.</param>
        /// <param name="message">Message of the input dialog.</param>
        /// <param name="primaryButton">Label of the primary button of the input dialog.</param>
        /// <param name="secondaryButton">Label of the secondary button of the input dialog.</param>
        /// <param name="dialogActionAsync">Async action to do by the primary button.</param>
        /// <param name="settings">Input dialog behavior/option settings.</param>
        /// <returns>The dialog action result as <see cref="bool"/> value.</returns>
        public static async Task<bool> ShowInputAsyncActionDialogAsync(string title, string message,
            string primaryButton, string secondaryButton, Func<string, Task<bool>> dialogActionAsync,
            InputDialogSettings settings = null)
        {
            var dialog = InputDialogInstance =
                new InputDialog(title, message, dialogActionAsync, primaryButton, secondaryButton, settings);
            return await dialog.ShowAsyncQueueBool();
        }

        /// <summary>
        /// Set the warning message of the input dialog displayed
        /// </summary>
        /// <param name="warningMessage">Text of the warning message</param>
        public static void SetInputDialogWarningMessage(string warningMessage)
        {
            if (InputDialogInstance?.ViewModel == null) return;
            InputDialogInstance.ViewModel.WarningText = warningMessage;
            InputDialogInstance.ViewModel.InputState= InputState.Warning;
        }

        /// <summary>
        /// Display an alert dialog indicating that the MEGA SSL key 
        /// can't be verified (API_ESSL Error) and giving the user several options.
        /// </summary>
        public static async Task<ContentDialogResult> ShowSSLCertificateAlert()
        {
            var dialog = new TwoButtonsDialog(
                ResourceService.AppMessages.GetString("AM_SSLKeyError_Title"),
                ResourceService.AppMessages.GetString("AM_SSLKeyError"), null,
                TwoButtonsDialogType.Custom,
                ResourceService.UiResources.GetString("UI_Retry"),
                ResourceService.UiResources.GetString("UI_OpenBrowser"),
                true, ResourceService.UiResources.GetString("UI_Ignore"),
                MegaDialogStyle.AlertDialog);

            return await dialog.ShowAsyncQueue();
        }

        public static async void ShowOverquotaAlert()
        {
            var result = await ShowOkCancelAsync(
                ResourceService.AppMessages.GetString("AM_OverquotaAlert_Title"),
                ResourceService.AppMessages.GetString("AM_OverquotaAlert"),
                TwoButtonsDialogType.YesNo);

            if (!result) return;

            UiService.OnUiThread(() =>
            {
                NavigateService.Instance.Navigate(typeof(MyAccountPage), false,
                    NavigationObject.Create(typeof(MainViewModel), NavigationActionType.Upgrade));
            });
        }

        public static async void ShowTransferOverquotaWarning()
        {
            var dialog = new TransferOverquotaWarningDialog();
            await dialog.ShowAsyncQueue();
        }

        /// <summary>
        /// Shows an alert dialog to inform that the DEBUG mode is enabled.
        /// <para>Also asks if the user wants to disable it.</para>
        /// </summary>
        public static async void ShowDebugModeAlert()
        {
            var result = await ShowOkCancelAsync(
                ResourceService.AppMessages.GetString("AM_DebugModeEnabled_Title"),
                ResourceService.AppMessages.GetString("AM_DebugModeEnabled_Message"),
                TwoButtonsDialogType.YesNo);

            if (result)
                DebugService.DebugSettings.DisableDebugMode();

            DebugService.DebugSettings.ShowDebugAlert = false;
        }

        /// <summary>
        /// Show a dialog to change the account email
        /// </summary>
        public static async void ShowChangeEmailDialog()
        {
            var changeEmailDialog = new ChangeEmailDialog();
            await changeEmailDialog.ShowAsyncQueue();
        }

        /// <summary>
        /// Show a dialog indicating that is waiting for an email confirmation
        /// </summary>
        /// <param name="email">Email for which is waiting confirmation</param>
        public static async void ShowAwaitEmailConfirmationDialog(string email)
        {
            if (awaitEmailConfirmationDialog == null)
                awaitEmailConfirmationDialog = new AwaitEmailConfirmationDialog(email);
            else
                awaitEmailConfirmationDialog.ViewModel.Email = email;

            await awaitEmailConfirmationDialog.ShowAsyncQueue();
        }

        /// <summary>
        /// Close the await email confirmation dialog if exists
        /// </summary>
        public static void CloseAwaitEmailConfirmationDialog()
        {
            awaitEmailConfirmationDialog?.Hide();
        }

        /// <summary>
        /// Show a dialog to check if the user remember the account password
        /// </summary>
        /// <param name="atLogout">True if the dialog is being displayed just before a logout</param>
        public static async void ShowPasswordReminderDialog(bool atLogout)
        {
            var passwordReminderDialog = new PasswordReminderDialog(atLogout);
            await passwordReminderDialog.ShowAsyncQueue();
        }

        /// <summary>
        /// Show a dialog to change the account password
        /// </summary>
        public static async void ShowChangePasswordDialog()
        {
            var changePasswordDialog = new ChangePasswordDialog();
            await changePasswordDialog.ShowAsyncQueue();
        }

        /// <summary>
        /// Show a dialog to setup the Multi-Factor Authentication for the account
        /// </summary>
        /// <returns>TRUE if the user continues with the setup process or FALSE in other case</returns>
        public static async Task<bool> ShowMultiFactorAuthSetupDialogAsync()
        {
            var mfaSetupDialog = new MultiFactorAuthSetupDialog();
            await mfaSetupDialog.ShowAsyncQueue();
            return mfaSetupDialog.ViewModel.DialogResult;
        }

        /// <summary>
        /// Show a dialog to indicate that the user has successfully enabled the Multi-Factor Authentication
        /// </summary>
        public static async void ShowMultiFactorAuthEnabledDialog()
        {
            var mfaEnabledDialog = new MultiFactorAuthEnabledDialog();
            await mfaEnabledDialog.ShowAsyncQueue();
        }

        /// <summary>
        /// Show a dialog to indicate that the user has successfully disabled the Multi-Factor Authentication
        /// </summary>
        public static async void ShowMultiFactorAuthDisabledDialog()
        {
            var mfaDisabledDialog = new MultiFactorAuthDisabledDialog();
            await mfaDisabledDialog.ShowAsyncQueue();
        }

        /// <summary>
        /// Show an input dialog to type the MFA code and execute an action.
        /// </summary>
        /// <param name="dialogAction">Action to do by the primary button.</param>
        /// <param name="title">Custom title of the input dialog.</param>
        /// <param name="message">Custom message of the input dialog.</param>
        /// <param name="showLostDeviceLink">Indicates if show the lost device link or not.</param>
        /// <returns>The dialog action result as <see cref="bool"/> value.</returns>
        public static async Task<bool> ShowMultiFactorAuthCodeInputDialogAsync(
            Func<string, bool> dialogAction,
            string title = null, string message = null, bool showLostDeviceLink = true)
        {
            var dialog = MultiFactorAuthCodeInputDialogInstance =
                new MultiFactorAuthCodeInputDialog(dialogAction, title, message, showLostDeviceLink);
            return await dialog.ShowAsyncQueueBool();
        }

        /// <summary>
        /// Show an input dialog to type the MFA code and execute an async action.
        /// </summary>
        /// <param name="dialogActionAsync">Async action to do by the primary button.</param>
        /// <param name="title">Custom title of the input dialog.</param>
        /// <param name="message">Custom message of the input dialog.</param>
        /// <param name="showLostDeviceLink">Indicates if show the lost device link or not.</param>
        /// <returns>The dialog action result as <see cref="bool"/> value.</returns>
        public static async Task<bool> ShowAsyncMultiFactorAuthCodeInputDialogAsync(
            Func<string, Task<bool>> dialogActionAsync,
            string title = null, string message = null, bool showLostDeviceLink = true)
        {
            var dialog = MultiFactorAuthCodeInputDialogInstance =
                new MultiFactorAuthCodeInputDialog(dialogActionAsync, title, message, showLostDeviceLink);
            return await dialog.ShowAsyncQueueBool();
        }

        /// <summary>
        /// Set the warning message of the MFA code input dialog displayed
        /// </summary>
        /// <param name="warningMessage">Text of the warning message</param>
        public static void SetMultiFactorAuthCodeInputDialogWarningMessage(string warningMessage = null)
        {
            if (MultiFactorAuthCodeInputDialogInstance?.ViewModel == null) return;
            MultiFactorAuthCodeInputDialogInstance.ViewModel.InputState = InputState.Warning;
            MultiFactorAuthCodeInputDialogInstance.ViewModel.DigitColor =
                (SolidColorBrush)Application.Current.Resources["MegaRedColorBrush"];
            MultiFactorAuthCodeInputDialogInstance.ViewModel.WarningText = warningMessage ??
                ResourceService.AppMessages.GetString("AM_InvalidCode");
        }

        /// <summary>
        /// Shows a dialog to allow copy a node link to the clipboard or share it using other app
        /// </summary>
        /// <param name="node">Node to share the link</param>
        public static async void ShowShareLink(NodeViewModel node)
        {
            var dialog = new MegaContentDialog
            {
                Background = (Brush)Application.Current.Resources["MegaAppBackgroundBrush"],
                BorderBrush = (Brush)Application.Current.Resources["MegaDialogBorderBrush"],
                CloseButtonVisibility = Visibility.Visible,
                IsPrimaryButtonEnabled = true,
                IsSecondaryButtonEnabled = true,
                Margin = new Thickness(0, 0, 24, 24),
                PrimaryButtonText = ResourceService.UiResources.GetString("UI_Copy"),
                SecondaryButtonText = ResourceService.UiResources.GetString("UI_Share"),
                Style = (Style)Application.Current.Resources["MegaAlertDialogStyle"],
                Title = ResourceService.UiResources.GetString("UI_ExportLink")
            };
            dialog.CloseButtonCommand = new RelayCommand(() => dialog.Hide());

            var stackPanel = new StackPanel
            {
                Margin = new Thickness(24, 24, 0, 0)
            };

            var messageText = new TextBlock
            {
                Text = node.ExportLink,
                Margin = new Thickness(0, 20, 0, 12),
                TextWrapping = TextWrapping.WrapWholeWords,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            var linkWithoutKey = new RadioButton
            {
                Content = ResourceService.UiResources.GetString("UI_LinkWithoutKey")
            };
            linkWithoutKey.Checked += (sender, args) => messageText.Text = node.OriginalMNode.getPublicLink(false);

            var decryptionKey = new RadioButton
            {
                Content = ResourceService.UiResources.GetString("UI_DecryptionKey")
            };
            decryptionKey.Checked += (sender, args) => messageText.Text = node.OriginalMNode.getBase64Key();

            var linkWithKey = new RadioButton
            {
                Content = ResourceService.UiResources.GetString("UI_LinkWithKey"),
                IsChecked = true
            };
            linkWithKey.Checked += (sender, args) => messageText.Text = node.OriginalMNode.getPublicLink(true);

            stackPanel.Children.Add(linkWithoutKey);
            stackPanel.Children.Add(decryptionKey);
            stackPanel.Children.Add(linkWithKey);

            var stackPanelLinkWithExpirationDate = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            var linkWithExpirationDateLabel = new TextBlock
            {
                Text = node.SetLinkExpirationDateText,
                Margin = new Thickness(0, 20, 0, 8),
                TextWrapping = TextWrapping.WrapWholeWords,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            var enableLinkExpirationDateSwitch = new ToggleSwitch
            {
                IsOn = node.LinkWithExpirationTime,
                IsEnabled = AccountService.AccountDetails.IsProAccount
            };

            var expirationDateCalendarDatePicker = new CalendarDatePicker
            {
                IsEnabled = enableLinkExpirationDateSwitch.IsOn,
                DateFormat = "{day.integer(2)}‎/‎{month.integer(2)}‎/‎{year.full}",
                Date = node.LinkExpirationDate
            };
            expirationDateCalendarDatePicker.Opened += (sender, args) =>
            {
                expirationDateCalendarDatePicker.LightDismissOverlayMode = LightDismissOverlayMode.On;
                expirationDateCalendarDatePicker.MinDate = DateTime.Today.AddDays(1);                
            };
            expirationDateCalendarDatePicker.DateChanged += (sender, args) =>
            {
                expirationDateCalendarDatePicker.IsCalendarOpen = false;

                if (expirationDateCalendarDatePicker.Date == null)
                {
                    enableLinkExpirationDateSwitch.IsOn = false;
                    if (node.LinkExpirationTime > 0)
                        node.SetLinkExpirationTime(0);
                }
                else if (node.LinkExpirationDate == null ||
                    !node.LinkExpirationDate.Value.ToUniversalTime().Equals(expirationDateCalendarDatePicker.Date.Value.ToUniversalTime()))
                {
                    node.SetLinkExpirationTime(expirationDateCalendarDatePicker.Date.Value.ToUniversalTime().ToUnixTimeSeconds());
                }
            };

            enableLinkExpirationDateSwitch.Toggled += (sender, args) =>
            {
                expirationDateCalendarDatePicker.IsEnabled = enableLinkExpirationDateSwitch.IsOn;
                if (enableLinkExpirationDateSwitch.IsOn)
                    expirationDateCalendarDatePicker.Date = node.LinkExpirationDate;
                else
                    expirationDateCalendarDatePicker.Date = null;
            };

            stackPanelLinkWithExpirationDate.Children.Add(enableLinkExpirationDateSwitch);
            stackPanelLinkWithExpirationDate.Children.Add(expirationDateCalendarDatePicker);

            stackPanel.Children.Add(linkWithExpirationDateLabel);
            stackPanel.Children.Add(stackPanelLinkWithExpirationDate);
            stackPanel.Children.Add(messageText);
            dialog.Content = stackPanel;

            var result = await dialog.ShowAsyncQueue();
            switch (result)
            {
                case ContentDialogResult.None:
                    break;

                case ContentDialogResult.Primary:
                    ShareService.CopyLinkToClipboard(messageText.Text);
                    break;

                case ContentDialogResult.Secondary:
                    ShareService.ShareLink(messageText.Text);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Creates the sort menu with all the sort options.
        /// </summary>
        /// <param name="folder">Folder to sort.</param>
        /// <returns>The flyout menu with the sort options.</returns>
        public static MenuFlyout CreateSortMenu(BaseFolderViewModel folder)
        {
            var currentSortOrder = UiService.GetSortOrder(folder?.FolderRootNode?.Base64Handle, folder?.FolderRootNode?.Name);

            MenuFlyout menuFlyout = new MenuFlyout();

            menuFlyout.Items?.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionName"),
                Foreground = GetSortMenuItemForeground(currentSortOrder, NodesSortOrderType.ORDER_NAME),
                Command = new RelayCommand(() =>
                {
                    var newOrder = folder != null && folder.ItemCollection.IsCurrentOrderAscending ?
                        MSortOrderType.ORDER_ALPHABETICAL_ASC : MSortOrderType.ORDER_ALPHABETICAL_DESC;
                    if (folder == null) return;
                    UiService.SetSortOrder(folder.FolderRootNode.Base64Handle, newOrder);
                    folder.LoadChildNodes();
                })
            });

            menuFlyout.Items?.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionSize"),
                Foreground = GetSortMenuItemForeground(currentSortOrder, NodesSortOrderType.ORDER_SIZE),
                Command = new RelayCommand(() =>
                {
                    var newOrder = folder != null && folder.ItemCollection.IsCurrentOrderAscending ?
                        MSortOrderType.ORDER_SIZE_ASC : MSortOrderType.ORDER_SIZE_DESC;
                    if (folder == null) return;
                    UiService.SetSortOrder(folder.FolderRootNode.Base64Handle, newOrder);
                    folder.LoadChildNodes();
                })
            });

            menuFlyout.Items?.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionDateModified"),
                Foreground = GetSortMenuItemForeground(currentSortOrder, NodesSortOrderType.ORDER_MODIFICATION),
                Command = new RelayCommand(() =>
                {
                    var newOrder = folder != null && folder.ItemCollection.IsCurrentOrderAscending ?
                        MSortOrderType.ORDER_MODIFICATION_ASC : MSortOrderType.ORDER_MODIFICATION_DESC;
                    if (folder == null) return;
                    UiService.SetSortOrder(folder.FolderRootNode.Base64Handle, newOrder);
                    folder.LoadChildNodes();
                })
            });

            menuFlyout.Items?.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionType"),
                Foreground = GetSortMenuItemForeground(currentSortOrder, NodesSortOrderType.ORDER_TYPE),
                Command = new RelayCommand(() =>
                {
                    var newOrder = folder != null && folder.ItemCollection.IsCurrentOrderAscending ?
                        MSortOrderType.ORDER_DEFAULT_ASC : MSortOrderType.ORDER_DEFAULT_DESC;
                    if (folder == null) return;
                    UiService.SetSortOrder(folder.FolderRootNode.Base64Handle, newOrder);
                    folder.LoadChildNodes();
                })
            });

            return menuFlyout;
        }

        /// <summary>
        /// Gets the sort menu item foreground color depending on the current sort order.
        /// </summary>
        /// <param name="currentSortOrder">Current sort order of the list/collection.</param>
        /// <param name="sortOrderToCheck">Sort order to check.</param>
        /// <returns>The brush object with the color.</returns>
        private static Brush GetSortMenuItemForeground(object currentSortOrder, object sortOrderToCheck)
        {
            if (currentSortOrder is MSortOrderType && sortOrderToCheck is NodesSortOrderType)
            {
                switch ((MSortOrderType)currentSortOrder)
                {
                    case MSortOrderType.ORDER_ALPHABETICAL_ASC:
                    case MSortOrderType.ORDER_ALPHABETICAL_DESC:
                        if ((NodesSortOrderType)sortOrderToCheck == NodesSortOrderType.ORDER_NAME)
                            return (SolidColorBrush)Application.Current.Resources["MegaRedColorBrush"];
                        break;

                    case MSortOrderType.ORDER_CREATION_ASC:
                    case MSortOrderType.ORDER_CREATION_DESC:
                        if ((NodesSortOrderType)sortOrderToCheck == NodesSortOrderType.ORDER_CREATION)
                            return (SolidColorBrush)Application.Current.Resources["MegaRedColorBrush"];
                        break;

                    case MSortOrderType.ORDER_DEFAULT_ASC:
                    case MSortOrderType.ORDER_DEFAULT_DESC:
                        if ((NodesSortOrderType)sortOrderToCheck == NodesSortOrderType.ORDER_TYPE)
                            return (SolidColorBrush)Application.Current.Resources["MegaRedColorBrush"];
                        break;

                    case MSortOrderType.ORDER_MODIFICATION_ASC:
                    case MSortOrderType.ORDER_MODIFICATION_DESC:
                        if ((NodesSortOrderType)sortOrderToCheck == NodesSortOrderType.ORDER_MODIFICATION)
                            return (SolidColorBrush)Application.Current.Resources["MegaRedColorBrush"];
                        break;

                    case MSortOrderType.ORDER_SIZE_ASC:
                    case MSortOrderType.ORDER_SIZE_DESC:
                        if ((NodesSortOrderType)sortOrderToCheck == NodesSortOrderType.ORDER_SIZE)
                            return (SolidColorBrush)Application.Current.Resources["MegaRedColorBrush"];
                        break;
                }
            }

            if (currentSortOrder is MSortOrderType && sortOrderToCheck is MSortOrderType &&
                (MSortOrderType)currentSortOrder == (MSortOrderType)sortOrderToCheck)
                return (SolidColorBrush)Application.Current.Resources["MegaRedColorBrush"];

            if (currentSortOrder is ContactsSortOrderType && sortOrderToCheck is ContactsSortOrderType &&
                (ContactsSortOrderType)currentSortOrder == (ContactsSortOrderType)sortOrderToCheck)
                return (SolidColorBrush)Application.Current.Resources["MegaRedColorBrush"];

            if (currentSortOrder is ContactRerquestsSortOrderType && sortOrderToCheck is ContactRerquestsSortOrderType &&
                (ContactRerquestsSortOrderType)currentSortOrder == (ContactRerquestsSortOrderType)sortOrderToCheck)
                return (SolidColorBrush)Application.Current.Resources["MegaRedColorBrush"];

            if (currentSortOrder is IncomingSharesSortOrderType && sortOrderToCheck is IncomingSharesSortOrderType &&
                (IncomingSharesSortOrderType)currentSortOrder == (IncomingSharesSortOrderType)sortOrderToCheck)
                return (SolidColorBrush)Application.Current.Resources["MegaRedColorBrush"];

            if (currentSortOrder is OutgoingSharesSortOrderType && sortOrderToCheck is OutgoingSharesSortOrderType &&
                (OutgoingSharesSortOrderType)currentSortOrder == (OutgoingSharesSortOrderType)sortOrderToCheck)
                return (SolidColorBrush)Application.Current.Resources["MegaRedColorBrush"];

            return (SolidColorBrush)Application.Current.Resources["MegaAppForegroundBrush"];
        }

        /// <summary>
        /// Creates a sort menu for contacts.
        /// </summary>
        /// <returns>The flyout menu with the sort options.</returns>
        public static MenuFlyout CreateContactsSortMenu(ContactsListViewModel contacts,
            bool showReferralStatusSort = false)
        {
            MenuFlyout menuFlyout = new MenuFlyout();

            menuFlyout.Items?.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionName"),
                Foreground = GetSortMenuItemForeground(contacts.CurrentOrder, ContactsSortOrderType.ORDER_NAME),
                Command = new RelayCommand(() =>
                {
                    contacts.CurrentOrder = ContactsSortOrderType.ORDER_NAME;
                    contacts.SortBy(contacts.CurrentOrder, contacts.ItemCollection.CurrentOrderDirection);
                })
            });

            menuFlyout.Items?.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionEmail"),
                Foreground = GetSortMenuItemForeground(contacts.CurrentOrder, ContactsSortOrderType.ORDER_EMAIL),
                Command = new RelayCommand(() =>
                {
                    contacts.CurrentOrder = ContactsSortOrderType.ORDER_EMAIL;
                    contacts.SortBy(contacts.CurrentOrder, contacts.ItemCollection.CurrentOrderDirection);
                })
            });

            if (showReferralStatusSort)
            {
                menuFlyout.Items?.Add(new MenuFlyoutItem()
                {
                    Text = ResourceService.UiResources.GetString("UI_SortOptionReferralStatus"),
                    Foreground = GetSortMenuItemForeground(contacts.CurrentOrder, ContactsSortOrderType.ORDER_STATUS),
                    Command = new RelayCommand(() =>
                    {
                        contacts.CurrentOrder = ContactsSortOrderType.ORDER_STATUS;
                        contacts.SortBy(contacts.CurrentOrder, contacts.ItemCollection.CurrentOrderDirection);
                    })
                });
            }

            return menuFlyout;
        }

        /// <summary>
        /// Creates a sort menu for contacts.
        /// </summary>
        /// <returns>The flyout menu with the sort options.</returns>
        public static MenuFlyout CreateInviteContactsSortMenu(ContactsListViewModel contacts)
        {
            MenuFlyout menuFlyout = new MenuFlyout();

            menuFlyout.Items?.Add(new MenuFlyoutItem
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionName"),
                Foreground = GetSortMenuItemForeground(contacts.CurrentOrder, ContactsSortOrderType.ORDER_NAME),
                Command = new RelayCommand(() =>
                {
                    contacts.CurrentOrder = ContactsSortOrderType.ORDER_NAME;
                    contacts.SortBy(contacts.CurrentOrder, contacts.ItemCollection.CurrentOrderDirection);
                })
            });

            menuFlyout.Items?.Add(new MenuFlyoutItem
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionReferralStatus"),
                Foreground = GetSortMenuItemForeground(contacts.CurrentOrder, ContactsSortOrderType.ORDER_STATUS),
                Command = new RelayCommand(() =>
                {
                    contacts.CurrentOrder = ContactsSortOrderType.ORDER_STATUS;
                    contacts.SortBy(contacts.CurrentOrder, contacts.ItemCollection.CurrentOrderDirection);
                })
            });

            return menuFlyout;
        }

        /// <summary>
        /// Creates a sort menu for contact requests.
        /// </summary>
        /// <returns>The flyout menu with the sort options.</returns>
        public static MenuFlyout CreateContactRequestsSortMenu(ContactRequestsListViewModel contactRequests)
        {
            MenuFlyout menuFlyout = new MenuFlyout();

            menuFlyout.Items?.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionName"),
                Foreground = GetSortMenuItemForeground(contactRequests.CurrentOrder, ContactRerquestsSortOrderType.ORDER_NAME),
                Command = new RelayCommand(() =>
                {
                    contactRequests.CurrentOrder = ContactRerquestsSortOrderType.ORDER_NAME;
                    contactRequests.SortBy(contactRequests.CurrentOrder, contactRequests.ItemCollection.CurrentOrderDirection);
                })
            });

            return menuFlyout;
        }

        /// <summary>
        /// Creates a sort menu for incoming shared items.
        /// </summary>
        /// <returns>The flyout menu with the sort options.</returns>
        public static MenuFlyout CreateIncomingSharedItemsSortMenu(IncomingSharesViewModel sharedItems, 
            bool areContactIncomingShares = false)
        {
            MenuFlyout menuFlyout = new MenuFlyout();

            menuFlyout.Items?.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionName"),
                Foreground = GetSortMenuItemForeground(sharedItems.CurrentOrder, IncomingSharesSortOrderType.ORDER_NAME),
                Command = new RelayCommand(() =>
                {
                    sharedItems.CurrentOrder = IncomingSharesSortOrderType.ORDER_NAME;
                    sharedItems.SortBy(sharedItems.CurrentOrder, sharedItems.ItemCollection.CurrentOrderDirection);
                })
            });

            menuFlyout.Items?.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionDateModified"),
                Foreground = GetSortMenuItemForeground(sharedItems.CurrentOrder, IncomingSharesSortOrderType.ORDER_MODIFICATION),
                Command = new RelayCommand(() =>
                {
                    sharedItems.CurrentOrder = IncomingSharesSortOrderType.ORDER_MODIFICATION;
                    sharedItems.SortBy(sharedItems.CurrentOrder, sharedItems.ItemCollection.CurrentOrderDirection);
                })
            });

            menuFlyout.Items?.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionAccessLevel"),
                Foreground = GetSortMenuItemForeground(sharedItems.CurrentOrder, IncomingSharesSortOrderType.ORDER_ACCESS),
                Command = new RelayCommand(() =>
                {
                    sharedItems.CurrentOrder = IncomingSharesSortOrderType.ORDER_ACCESS;
                    sharedItems.SortBy(sharedItems.CurrentOrder, sharedItems.ItemCollection.CurrentOrderDirection);
                })
            });

            if(!areContactIncomingShares)
            {
                menuFlyout.Items?.Add(new MenuFlyoutItem()
                {
                    Text = ResourceService.UiResources.GetString("UI_SortOptionOwner"),
                    Foreground = GetSortMenuItemForeground(sharedItems.CurrentOrder, IncomingSharesSortOrderType.ORDER_OWNER),
                    Command = new RelayCommand(() =>
                    {
                        sharedItems.CurrentOrder = IncomingSharesSortOrderType.ORDER_OWNER;
                        sharedItems.SortBy(sharedItems.CurrentOrder, sharedItems.ItemCollection.CurrentOrderDirection);
                    })
                });
            }

            return menuFlyout;
        }

        /// <summary>
        /// Creates a sort menu for outgoing shared items.
        /// </summary>
        /// <returns>The flyout menu with the sort options.</returns>
        public static MenuFlyout CreateOutgoingSharedItemsSortMenu(OutgoingSharesViewModel sharedItems,
            bool areContactIncomingShares = false)
        {
            MenuFlyout menuFlyout = new MenuFlyout();

            menuFlyout.Items?.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionName"),
                Foreground = GetSortMenuItemForeground(sharedItems.CurrentOrder, OutgoingSharesSortOrderType.ORDER_NAME),
                Command = new RelayCommand(() =>
                {
                    sharedItems.CurrentOrder = OutgoingSharesSortOrderType.ORDER_NAME;
                    sharedItems.SortBy(sharedItems.CurrentOrder, sharedItems.ItemCollection.CurrentOrderDirection);
                })
            });

            return menuFlyout;
        }

        #endregion
    }
}
