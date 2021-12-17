﻿using DGP.Genshin.Common.Exceptions;
using DGP.Genshin.Common.Extensions.System;
using DGP.Genshin.Core;
using DGP.Genshin.Helpers.Notifications;
using DGP.Genshin.Services.Abstratcions;
using DGP.Genshin.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Uwp.Notifications;
using ModernWpf;
using System;
using System.IO;
using System.Reflection;
using System.Windows;

namespace DGP.Genshin
{
    /// <summary>
    /// Snap Genshin
    /// </summary>
    public partial class App : Application
    {
        private readonly ToastNotificationHandler toastNotificationHandler = new();
        private readonly SingleInstanceChecker singleInstanceChecker = new("Snap.Genshin");
        private readonly ServiceManager serviceManager;

        public App()
        {
            serviceManager = new();
        }

        /// <summary>
        /// 覆盖默认类型的 Current
        /// </summary>
        public new static App Current => (App)Application.Current;

        #region Dependency Injection Helper
        /// <summary>
        /// 全局消息交换器
        /// </summary>
        public static WeakReferenceMessenger Messenger => WeakReferenceMessenger.Default;

        /// <summary>
        /// 获取应注入的服务
        /// 获取时应使用服务的接口类型
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        /// <exception cref="SnapGenshinInternalException">对应的服务类型未注册</exception>
        public static TService GetService<TService>()
        {
            return Current.serviceManager.Services.GetService<TService>()
                ?? throw new SnapGenshinInternalException($"无法找到 {typeof(TService)} 类型的服务");
        }

        /// <summary>
        /// 获取应注入的视图模型
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <returns></returns>
        public static TViewModel GetViewModel<TViewModel>()
        {
            return GetService<TViewModel>();
        }
        #endregion

        #region LifeCycle
        protected override void OnStartup(StartupEventArgs e)
        {
            EnsureWorkingPath();
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            //handle notification activation
            SetupToastNotificationHandling();
            singleInstanceChecker.Ensure(Current);
            //file operation starts
            this.Log($"Snap Genshin - {Assembly.GetExecutingAssembly().GetName().Version}");
            GetService<ISettingService>().Initialize();
            //app theme
            SetAppTheme();
            //open main window
            base.OnStartup(e);
        }

        private void SetupToastNotificationHandling()
        {
            if (!ToastNotificationManagerCompat.WasCurrentProcessToastActivated())
            {
                //remove toast last time not cleared if it's manually launched
                ToastNotificationManagerCompat.History.Clear();
            }
            ToastNotificationManagerCompat.OnActivated += toastNotificationHandler.OnActivatedByNotification;
        }

        /// <summary>
        /// set working dir while launch by windows autorun
        /// </summary>
        private void EnsureWorkingPath()
        {
            if (Path.GetDirectoryName(AppContext.BaseDirectory) is string workingPath)
            {
                Environment.CurrentDirectory = workingPath;
            }
        }
        private void SetAppTheme()
        {
            ThemeManager.Current.ApplicationTheme =
                GetService<ISettingService>().GetOrDefault(Setting.AppTheme, null, Setting.ApplicationThemeConverter);
        }
        protected override void OnExit(ExitEventArgs e)
        {
            if (!singleInstanceChecker.IsExitDueToSingleInstanceRestriction)
            {
                GetService<ISettingService>().UnInitialize();
                GetViewModel<MetadataViewModel>().UnInitialize();
                this.Log($"Exit code:{e.ApplicationExitCode}");
            }
            base.OnExit(e);
        }
        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (!singleInstanceChecker.IsEnsureingSingleInstance)
            {
                using (StreamWriter sw = new(File.Create($"{DateTime.Now:yyyy-MM-dd HH-mm-ss}-crash.log")))
                {
                    sw.WriteLine($"Snap Genshin - {Assembly.GetExecutingAssembly().GetName().Version}");
                    sw.Write(e.ExceptionObject);
                }
            }
        }
        #endregion
    }
}
