using GameFramework;
using GameFramework.Setting;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 游戏配置模块
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Setting")]
    public sealed class SettingComponent : GameFrameworkComponent
    {
        private const string DefaultDateTimeFormet = "yyyy-MM-dd HH:mm:ss";
        private DateTime m_MinDateTime = new DateTime(2000, 1, 1);

        private ISettingManager m_SettingManager = null;

        [SerializeField]
        private string m_SettingHelperTypeName = "UnityGameFramework.Runtime.PlayerPrefsSettingHelper";

        /// <summary>
        /// 游戏配置项数量
        /// </summary>
        public int Count
        {
            get
            {
                return m_SettingManager.Count;
            }
        }

        /// <summary>
        /// 默认最小日期
        /// </summary>
        public DateTime MinDateTime
        {
            get
            {
                return m_MinDateTime;
            }
        }

        /// <summary>
        /// 游戏框架组件初始化
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_SettingManager = GameFrameworkEntry.GetModule<SettingManager>();
            if (m_SettingManager == null)
            {
                Log.Fatal("Setting manager is invalid.");
                return;
            }

            ISettingHelper settingHelper = new PlayerPrefsSettingHelper();

            m_SettingManager.SetSettingHelper(settingHelper);
        }

        private void Start()
        {
            if (!m_SettingManager.Load())
            {
                Log.Error("Load settings failure.");
            }
        }

        /// <summary>
        /// 保存游戏配置
        /// </summary>
        public void Save()
        {
            m_SettingManager.Save();
        }

        /// <summary>
        /// 获取所有游戏配置项的名称
        /// </summary>
        /// <returns>所有游戏配置项的名称</returns>
        public string[] GetAllSettingNames()
        {
            return m_SettingManager.GetAllSettingNames();
        }

        /// <summary>
        /// 获取所有游戏配置项的名称
        /// </summary>
        /// <param name="results">所有游戏配置项的名称</param>
        public void GetAllSettingNames(List<string> results)
        {
            m_SettingManager.GetAllSettingNames(results);
        }

        /// <summary>
        /// 检查是否存在指定游戏配置项
        /// </summary>
        /// <param name="settingName">要检查游戏配置项的名称</param>
        /// <returns>指定的游戏配置项是否存在</returns>
        public bool HasSetting(string settingName)
        {
            return m_SettingManager.HasSetting(settingName);
        }

        /// <summary>
        /// 移除指定游戏配置项
        /// </summary>
        /// <param name="settingName">要移除游戏配置项的名称</param>
        public void RemoveSetting(string settingName)
        {
            m_SettingManager.RemoveSetting(settingName);
        }

        /// <summary>
        /// 清空所有游戏配置项
        /// </summary>
        public void RemoveAllSettings()
        {
            m_SettingManager.RemoveAllSettings();
        }

        /// <summary>
        /// 从指定游戏配置项中读取布尔值
        /// </summary>
        /// <param name="settingName">要获取游戏配置项的名称</param>
        /// <returns>读取的布尔值</returns>
        public bool GetBool(string settingName)
        {
            return m_SettingManager.GetBool(settingName);
        }

        /// <summary>
        /// 从指定游戏配置项中读取布尔值
        /// </summary>
        /// <param name="settingName">要获取游戏配置项的名称</param>
        /// <param name="defaultValue">当指定的游戏配置项不存在时，返回此默认值</param>
        /// <returns>读取的布尔值</returns>
        public bool GetBool(string settingName, bool defaultValue)
        {
            return m_SettingManager.GetBool(settingName, defaultValue);
        }

        /// <summary>
        /// 向指定游戏配置项写入布尔值
        /// </summary>
        /// <param name="settingName">要写入游戏配置项的名称</param>
        /// <param name="value">要写入的布尔值</param>
        public void SetBool(string settingName, bool value)
        {
            m_SettingManager.SetBool(settingName, value);
        }

        /// <summary>
        /// 从指定游戏配置项中读取整数值
        /// </summary>
        /// <param name="settingName">要获取游戏配置项的名称</param>
        /// <returns>读取的整数值</returns>
        public int GetInt(string settingName)
        {
            return m_SettingManager.GetInt(settingName);
        }

        /// <summary>
        /// 从指定游戏配置项中读取整数值
        /// </summary>
        /// <param name="settingName">要获取游戏配置项的名称</param>
        /// <param name="defaultValue">当指定的游戏配置项不存在时，返回此默认值</param>
        /// <returns>读取的整数值</returns>
        public int GetInt(string settingName, int defaultValue)
        {
            return m_SettingManager.GetInt(settingName, defaultValue);
        }

        /// <summary>
        /// 向指定游戏配置项写入整数值
        /// </summary>
        /// <param name="settingName">要写入游戏配置项的名称</param>
        /// <param name="value">要写入的整数值</param>
        public void SetInt(string settingName, int value)
        {
            m_SettingManager.SetInt(settingName, value);
        }

        /// <summary>
        /// 从指定游戏配置项中读取浮点数值
        /// </summary>
        /// <param name="settingName">要获取游戏配置项的名称</param>
        /// <returns>读取的浮点数值</returns>
        public float GetFloat(string settingName)
        {
            return m_SettingManager.GetFloat(settingName);
        }

        /// <summary>
        /// 从指定游戏配置项中读取浮点数值
        /// </summary>
        /// <param name="settingName">要获取游戏配置项的名称</param>
        /// <param name="defaultValue">当指定的游戏配置项不存在时，返回此默认值</param>
        /// <returns>读取的浮点数值</returns>
        public float GetFloat(string settingName, float defaultValue)
        {
            return m_SettingManager.GetFloat(settingName, defaultValue);
        }

        /// <summary>
        /// 向指定游戏配置项写入浮点数值
        /// </summary>
        /// <param name="settingName">要写入游戏配置项的名称</param>
        /// <param name="value">要写入的浮点数值</param>
        public void SetFloat(string settingName, float value)
        {
            m_SettingManager.SetFloat(settingName, value);
        }

        /// <summary>
        /// 从指定游戏配置项中读取字符串值
        /// </summary>
        /// <param name="settingName">要获取游戏配置项的名称</param>
        /// <returns>读取的字符串值</returns>
        public string GetString(string settingName)
        {
            return m_SettingManager.GetString(settingName);
        }

        /// <summary>
        /// 从指定游戏配置项中读取字符串值
        /// </summary>
        /// <param name="settingName">要获取游戏配置项的名称</param>
        /// <param name="defaultValue">当指定的游戏配置项不存在时，返回此默认值</param>
        /// <returns>读取的字符串值</returns>
        public string GetString(string settingName, string defaultValue)
        {
            return m_SettingManager.GetString(settingName, defaultValue);
        }

        /// <summary>
        /// 向指定游戏配置项写入字符串值
        /// </summary>
        /// <param name="settingName">要写入游戏配置项的名称</param>
        /// <param name="value">要写入的字符串值</param>
        public void SetString(string settingName, string value)
        {
            m_SettingManager.SetString(settingName, value);
        }

        /// <summary>
        /// 从指定游戏配置项中读取日期
        /// </summary>
        /// <param name="settingName">要获取游戏配置项的名称</param>
        /// <param name="value">读取的日期，默认值为DateTime(2000, 1, 1)</param>
        /// <returns>是否读取日期成功</returns>
        public bool GetDateTime(string settingName, out DateTime value)
        {
            string saveValue = GetString(settingName);
            if (string.IsNullOrEmpty(saveValue) || !DateTime.TryParse(saveValue, out value) || value == m_MinDateTime)
            {
                value = m_MinDateTime;
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 向指定游戏配置项写入日期
        /// </summary>
        /// <param name="settingName">要写入游戏配置项的名称</param>
        /// <param name="value">写入的日期，最低日期为DateTime(2000, 1, 1)</param>
        public void SetDateTime(string settingName, DateTime value)
        {
            if (value < m_MinDateTime)
            {
                value = m_MinDateTime;
            }

            SetString(settingName, value.ToString(DefaultDateTimeFormet));
        }
    }
}