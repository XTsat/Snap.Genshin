﻿using DGP.Genshin.Common.Core.DependencyInjection;
using DGP.Genshin.Common.Extensions.System;
using DGP.Genshin.Common.Extensions.System.Collections.Generic;
using DGP.Genshin.Common.Threading;
using DGP.Genshin.Controls.TitleBarButtons;
using DGP.Genshin.MiHoYoAPI.GameRole;
using DGP.Genshin.MiHoYoAPI.Record.DailyNote;
using DGP.Genshin.Services.Abstratcions;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using ModernWpf.Controls.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DGP.Genshin.ViewModels
{
    [ViewModel(ViewModelType.Transient)]
    public class DailyNoteViewModel : ObservableObject
    {
        private readonly ICookieService cookieService;

        private List<DailyNote> dailyNotes = new();
        private IAsyncRelayCommand<TitleBarButton> openUICommand;

        public List<DailyNote> DailyNotes { get => dailyNotes; set => SetProperty(ref dailyNotes, value); }
        public IAsyncRelayCommand<TitleBarButton> OpenUICommand
        {
            get => openUICommand;
            [MemberNotNull(nameof(openUICommand))]
            set => openUICommand = value;
        }

        public DailyNoteViewModel(ICookieService cookieService)
        {
            this.cookieService = cookieService;
            OpenUICommand = new AsyncRelayCommand<TitleBarButton>(OpenUIAsync);
        }

        /// <summary>
        /// 打开浮出控件
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private async Task OpenUIAsync(TitleBarButton? t)
        {
            if (t?.ShowAttachedFlyout<Grid>(this) == true)
            {
                await RefreshDailyNotesAsync();
            }
        }

        private readonly TaskPreventer refreshDailyNoteTaskPreventer = new();
        /// <summary>
        /// 刷新实时便笺
        /// </summary>
        /// <returns></returns>
        private async Task RefreshDailyNotesAsync()
        {
            if (refreshDailyNoteTaskPreventer.ShouldExecute)
            {
                List<DailyNote> list = new();
                List<UserGameRole> roles = await new UserGameRoleProvider(cookieService.CurrentCookie).GetUserGameRolesAsync();

                foreach (UserGameRole role in roles)
                {
                    if (role.Region is not null && role.GameUid is not null)
                    {
                        DailyNote? note = await new DailyNoteProvider(cookieService.CurrentCookie).GetDailyNoteAsync(role.Region, role.GameUid);
                        bool result = list.AddIfNotNull(note);
                        this.Log($"add dailynote to result :{result}");
                    }
                }
                DailyNotes = list;
                refreshDailyNoteTaskPreventer.Release();
            }
        }
    }
}