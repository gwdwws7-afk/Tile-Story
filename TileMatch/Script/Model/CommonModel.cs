
using System;
using System.Collections.Generic;

namespace MySelf.Model
{
	public class CommonModelData
	{
		public bool NewPlayer=true;
		public bool NewPlayerForGoldCollection = true;
		public bool HasOpenDebug;

		public bool IsAudioMuted;
		public bool IsMusicMuted;
		public bool IsShakeMuted;
		public bool IsTurnOffTips = true;
		public bool IsKidMode;

		public int Music=1;
		public int HeadPortrait;
		public string Language;
		public string PlayerName;
		public bool IsSetPlayerName=false;
		public int AccumulatedLoginDays;

		public bool IsShowSettingRedPoint=true;
		public bool IsShowHeadPortraitRedPoint=true;
		public bool IsShowChangeImageRedPoint = true;
		public bool IsShowChangeImageToggleRedPoint = true;

		public string OnePlusOnePackType = string.Empty;
		public int OnePlusTwoPackGetRewardTime = 0;

		public int ChainPackStage = 0;

		public int BgMusicVolume = 100;

		public bool IsChangeBgImage6 = false;
	}
	public class CommonModel:BaseModelService<CommonModel, CommonModelData>
	{
		#region Service
		public override Dictionary<string, object> GetNeedSaveToServiceDictAllModelVersion()
		{
			return new Dictionary<string, object>()
			{
				{"CommonModel.PlayerName",Data.PlayerName},
				{"CommonModel.HeadPortrait",Data.HeadPortrait},
				{"CommonModel.OnePlusOnePackType", Data.OnePlusOnePackType},
				{"CommonModel.OnePlusTwoPackGetRewardTime",Data.OnePlusTwoPackGetRewardTime },
				{"CommonModel.ChainPackStage",Data.ChainPackStage },
			};
		}

		public override void SaveServiceDataToLocalAllModelVersion(Dictionary<string, object> dictionary)
		{
			if (dictionary != null)
			{
				foreach (var item in dictionary)
				{
					switch (item.Key)
					{
						case "CommonModel.PlayerName":
							Data.PlayerName = Convert.ToString(item.Value);
							break;
						case "CommonModel.HeadPortrait":
							Data.HeadPortrait = Convert.ToInt32(item.Value);
							break;
						case "CommonModel.OnePlusOnePackType":
							Data.OnePlusOnePackType = Convert.ToString(item.Value);
							break;
						case "CommonModel.OnePlusTwoPackGetRewardTime":
							Data.OnePlusTwoPackGetRewardTime = Convert.ToInt32(item.Value);
							break;
						case "CommonModel.ChainPackStage":
							Data.ChainPackStage = Convert.ToInt32(item.Value);
							break;
					}
				}
				SaveToLocal();
			}
		}
		#endregion

		public void SetNewPlayer(bool isNewPlayer)
		{
			if (Data.NewPlayer == isNewPlayer) return;

			Data.NewPlayer = isNewPlayer;
			SaveToLocal();
		}

        public void SetNewPlayerForGoldCollection(bool isNewPlayer)
        {
            if (Data.NewPlayerForGoldCollection == isNewPlayer) return;

            Data.NewPlayerForGoldCollection = isNewPlayer;
            SaveToLocal();
        }

        public void SetOpenDebug(bool isOpenDebug)
		{
			if (Data.HasOpenDebug == isOpenDebug) return;

			Data.HasOpenDebug = isOpenDebug;
			SaveToLocal();
		}

		public void SetLanguage(string language)
		{
			if (Data.Language == language) return;

			Data.Language = language;
			SaveToLocal();
		}

