using System.Collections;
using System.Collections.Generic;

public enum NotificationKey
{
    None,
    TileMatch_Normal,
    TileMatch_TurnTable,
    TileMatch_PersonRank_StartOrOngoing,
    TileMatch_PersonRank_Finished,
    //后续往这里加


    //再后面的是不直接配置 而是通过展开的随机Term
    Normal_RandomTerm_1 = 101,
    Normal_RandomTerm_2 = 102,
    Normal_RandomTerm_3 = 103,
    Normal_RandomTerm_4 = 104,
    Normal_RandomTerm_5 = 105,
    Normal_RandomTerm_6 = 106,
    Normal_RandomTerm_7 = 107,
    Normal_RandomTerm_8 = 108,
    Normal_RandomTerm_9 = 109,
    Normal_RandomTerm_10 = 110,
    Normal_RandomTerm_11 = 111,
    Normal_RandomTerm_12 = 112,

    TurnTable_RandomTerm_1 = 201,
    TurnTable_RandomTerm_2 = 202,

    PersonRank_Start = 301,
    PersonRank_OnGoing = 302,
    
    Merge_1,
    Merge_2,
    Merge_3,

    Christmas_1,
}
