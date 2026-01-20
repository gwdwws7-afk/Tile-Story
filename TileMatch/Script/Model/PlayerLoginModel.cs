namespace MySelf.Model
{
    public enum LoginType
    {
        None,
        Facebook,
        Google,
        Apple,
    }

    public class PlayerLoginData
    {
        public string UserID;
        public string UserName;
        public LoginType LoginSdkNameType;//google or facebook
        public string RecordLastSaveDataTime = string.Empty;
        public bool IsHaveShowLoginGuide=false;
        public bool IsHaveShowSaveDataGuide = false;
        public int NeedLevelShowGuide = 2;
    }

    public class PlayerLoginModel : BaseModel<PlayerLoginModel, PlayerLoginData>
    {
        public void SetUserID(string id)
        {
            Data.UserID = id;
            SaveToLocal();
        }

        public void SetUserName(string userName)
        {
            Data.UserName = userName;
            SaveToLocal();
        }

        public void RecordSaveDataTime()
        {
            Data.RecordLastSaveDataTime = System.DateTime.Now.ToString();
            SaveToLocal();
        }
    }
}