		public void SetAudioMuted(bool isMuted)
		{
			if (Data.IsAudioMuted == isMuted) return;

			Data.IsAudioMuted = isMuted;
			SaveToLocal();
		}
		public void SetMusicMuted(bool isMuted)
		{
			if (Data.IsMusicMuted == isMuted) return;

			Data.IsMusicMuted = isMuted;
			SaveToLocal();
		}
		public void SetShakeMuted(bool isMuted)
		{
			if (Data.IsShakeMuted == isMuted) return;

			Data.IsShakeMuted = isMuted;
			SaveToLocal();
		}
		public void SetTurnOffTips(bool isTurnOff)
        {
			if (Data.IsTurnOffTips == isTurnOff) return;

			Data.IsTurnOffTips = isTurnOff;
			SaveToLocal();
		}
		public void SetKidMuted(bool isMuted)
		{
			if (Data.IsKidMode == isMuted) return;

			Data.IsKidMode = isMuted;
			SaveToLocal();
		}

		public void SetMusicIndex(int musicIndex)
		{
			if (Data.Music == musicIndex) return;

			Data.Music = musicIndex;
			SaveToLocal();
		}

		public void SetAccumulatedLoginDays(int days)
        {
			if (Data.AccumulatedLoginDays == days) return;

			Data.AccumulatedLoginDays = days;
			SaveToLocal();
		}

		public void SetHeadPortrait(int headPortraitIndex)
		{
			if (Data.HeadPortrait == headPortraitIndex) return;

			Data.HeadPortrait = headPortraitIndex;
			SaveToLocal();
		}

		public void SetPlayerName(string playerName)
		{
			if (Data.PlayerName == playerName) return;

			Data.PlayerName = playerName;
			SaveToLocal();
		}

		public void RecordPlayerNameInput()
		{
			if(Data.IsSetPlayerName)return;
			Data.IsSetPlayerName = true;
			SaveToLocal();
		}

		public void SetIsShowSettingRedPoint(bool isShowSettingRedPoint)
		{
			if (Data.IsShowSettingRedPoint == isShowSettingRedPoint) return;

			Data.IsShowSettingRedPoint = isShowSettingRedPoint;
			SaveToLocal();
		}

		public void SetIsShowHeadPortraitRedPoint(bool isShowHeadPortraitRedPoint)
		{
			if (Data.IsShowHeadPortraitRedPoint == isShowHeadPortraitRedPoint) return;

			Data.IsShowHeadPortraitRedPoint = isShowHeadPortraitRedPoint;
			SaveToLocal();
		}

		public void SetIsShowChangeImageRedPoint(bool isShowChangeImageRedPoint)
		{
			if (Data.IsShowChangeImageRedPoint == isShowChangeImageRedPoint) return;

			Data.IsShowChangeImageRedPoint = isShowChangeImageRedPoint;
			SaveToLocal();
		}

		public void SetIsShowChangeImageToggleRedPoint(bool isShowChangeImageToggleRedPoint)
		{
			if (Data.IsShowChangeImageToggleRedPoint == isShowChangeImageToggleRedPoint) return;

			Data.IsShowChangeImageToggleRedPoint = isShowChangeImageToggleRedPoint;
			SaveToLocal();
		}

		public void SetOnePlusOnePackType(string onePlusOnePackType)
		{
			if (Data.OnePlusOnePackType == onePlusOnePackType) return;

			Data.OnePlusOnePackType = onePlusOnePackType;
			SaveToLocal();
		}

		public void SetOnePlusTwoPackGetRewardTime(int time)
        {
			if (Data.OnePlusTwoPackGetRewardTime == time) return;

			Data.OnePlusTwoPackGetRewardTime = time;
			SaveToLocal();
		}

		public void SetChainPackStage(int stage)
        {
			if (Data.ChainPackStage == stage) return;

			Data.ChainPackStage = stage;
			SaveToLocal();
        }

		public void SetBgMusicVolume(int volume)
		{
			if(Data.BgMusicVolume==volume)return;
			
			Data.BgMusicVolume = volume;
			SaveToLocal();
		}
	}
}

