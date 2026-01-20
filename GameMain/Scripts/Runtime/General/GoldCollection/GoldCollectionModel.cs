using System;

namespace MySelf.Model
{
    public class GoldCollectionData
    {
        public int activityID = -1;
        public int currentIndex;
        public int totalCollectNum;
        public int lastRecordTotalCollectNum;
        public int levelCollectNum;
        public DateTime endTime;
        public bool showedFirstMenu;
        public bool showedLastMenu;
    }

    public class GoldCollectionModel : BaseModel<GoldCollectionModel, GoldCollectionData>
    {
        public int ActivityID
        {
            get => Data.activityID;
            set
            {
                if (Data.activityID != value)
                {
                    Data.activityID = value;
                    SaveToLocal();
                }
            }
        }
        
        public int CurrentIndex
        {
            get => Data.currentIndex;
            set
            {
                if (Data.currentIndex != value)
                {
                    Data.currentIndex = value;
                    SaveToLocal();
                }
            }
        }

        public int TotalCollectNum
        {
            get => Data.totalCollectNum;
            set
            {
                if (Data.totalCollectNum != value)
                {
                    Data.totalCollectNum = value;
                    SaveToLocal();
                }
            }
        }

        public int LastRecordTotalCollectNum
        {
            get => Data.lastRecordTotalCollectNum;
            set
            {
                if (Data.lastRecordTotalCollectNum != value)
                {
                    Data.lastRecordTotalCollectNum = value;
                    SaveToLocal();
                }
            }
        }

        public int LevelCollectNum
        {
            get => Data.levelCollectNum;
            set
            {
                if (Data.levelCollectNum != value)
                {
                    Data.levelCollectNum = value;
                    SaveToLocal();
                }
            }
        }

        public DateTime EndTime
        {
            get => Data.endTime;
            set
            {
                if (Data.endTime != value)
                {
                    Data.endTime = value;
                    SaveToLocal();
                }
            }
        }

        public bool ShowedFirstMenu
        {
            get => Data.showedFirstMenu;
            set
            {
                if (Data.showedFirstMenu != value)
                {
                    Data.showedFirstMenu = value;
                    SaveToLocal();
                }
            }
        }

        public bool ShowedLastMenu
        {
            get => Data.showedLastMenu;
            set
            {
                if (Data.showedLastMenu != value)
                {
                    Data.showedLastMenu = value;
                    SaveToLocal();
                }
            }
        }
    }
}
