﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaintCoinach.Xiv {
    public class QuestRewards {
        #region Fields
        private readonly Quest _Quest;
        private QuestRewardItemGroup[] _Items;
        #endregion

        #region Properties
        public Quest Quest { get { return _Quest; } }
        public ClassJob ClassJob { get { return Quest.As<ClassJob>("ClassJob{Unlock}"); } }
        public int Gil { get { return Quest.AsInt32("GilReward"); } }
        public int ExpFactor { get { return Quest.AsInt32("ExpFactor"); } }
        public int GrandCompanySeals { get { return Quest.AsInt32("GCSeals"); } }
        public IEnumerable<QuestRewardItemGroup> Items { get { return _Items ?? (_Items = BuildItems()); } }
        public Emote Emote { get { return Quest.As<Emote>("Emote{Reward}"); } }
        public Action Action { get { return Quest.As<Action>("Action{Reward}"); } }
        public GeneralAction GeneralAction { get { return Quest.As<GeneralAction>("GeneralAction{Reward}"); } }
        public InstanceContent InstanceContent { get { return Quest.As<InstanceContent>("InstanceContent{Unlock}"); } }
        public int Reputation { get { return Quest.AsInt32("ReputationReward"); } }
        public QuestRewardOther QuestRewardOther { get { return Quest.As<QuestRewardOther>("Other{Reward}"); } }
        #endregion

        #region Constructors
        public QuestRewards(Quest quest) {
            _Quest = quest;
        }
        #endregion

        #region Build
        private QuestRewardItemGroup[] BuildItems() {
            const int CatalystCount = 3;
            const int Group1Count = 6;
            const int Group2Count = 5;

            var groupsType = Quest.AsInt32("ItemRewardType");
            var t1 = QuestRewardGroupType.Unknown;
            var t2 = QuestRewardGroupType.Unknown;
            switch (groupsType) {
                case 0:
                    return new QuestRewardItemGroup[0];
                case 1:
                    t1 = QuestRewardGroupType.All;
                    t2 = QuestRewardGroupType.One;
                    break;
                case 3: // Gender-specific rewards.
                case 7: // Beast rank bonuses.
                    // Special handler
                    break;
                case 5:
                    t1 = QuestRewardGroupType.OnePerDay;
                    t2 = QuestRewardGroupType.OnePerDay;
                    break;
                case 6:
                    // Relic quests
                    break;
            }

            var groups = new List<QuestRewardItemGroup>();

            var catalysts = BuildItemGroup(QuestRewardGroupType.All, "Item{Catalyst}", "ItemCount{Catalyst}", null, null, CatalystCount);
            groups.Add(catalysts);

            var tomestoneCount = Quest.AsInt32("TomestoneCount{Reward}");
            if (tomestoneCount > 0) {
                var tomestoneItem = Quest.As<Item>("Tomestone{Reward}");
                if (tomestoneItem != null)
                {
                    groups.Add(
                        new QuestRewardItemGroup(
                            new[] { new QuestRewardItem(tomestoneItem, tomestoneCount, null, false) },
                            QuestRewardGroupType.All));
                }
            }

            if (groupsType == 3) {
                {
                    var mItem = Quest.As<Item>("Item{Reward}[0]", 0);
                    var mCount = Quest.AsInt32("ItemCount{Reward}[0]", 0);
                    var mStain = Quest.As<Stain>("Stain{Reward}[0]", 0);

                    groups.Add(
                        new QuestRewardItemGroup(
                            new[] { new QuestRewardItem(mItem, mCount, mStain, false) },
                            QuestRewardGroupType.GenderSpecificMale));
                }
                {
                    var fItem = Quest.As<Item>("Item{Reward}[0]", 1);
                    var fCount = Quest.AsInt32("ItemCount{Reward}[0]", 1);
                    var fStain = Quest.As<Stain>("Stain{Reward}[0]", 1);

                    groups.Add(
                        new QuestRewardItemGroup(
                            new[] { new QuestRewardItem(fItem, fCount, fStain, false) },
                            QuestRewardGroupType.GenderSpecificFemale));
                }
            } else if (groupsType == 7) {
                var beastRankBonus = (XivRow)Quest.BeastTribe["BeastRankBonus"];
                var item = beastRankBonus.As<Item>();
                var counts = new List<int>();
                for (var i = 0; i < 8; i++)
                    counts.Add(beastRankBonus.AsInt32("Item{Quantity}", i));
                groups.Add(new QuestRewardItemGroup(new[] { new QuestRewardItem(item, counts.Distinct(), null, false) }, QuestRewardGroupType.BeastRankBonus));
            } else {
                groups.Add(BuildItemGroup(t1, "Item{Reward}[0]", "ItemCount{Reward}[0]", "Stain{Reward}[0]", null, Group1Count));
                groups.Add(BuildItemGroup(t2, "Item{Reward}[1]", "ItemCount{Reward}[1]", "Stain{Reward}[1]", "IsHQ{Reward}[1]", Group2Count));
            }

            return groups.Where(g => g.Items.Any()).ToArray();
        }
        private QuestRewardItemGroup BuildItemGroup(QuestRewardGroupType type, string itemPrefix, string countPrefix, string stainPrefix, string hqPrefix, int count) {
            var items = new List<QuestRewardItem>();

            for (var i = 0; i < count; ++i) {
                var itm = Quest.As<Item>(itemPrefix, i);
                var c = Quest.AsInt32(countPrefix, i);

                if (itm.Key == 0 || c == 0)
                    continue;

                Stain s = null;
                if (stainPrefix != null)
                    s = Quest.As<Stain>(stainPrefix, i);

                var isHq = false;
                if (hqPrefix != null)
                    isHq = Quest.AsBoolean(hqPrefix, i);

                items.Add(new QuestRewardItem(itm, c, s, isHq));
            }

            return new QuestRewardItemGroup(items, type);
        }
        #endregion
    }
}
