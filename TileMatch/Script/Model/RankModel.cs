using System.Collections;
using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;

namespace MySelf.Model
{
    public interface IRankModelBase
    {
        RankType RankType { get; }
        int RankTerm { get; set; }
    }
}

public abstract class BaseRankModelData
{
    public string Name { get; set; }
    public int Score { get; set; }
    public string SerialNum { get; set; }
    public int HeadId { get; set; }

    public bool IsNoShowInGroup { get; set; }

    public abstract bool IsSelf();
}

public enum RankType
{
    None,
    PersonRank
}

