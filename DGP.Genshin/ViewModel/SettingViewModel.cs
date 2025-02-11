﻿using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DGP.Genshin.Core.Notification;
using DGP.Genshin.Factory.Abstraction;
using DGP.Genshin.Message;
using DGP.Genshin.Page;
using DGP.Genshin.Service.Abstraction.Setting;
using DGP.Genshin.Service.Abstraction.Updating;
using Microsoft.Toolkit.Uwp.Notifications;
using ModernWpf;
using Snap.Core.DependencyInjection;
using Snap.Core.Mvvm;
using Snap.Data.Primitive;
using Snap.Data.Utility;
using Snap.Win32;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DGP.Genshin.ViewModel
{
    /// <summary>
    /// 为需要及时响应的设置项提供 <see cref="Observable"/> 模型支持
    /// </summary>
    [ViewModel(InjectAs.Transient)]
    internal class SettingViewModel : ObservableRecipient2,
        IRecipient<UpdateProgressedMessage>,
        IRecipient<AdaptiveBackgroundOpacityChangedMessage>
    {
        private readonly IUpdateService updateService;

        private bool skipCacheCheck;
        private string versionString;
        private string userId;
        private AutoRun autoRun = new();
        private NamedValue<ApplicationTheme?> selectedTheme;
        private bool isTaskBarIconEnabled;
        private bool closeMainWindowAfterInitializaion;
        private UpdateProgressedMessage updateInfo;
        private double backgroundOpacity;
        private bool isBackgroundOpacityAdaptive;
        private bool isBannerWithNoItemVisible;
        private NamedValue<UpdateAPI> currentUpdateAPI;

        /// <summary>
        /// 构造一个新的设置视图模型
        /// </summary>
        /// <param name="updateService">更新服务</param>
        /// <param name="asyncRelayCommandFactory">异步命令工厂</param>
        /// <param name="messenger">消息器</param>
        public SettingViewModel(IUpdateService updateService, IAsyncRelayCommandFactory asyncRelayCommandFactory, IMessenger messenger)
            : base(messenger)
        {
            this.updateService = updateService;

            selectedTheme = Themes.First(x => x.Value == Setting2.AppTheme);
            currentUpdateAPI = UpdateAPIs.First(x => x.Value == Setting2.UpdateAPI);

            SkipCacheCheck = Setting2.SkipCacheCheck;
            IsTaskBarIconEnabled = Setting2.IsTaskBarIconEnabled;
            CloseMainWindowAfterInitializaion = Setting2.CloseMainWindowAfterInitializaion;
            BackgroundOpacity = Setting2.BackgroundOpacity;
            IsBackgroundOpacityAdaptive = Setting2.IsBackgroundOpacityAdaptive;
            IsBannerWithNoItemVisible = Setting2.IsBannerWithNoItemVisible;

            Version v = App.Current.Version;
            VersionString = $"Snap Genshin {v.Major} - Version {v.Minor}.{v.Build} Build {v.Revision}";
            UserId = User.Id;

            UpdateInfo = UpdateProgressedMessage.Default;
            ReleaseNote = updateService.ReleaseNote;

            CheckUpdateCommand = asyncRelayCommandFactory.Create(CheckUpdateAsync);
            CopyUserIdCommand = new RelayCommand(CopyUserIdToClipBoard);

            SponsorUICommand = new RelayCommand(NavigateToSponsorPage);
            OpenBackgroundFolderCommand = new RelayCommand(() => FileExplorer.Open(PathContext.Locate("Background")));
            OpenCacheFolderCommand = new RelayCommand(() => FileExplorer.Open(PathContext.Locate("Cache")));
            NextWallpaperCommand = new RelayCommand(SwitchToNextWallpaper);
            OpenImplementationPageCommand = new RelayCommand(() => messenger.Send(new NavigateRequestMessage(typeof(ImplementationPage))));
        }

        /// <summary>
        /// 可选的主题色
        /// </summary>
        public List<NamedValue<ApplicationTheme?>> Themes { get; } = new()
        {
            new("浅色", ApplicationTheme.Light),
            new("深色", ApplicationTheme.Dark),
            new("系统默认", null),
        };

        /// <summary>
        /// 可选的更新通道
        /// </summary>
        public List<NamedValue<UpdateAPI>> UpdateAPIs { get; } = new()
        {
            new("正式通道", UpdateAPI.PatchAPI),
            new("预览通道", UpdateAPI.GithubAPI),
        };

        /// <summary>
        /// 跳过完整性检查
        /// </summary>
        public bool SkipCacheCheck
        {
            get => skipCacheCheck;

            set
            {
                Setting2.SkipCacheCheck.Set(value, false);
                SetProperty(ref skipCacheCheck, value);
            }
        }

        /// <summary>
        /// 是否启用任务栏图标
        /// </summary>
        public bool IsTaskBarIconEnabled
        {
            get => isTaskBarIconEnabled;

            set
            {
                Setting2.IsTaskBarIconEnabled.Set(value, false);
                SetProperty(ref isTaskBarIconEnabled, value);
            }
        }

        /// <summary>
        /// 在初始化完成后关闭主窗体
        /// </summary>
        public bool CloseMainWindowAfterInitializaion
        {
            get => closeMainWindowAfterInitializaion;

            set
            {
                Setting2.CloseMainWindowAfterInitializaion.Set(value, false);
                SetProperty(ref closeMainWindowAfterInitializaion, value);
            }
        }

        /// <summary>
        /// 背景不透明度
        /// </summary>
        public double BackgroundOpacity
        {
            get => backgroundOpacity;

            set
            {
                Setting2.BackgroundOpacity.Set(value, false);
                Messenger.Send(new BackgroundOpacityChangedMessage(value));
                SetProperty(ref backgroundOpacity, value);
            }
        }

        /// <summary>
        /// 是否启用自适应背景不透明度
        /// </summary>
        public bool IsBackgroundOpacityAdaptive
        {
            get => isBackgroundOpacityAdaptive;

            set
            {
                Setting2.IsBackgroundOpacityAdaptive.Set(value, false);
                SetProperty(ref isBackgroundOpacityAdaptive, value);
            }
        }

        /// <summary>
        /// 版本字符串
        /// </summary>
        public string VersionString
        {
            get => versionString;

            [MemberNotNull("versionString")]
            set => SetProperty(ref versionString, value);
        }

        /// <summary>
        /// 用户设备ID
        /// </summary>
        public string UserId
        {
            get => userId;

            [MemberNotNull(nameof(userId))]
            set => userId = value;
        }

        /// <summary>
        /// 是否显示未抽取的卡池
        /// </summary>
        public bool IsBannerWithNoItemVisible
        {
            get => isBannerWithNoItemVisible;

            set
            {
                Setting2.IsBannerWithNoItemVisible.Set(value, false);
                SetProperty(ref isBannerWithNoItemVisible, value);
            }
        }

        /// <summary>
        /// 自启
        /// </summary>
        public AutoRun AutoRun
        {
            get => autoRun;

            set => autoRun = value;
        }

        /// <summary>
        /// 选中的主题
        /// </summary>
        public NamedValue<ApplicationTheme?> SelectedTheme
        {
            get => selectedTheme;

            set => SetPropertyAndCallbackOnCompletion(ref selectedTheme, value, v => { UpdateAppTheme(v!); });
        }

        /// <summary>
        /// 当前的更新通道
        /// </summary>
        public NamedValue<UpdateAPI> CurrentUpdateAPI
        {
            get => currentUpdateAPI;

            set => SetPropertyAndCallbackOnCompletion(ref currentUpdateAPI, value, v => Setting2.UpdateAPI.Set(v.Value));
        }

        /// <summary>
        /// 更新信息
        /// </summary>
        public UpdateProgressedMessage UpdateInfo
        {
            get => updateInfo;

            [MemberNotNull(nameof(updateInfo))]
            set => SetProperty(ref updateInfo, value);
        }

        /// <summary>
        /// 更新日志
        /// </summary>
        public string? ReleaseNote { get; }

        /// <summary>
        /// 检查更新命令
        /// </summary>
        public ICommand CheckUpdateCommand { get; }

        /// <summary>
        /// 复制用户设备ID命令
        /// </summary>
        public ICommand CopyUserIdCommand { get; }

        /// <summary>
        /// 打开赞助页面命令
        /// </summary>
        public ICommand SponsorUICommand { get; }

        /// <summary>
        /// 打开缓存文件夹命令
        /// </summary>
        public ICommand OpenCacheFolderCommand { get; }

        /// <summary>
        /// 打开背景图片文件夹
        /// </summary>
        public ICommand OpenBackgroundFolderCommand { get; }

        /// <summary>
        /// 下一张背景图片命令
        /// </summary>
        public ICommand NextWallpaperCommand { get; }

        /// <summary>
        /// 打开可切换实现页面命令
        /// </summary>
        public ICommand OpenImplementationPageCommand { get; }

        /// <inheritdoc/>
        public void Receive(UpdateProgressedMessage message)
        {
            UpdateInfo = message;
        }

        /// <inheritdoc/>
        public void Receive(AdaptiveBackgroundOpacityChangedMessage message)
        {
            BackgroundOpacity = message.Value;
        }

        private void CopyUserIdToClipBoard()
        {
            Clipboard.Clear();
            try
            {
                Clipboard.SetText(UserId);
            }
            catch
            {
                try
                {
                    Clipboard2.SetText(UserId);
                }
                catch
                {
                }
            }
        }

        private void NavigateToSponsorPage()
        {
            Messenger.Send(new NavigateRequestMessage(typeof(SponsorPage)));
        }

        private async Task CheckUpdateAsync()
        {
            UpdateState result = await updateService.CheckUpdateStateAsync();
            switch (result)
            {
                case UpdateState.NeedUpdate:
                    {
                        await updateService.DownloadAndInstallPackageAsync();
                        break;
                    }

                case UpdateState.IsNewestRelease:
                    {
                        new ToastContentBuilder()
                            .AddText("已是最新发行版")
                            .SafeShow();
                        break;
                    }

                case UpdateState.IsInsiderVersion:
                    {
                        new ToastContentBuilder()
                            .AddText("当前为开发测试版")
                            .SafeShow();
                        break;
                    }

                case UpdateState.NotAvailable:
                    {
                        new ToastContentBuilder()
                            .AddText("检查更新失败")
                            .SafeShow();
                        break;
                    }

                default:
                    break;
            }
        }

        private void SwitchToNextWallpaper()
        {
            Messenger.Send(new BackgroundChangeRequestMessage());
        }

        [PropertyChangedCallback]
        private void UpdateAppTheme(NamedValue<ApplicationTheme?> value)
        {
            Setting2.AppTheme.Set(value.Value);
            ThemeManager.Current.ApplicationTheme = Setting2.AppTheme;
        }
    }
}