﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaintCoinach.Xiv {
    public enum QuestRewardGroupType {
        Unknown,
        All,
        One,
        GenderSpecificMale,
        GenderSpecificFemale,
        OnePerDay,       // Used by 2014 Starlight
        BeastRankBonus
    }
}
