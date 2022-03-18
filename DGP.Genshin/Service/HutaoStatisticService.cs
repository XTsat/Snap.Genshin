﻿using DGP.Genshin.DataModel.HutaoAPI;
using DGP.Genshin.HutaoAPI;
using DGP.Genshin.HutaoAPI.GetModel;
using DGP.Genshin.HutaoAPI.PostModel;
using DGP.Genshin.MiHoYoAPI.Response;
using DGP.Genshin.Service.Abstraction;
using Snap.Core.DependencyInjection;
using Snap.Data.Primitive;
using Snap.Extenion.Enumerable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DGP.Genshin.Service
{
    [Service(typeof(IHutaoStatisticService), InjectAs.Transient)]
    internal class HutaoStatisticService : IHutaoStatisticService
    {
        private readonly PlayerRecordClient playerRecordClient = new();

        private IEnumerable<HutaoItem> avatarMap = null!;
        private IEnumerable<HutaoItem> weaponMap = null!;
        private IEnumerable<HutaoItem> reliquaryMap = null!;

        private IEnumerable<AvatarParticipation> _avatarParticipations = null!;
        private IEnumerable<AvatarConstellationNum> _avatarConstellationNums = null!;
        private IEnumerable<TeamCollocation> _teamCollocations = null!;
        private IEnumerable<WeaponUsage> _weaponUsages = null!;
        private IEnumerable<AvatarReliquaryUsage> _avatarReliquaryUsages = null!;
        private IEnumerable<TeamCombination> _teamCombinations = null!;

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            await playerRecordClient.InitializeAsync(cancellationToken);

            avatarMap = await playerRecordClient.GetAvatarMapAsync(cancellationToken);
            weaponMap = await playerRecordClient.GetWeaponMapAsync(cancellationToken);
            reliquaryMap = await playerRecordClient.GetReliquaryMapAsync(cancellationToken);

            _avatarParticipations = await playerRecordClient.GetAvatarParticipationsAsync(cancellationToken);
            _avatarConstellationNums = await playerRecordClient.GetAvatarConstellationsAsync(cancellationToken);
            _teamCollocations = await playerRecordClient.GetTeamCollocationsAsync(cancellationToken);
            _weaponUsages = await playerRecordClient.GetWeaponUsagesAsync(cancellationToken);
            _avatarReliquaryUsages = await playerRecordClient.GetAvatarReliquaryUsagesAsync(cancellationToken);
            _teamCombinations = await playerRecordClient.GetTeamCombinationsAsync(cancellationToken);
        }

        public async Task<Overview?> GetOverviewAsync(CancellationToken cancellationToken = default)
        {
            return await playerRecordClient.GetOverviewAsync(cancellationToken);
        }

        public IList<Indexed<int, Item<double>>> GetAvatarParticipations()
        {
            List<Indexed<int, Item<double>>> avatarParticipationResults = new();
            //保证 12层在前
            foreach (AvatarParticipation avatarParticipation in _avatarParticipations.OrderByDescending(x => x.Floor))
            {
                IEnumerable<Item<double>> result = avatarParticipation.AvatarUsage
                    .Join(avatarMap, rate => rate.Id, avatar => avatar.Id,
                    (rate, avatar) => new Item<double>(avatar.Id, avatar.Name, avatar.Url, rate.Value))
                    .OrderByDescending(x => x.Value)
                    .ToList();

                avatarParticipationResults
                    .Add(new Indexed<int, Item<double>>(avatarParticipation.Floor, result));
            }
            return avatarParticipationResults;
        }

        public IList<Rate<Item<IList<NamedValue<double>>>>> GetAvatarConstellations()
        {
            List<Rate<Item<IList<NamedValue<double>>>>> avatarConstellationsResults = new();
            foreach (AvatarConstellationNum avatarConstellationNum in _avatarConstellationNums)
            {
                HutaoItem? matched = avatarMap.FirstOrDefault(x => x.Id == avatarConstellationNum.Avatar);
                if (matched != null)
                {
                    IList<NamedValue<double>> result = avatarConstellationNum.Rate
                        .Select(rate => new NamedValue<double>($"{rate.Id} 命", rate.Value))
                        .ToList();

                    avatarConstellationsResults.Add(new()
                    {
                        Id = new(matched.Id, matched.Name, matched.Url, result),
                        Value = avatarConstellationNum.HoldingRate
                    });
                }
            }
            return avatarConstellationsResults
                .OrderByDescending(x => x.Id!.Id)
                .ToList();
        }

        public IList<Item<IList<Item<double>>>> GetTeamCollocations()
        {
            List<Item<IList<Item<double>>>> teamCollocationsResults = new();
            foreach (TeamCollocation teamCollocation in _teamCollocations)
            {
                HutaoItem? matched = avatarMap.FirstOrDefault(x => x.Id == teamCollocation.Avater);
                if (matched != null)
                {
                    IEnumerable<Item<double>> result = teamCollocation.Collocations
                    .Join(avatarMap.DistinctBy(a => a.Id), rate => rate.Id, avatar => avatar.Id,
                    (rate, avatar) => new Item<double>(avatar.Id, avatar.Name, avatar.Url, rate.Value));

                    teamCollocationsResults
                        .Add(new Item<IList<Item<double>>>(
                            matched.Id, matched.Name, matched.Url,
                            result.OrderByDescending(x => x.Value).ToList()));
                }
            }
            return teamCollocationsResults
                .OrderByDescending(x => x.Id)
                .ToList();
        }

        public IList<Item<IList<Item<double>>>> GetWeaponUsages()
        {
            List<Item<IList<Item<double>>>> weaponUsagesResults = new();
            foreach (WeaponUsage weaponUsage in _weaponUsages)
            {
                HutaoItem? matchedAvatar = avatarMap.FirstOrDefault(x => x.Id == weaponUsage.Avatar);
                if (matchedAvatar != null)
                {
                    IEnumerable<Item<double>> result = weaponUsage.Weapons
                    .Join(weaponMap, rate => rate.Id, weapon => weapon.Id,
                    (rate, weapon) => new Item<double>(weapon.Id, weapon.Name, weapon.Url, rate.Value));

                    weaponUsagesResults
                        .Add(new Item<IList<Item<double>>>(
                            matchedAvatar.Id, matchedAvatar.Name, matchedAvatar.Url,
                            result.OrderByDescending(x => x.Value).ToList()));
                }
            }
            return weaponUsagesResults
                .OrderByDescending(x => x.Id)
                .ToList();
        }

        public IList<Item<IList<NamedValue<Rate<IList<Item<int>>>>>>> GetReliquaryUsages()
        {
            List<Item<IList<NamedValue<Rate<IList<Item<int>>>>>>> reliquaryUsagesResults = new();
            foreach (AvatarReliquaryUsage reliquaryUsage in _avatarReliquaryUsages)
            {
                HutaoItem? matchedAvatar = avatarMap.FirstOrDefault(x => x.Id == reliquaryUsage.Avatar);
                if (matchedAvatar != null)
                {
                    List<NamedValue<Rate<IList<Item<int>>>>> result = new();

                    foreach (Rate<string> usage in reliquaryUsage.ReliquaryUsage)
                    {
                        List<Item<int>> relicList = new();
                        StringBuilder nameBuilder = new();
                        string[] relicWithCountArray = usage.Id!.Split(';');
                        foreach (string? relicAndCount in relicWithCountArray)
                        {
                            //0 id 1 count
                            string[]? relicSetIdAndCount = relicAndCount.Split('-');
                            HutaoItem? matchedRelic = reliquaryMap.FirstOrDefault(x => x.Id == int.Parse(relicSetIdAndCount[0]));
                            if (matchedRelic != null)
                            {
                                string count = relicSetIdAndCount[1];
                                nameBuilder.Append($"{count}×{matchedRelic.Name} ");
                                relicList.Add(new Item<int>(matchedRelic.Id, matchedRelic.Name, matchedRelic.Url, int.Parse(count)));
                            }
                        }
                        if (nameBuilder.Length > 0)
                        {
                            Rate<IList<Item<int>>> rate = new() { Id = relicList, Value = usage.Value };
                            //remove last space
                            NamedValue<Rate<IList<Item<int>>>> namedValue = new(nameBuilder.ToString()[0..^1], rate);
                            result.Add(namedValue);
                        }
                    }

                    reliquaryUsagesResults
                        .Add(new Item<IList<NamedValue<Rate<IList<Item<int>>>>>>(
                            matchedAvatar.Id, matchedAvatar.Name, matchedAvatar.Url,
                            result));
                }
            }
            return reliquaryUsagesResults
                .OrderByDescending(x => x.Id)
                .ToList();
        }

        public IList<Indexed<string, Rate<Two<IList<HutaoItem>>>>> GetTeamCombinations()
        {
            List<Indexed<string, Rate<Two<IList<HutaoItem>>>>> teamCombinationResults = new();
            foreach (TeamCombination temaCombination in _teamCombinations.OrderByDescending(x => x.Level.Floor).ThenByDescending(x => x.Level.Index))
            {
                IList<Rate<Two<IList<HutaoItem>>>> teamRates = temaCombination.Teams
                .Select(team => new Rate<Two<IList<HutaoItem>>>
                {
                    Value = team.Value,
                    Id = new(team.Id!.GetUp().Select(id => avatarMap.FirstOrDefault(a => a.Id == id)).NotNull().ToList(),
                    team.Id!.GetDown().Select(id => avatarMap.FirstOrDefault(a => a.Id == id)).NotNull().ToList())
                })
                .ToList();

                teamCombinationResults
                    .Add(new Indexed<string, Rate<Two<IList<HutaoItem>>>>(
                        $"{temaCombination.Level.Floor}-{temaCombination.Level.Index}",
                        teamRates.OrderByDescending(x => x.Value).Take(16).ToList()));
            }
            return teamCombinationResults;
        }

        public async Task GetAllRecordsAndUploadAsync(string cookie, Func<PlayerRecord, Task<bool>> confirmFunc, Func<Response, Task> resultAsyncFunc, CancellationToken cancellationToken = default)
        {
            await playerRecordClient.GetAllRecordsAndUploadAsync(cookie, confirmFunc, resultAsyncFunc, cancellationToken);
        }
    }
}
