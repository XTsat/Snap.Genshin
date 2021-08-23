﻿using DGP.Genshin.Data.Characters;
using System.Collections.Generic;
using System.Linq;

namespace DGP.Genshin.Services
{
    public class WeeklyViewService
    {
        private readonly MetaDataService dataService = MetaDataService.Instance;

        #region 风魔龙
        private IEnumerable<Character> dvalinsPlume;
        public IEnumerable<Character> DvalinsPlume
        {
            get
            {
                if (this.dvalinsPlume == null)
                {
                    this.dvalinsPlume = this.dataService.Characters
                        .Where(c => c.Weekly.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_461_70.png").ToList();
                }
                return this.dvalinsPlume;
            }
        }
        private IEnumerable<Character> dvalinsClaw;
        public IEnumerable<Character> DvalinsClaw
        {
            get
            {
                if (this.dvalinsClaw == null)
                {
                    this.dvalinsClaw = this.dataService.Characters
                        .Where(c => c.Weekly.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_462_70.png").ToList();
                }
                return this.dvalinsClaw;
            }
        }
        private IEnumerable<Character> dvalinsSigh;
        public IEnumerable<Character> DvalinsSigh
        {
            get
            {
                if (this.dvalinsSigh == null)
                {
                    this.dvalinsSigh = this.dataService.Characters
                        .Where(c => c.Weekly.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_463_70.png").ToList();
                }
                return this.dvalinsSigh;
            }
        }
        #endregion

        #region 北风的王狼
        private IEnumerable<Character> tailofBoreas;
        public IEnumerable<Character> TailofBoreas
        {
            get
            {
                if (this.tailofBoreas == null)
                {
                    this.tailofBoreas = this.dataService.Characters
                        .Where(c => c.Weekly.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_464_70.png").ToList();
                }
                return this.tailofBoreas;
            }
        }
        private IEnumerable<Character> ringofBoreas;
        public IEnumerable<Character> RingofBoreas
        {
            get
            {
                if (this.ringofBoreas == null)
                {
                    this.ringofBoreas = this.dataService.Characters
                        .Where(c => c.Weekly.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_465_70.png").ToList();
                }
                return this.ringofBoreas;
            }
        }
        private IEnumerable<Character> spiritLocketofBoreas;
        public IEnumerable<Character> SpiritLocketofBoreas
        {
            get
            {
                if (this.spiritLocketofBoreas == null)
                {
                    this.spiritLocketofBoreas = this.dataService.Characters
                        .Where(c => c.Weekly.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_466_70.png").ToList();
                }
                return this.spiritLocketofBoreas;
            }
        }
        #endregion

        #region 公子
        private IEnumerable<Character> tuskofMonocerosCaeli;
        public IEnumerable<Character> TuskofMonocerosCaeli
        {
            get
            {
                if (this.tuskofMonocerosCaeli == null)
                {
                    this.tuskofMonocerosCaeli = this.dataService.Characters
                        .Where(c => c.Weekly.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_467_70.png").ToList();
                }
                return this.tuskofMonocerosCaeli;
            }
        }
        private IEnumerable<Character> shardofaFoulLegacy;
        public IEnumerable<Character> ShardofaFoulLegacy
        {
            get
            {
                if (this.shardofaFoulLegacy == null)
                {
                    this.shardofaFoulLegacy = this.dataService.Characters
                        .Where(c => c.Weekly.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_468_70.png").ToList();
                }
                return this.shardofaFoulLegacy;
            }
        }
        private IEnumerable<Character> shadowoftheWarrior;
        public IEnumerable<Character> ShadowoftheWarrior
        {
            get
            {
                if (this.shadowoftheWarrior == null)
                {
                    this.shadowoftheWarrior = this.dataService.Characters
                        .Where(c => c.Weekly.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_469_70.png").ToList();
                }
                return this.shadowoftheWarrior;
            }
        }
        #endregion

        #region 若陀龙王
        private IEnumerable<Character> dragonLordsCrown;
        public IEnumerable<Character> DragonLordsCrown
        {
            get
            {
                if (this.dragonLordsCrown == null)
                {
                    this.dragonLordsCrown = this.dataService.Characters
                        .Where(c => c.Weekly.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_470_70.png").ToList();
                }
                return this.dragonLordsCrown;
            }
        }
        private IEnumerable<Character> bloodjadeBranch;
        public IEnumerable<Character> BloodjadeBranch
        {
            get
            {
                if (this.bloodjadeBranch == null)
                {
                    this.bloodjadeBranch = this.dataService.Characters
                        .Where(c => c.Weekly.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_471_70.png").ToList();
                }
                return this.bloodjadeBranch;
            }
        }
        private IEnumerable<Character> gildedScale;
        public IEnumerable<Character> GildedScale
        {
            get
            {
                if (this.gildedScale == null)
                {
                    this.gildedScale = this.dataService.Characters
                        .Where(c => c.Weekly.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_472_70.png").ToList();
                }
                return this.gildedScale;
            }
        }
        #endregion

        #region 女士
        private IEnumerable<Character> laSignoria1;
        public IEnumerable<Character> LaSignoria1
        {
            get
            {
                if (this.laSignoria1 == null)
                {
                    this.laSignoria1 = this.dataService.Characters
                        .Where(c => c.Weekly.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_480_70.png").ToList();
                }
                return this.laSignoria1;
            }
        }
        private IEnumerable<Character> laSignoria2;
        public IEnumerable<Character> LaSignoria2
        {
            get
            {
                if (this.laSignoria2 == null)
                {
                    this.laSignoria2 = this.dataService.Characters
                        .Where(c => c.Weekly.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_481_70.png").ToList();
                }
                return this.laSignoria2;
            }
        }
        private IEnumerable<Character> laSignoria3;
        public IEnumerable<Character> LaSignoria3
        {
            get
            {
                if (this.laSignoria3 == null)
                {
                    this.laSignoria3 = this.dataService.Characters
                        .Where(c => c.Weekly.Source == @"https://genshin.honeyhunterworld.com/img/upgrade/guide/i_482_70.png").ToList();
                }
                return this.laSignoria3;
            }
        }
        #endregion

        #region 单例
        private static WeeklyViewService instance;
        private static readonly object _lock = new();
        private WeeklyViewService()
        {
        }
        public static WeeklyViewService Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (_lock)
                    {
                        if (instance == null)
                        {
                            instance = new WeeklyViewService();
                        }
                    }
                }
                return instance;
            }
        }
        #endregion
    }
}
