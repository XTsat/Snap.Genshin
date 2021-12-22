﻿using DGP.Genshin.Common.Core.DependencyInjection;
using DGP.Genshin.Common.Exceptions;
using DGP.Genshin.Common.Threading;
using DGP.Genshin.Controls.TitleBarButtons;
using DGP.Genshin.Messages;
using DGP.Genshin.MiHoYoAPI.GameRole;
using DGP.Genshin.MiHoYoAPI.Sign;
using DGP.Genshin.Services.Abstratcions;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Uwp.Notifications;
using ModernWpf.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DGP.Genshin.ViewModels
{
    [ViewModel(ViewModelType.Transient)]
    public class SignInViewModel : ObservableRecipient, IRecipient<CookieChangedMessage>
    {
        private readonly ISettingService settingService;
        private readonly ICookieService cookieService;

        private SignInReward? signInReward;
        private SignInInfo? signInInfo;
        private List<UserGameRole> roles = new();
        private UserGameRole? selectedRole;
        private IAsyncRelayCommand<TitleBarButton> openUICommand;
        private IAsyncRelayCommand signInCommand;

        /// <summary>
        /// 签到奖励一览
        /// </summary>
        public SignInReward? SignInReward
        {
            get => signInReward;
            set => SetProperty(ref signInReward, value);
        }
        /// <summary>
        /// 当前签到状态信息
        /// </summary>
        public SignInInfo? SignInInfo
        {
            get => signInInfo;
            set => SetProperty(ref signInInfo, value);
        }
        /// <summary>
        /// 绑定的角色信息
        /// </summary>
        public List<UserGameRole> Roles
        {
            get => roles;
            set => SetProperty(ref roles, value);
        }
        /// <summary>
        /// 选择的角色
        /// </summary>
        public UserGameRole? SelectedRole
        {
            get => selectedRole;
            set
            {
                SetProperty(ref selectedRole, value);
                OnSelectedRoleChanged();
            }
        }
        private async void OnSelectedRoleChanged()
        {
            if (SelectedRole is not null)
            {
                SignInInfo = await Task.Run(() => new SignInProvider(cookieService.CurrentCookie).GetSignInInfoAsync(SelectedRole));
                SignInReward? reward = await Task.Run(new SignInProvider(cookieService.CurrentCookie).GetSignInRewardAsync);
                ApplyItemOpacity(reward);
                SignInReward = reward;
            }
        }
        /// <summary>
        /// 更新物品透明度
        /// </summary>
        private void ApplyItemOpacity(SignInReward? reward)
        {
            for (int i = 0; i < reward?.Awards?.Count; i++)
            {
                SignInAward item = reward.Awards[i];
                item.Opacity = (i + 1) <= SignInInfo?.TotalSignDay ? 0.2 : 1;
            }
        }
        public IAsyncRelayCommand<TitleBarButton> OpenUICommand
        {
            get => openUICommand;
            [MemberNotNull(nameof(openUICommand))]
            set => SetProperty(ref openUICommand, value);
        }

        public IAsyncRelayCommand SignInCommand
        {
            get => signInCommand;
            [MemberNotNull(nameof(signInCommand))]
            set => SetProperty(ref signInCommand, value);
        }

        public SignInViewModel(ISettingService settingService, ICookieService cookieService, IMessenger messenger) : base(messenger)
        {
            this.settingService = settingService;
            this.cookieService = cookieService;

            OpenUICommand = new AsyncRelayCommand<TitleBarButton>(OpenUIAsync);
            SignInCommand = new AsyncRelayCommand(SignInAsync);

            IsActive = true;
        }

        //prevent multiple signin task
        private TaskPreventer signInTaskPreventer = new();
        private async Task SignInAsync()
        {
            if (signInTaskPreventer.ShouldExecute)
            {
                await App.Current.Dispatcher.InvokeAsync(SignInInternalAsync).Task.Unwrap();
                signInTaskPreventer.Release();
            }
        }
        private async Task SignInInternalAsync()
        {
            if (SelectedRole is null)
            {
                throw new UnexceptedNullException("无角色信息时不能签到");
            }

            SignInResult? result = await new SignInProvider(cookieService.CurrentCookie).SignInAsync(SelectedRole);
            if (result is not null)
            {
                settingService[Setting.LastAutoSignInTime] = DateTime.Now;
            }
            new ToastContentBuilder()
                    .AddText(result is null ? "签到失败" : "签到成功")
                    .AddText(SelectedRole.ToString())
                    .AddAttributionText("米游社每日签到")
                    .Show();
            SignInReward? reward = await new SignInProvider(cookieService.CurrentCookie).GetSignInRewardAsync();
            ApplyItemOpacity(reward);
            SignInReward = reward;
            //refresh info
            SignInInfo = await new SignInProvider(cookieService.CurrentCookie).GetSignInInfoAsync(SelectedRole);
        }
        private async Task OpenUIAsync(TitleBarButton? t)
        {
            if (t?.ShowAttachedFlyout<Grid>(this) == true)
            {
                await InitializeInternalAsync();
            }
        }
        /// <summary>
        /// 初始化 <see cref="SignInReward"/> 与 <see cref="SignInInfo"/>
        /// </summary>
        /// <returns></returns>
        private async Task InitializeInternalAsync()
        {
            SignInReward ??= await Task.Run(new SignInProvider(cookieService.CurrentCookie).GetSignInRewardAsync);
            if (SignInInfo is null)
            {
                Roles = await Task.Run(new UserGameRoleProvider(cookieService.CurrentCookie).GetUserGameRolesAsync);
                SelectedRole = Roles.FirstOrDefault(i => i.IsChosen);
            }
        }

        /// <summary>
        /// Cookie改变
        /// </summary>
        /// <param name="message"></param>
        public void Receive(CookieChangedMessage message)
        {
            //reset sign in panel
            SignInReward = null;
            SignInInfo = null;
        }
    }
}