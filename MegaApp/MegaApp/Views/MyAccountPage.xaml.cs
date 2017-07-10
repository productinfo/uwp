﻿using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using MegaApp.Classes;
using mega;
using MegaApp.Enums;
using MegaApp.Services;
using MegaApp.UserControls;
using MegaApp.ViewModels;

namespace MegaApp.Views
{
    // Helper class to define the viewmodel of this page
    // XAML cannot use generics in it's declaration.
    public class BaseMyAccountPage : PageEx<MyAccountViewModel> { }

    public sealed partial class MyAccountPage : BaseMyAccountPage
    {
        public MyAccountPage()
        {
            this.InitializeComponent();

            this.MainGrid.SizeChanged += OnSizeChanged;

            this.UpgradeView.UpgradeBackButtonTapped += OnUpgradeBackButtonTapped;
            this.UpgradeView.ProPlanSelected += OnProPlanSelected;
            this.UpgradeView.MembershipRadioButtonChecked += OnMembershipRadioButtonChecked;
            this.UpgradeView.PaymentMethodRadioButtonChecked += OnPaymentMethodRadioButtonChecked;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.ViewModel.Initialize();
            this.ViewModel.GoToUpgrade += GoToUpgrade;

            var navObj = NavigateService.GetNavigationObject(e.Parameter) as NavigationObject;
            var navActionType = navObj?.Action ?? NavigationActionType.Default;
            if (navActionType == NavigationActionType.Upgrade)
                this.MyAccountPivot.SelectedItem = this.UpgradePivot;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.ViewModel.GoToUpgrade -= GoToUpgrade;
            base.OnNavigatedTo(e);
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            SdkService.MegaSdk.retryPendingConnections();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width > 600)
            {
                this.GeneralView.MainStackPanel.Width = 600;

                this.StorageAndTransferView.MainStackPanel.Width = 600;
                this.StorageAndTransferView.MainStackPanel.HorizontalAlignment = HorizontalAlignment.Left;

                this.UpgradeView.MainStackPanel.Width = 600;
            }
            else
            {
                this.GeneralView.MainStackPanel.Width = this.MyAccountPivot.Width;

                this.StorageAndTransferView.MainStackPanel.Width = this.MyAccountPivot.Width;
                this.StorageAndTransferView.MainStackPanel.HorizontalAlignment = HorizontalAlignment.Stretch;

                this.UpgradeView.MainStackPanel.Width = this.MyAccountPivot.Width;
            }
        }

        private void GoToUpgrade(object sender, EventArgs e)
        {
            this.MyAccountPivot.SelectedItem = this.UpgradePivot;
        }

        private void OnPivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void OnProPlanSelected(object sender, TappedRoutedEventArgs e)
        {
            var selector = sender as ListViewBase;
            if (selector == null) return;

            this.ViewModel.UpgradeViewModel.SelectedPlan = ((ProductBase)selector.SelectedItem);
            this.ViewModel.UpgradeViewModel.Step2();

            // Set the monthly product as the default option
            this.UpgradeView.MonthlyRadioButton.IsChecked = true;
            this.ViewModel.UpgradeViewModel.SelectedProduct = this.ViewModel.UpgradeViewModel.MonthlyProduct;
        }

        private void OnUpgradeBackButtonTapped(object sender, TappedRoutedEventArgs e)
        {
            switch(this.ViewModel.UpgradeViewModel.CurrentStep)
            {
                case 2:
                    this.ViewModel.UpgradeViewModel.Step1();
                    this.UpgradeView.PlansGrid.SelectedItem = this.UpgradeView.PlansList.SelectedItem = null;
                    break;
                case 3:
                    this.ViewModel.UpgradeViewModel.Step2();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnMembershipRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            var radioButton = sender as RadioButton;
            if (radioButton == null) return;

            switch(radioButton.Tag.ToString())
            {
                case "Monthly":
                    this.ViewModel.UpgradeViewModel.SelectedProduct = this.ViewModel.UpgradeViewModel.MonthlyProduct;
                    break;
                case "Annual":
                    this.ViewModel.UpgradeViewModel.SelectedProduct = this.ViewModel.UpgradeViewModel.AnnualProduct;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            SetDefaultPaymentMethod();
        }

        private void OnPaymentMethodRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            var radioButton = sender as RadioButton;
            if (radioButton == null) return;

            switch (radioButton.Tag.ToString())
            {
                case "Centili":
                    this.ViewModel.UpgradeViewModel.SelectedPaymentMethod = MPaymentMethod.PAYMENT_METHOD_CENTILI;
                    break;
                case "Fortumo":
                    this.ViewModel.UpgradeViewModel.SelectedPaymentMethod = MPaymentMethod.PAYMENT_METHOD_FORTUMO;
                    break;
                case "InAppPurchase":
                    this.ViewModel.UpgradeViewModel.SelectedPaymentMethod = MPaymentMethod.PAYMENT_METHOD_WINDOWS_STORE;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SetDefaultPaymentMethod()
        {
            var selectedProduct = this.ViewModel.UpgradeViewModel.SelectedProduct;
            if (selectedProduct == null) return;

            if (selectedProduct.IsInAppPaymentMethodAvailable)
                this.UpgradeView.InAppPurchaseRadioButton.IsChecked = true;
            else if (selectedProduct.IsFortumoPaymentMethodAvailable)
                this.UpgradeView.FortumoRadioButton.IsChecked = true;
            else if (selectedProduct.IsCentiliPaymentMethodAvailable)
                this.UpgradeView.CentiliRadioButton.IsChecked = true;
        }
    }
}
