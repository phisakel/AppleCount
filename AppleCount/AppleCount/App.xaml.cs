﻿using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using AppleCount.Services;
using AppleCount.Views;

namespace AppleCount
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            MainPage = new ApplePage(); // AppShell();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
