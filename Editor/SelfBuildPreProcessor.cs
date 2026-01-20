using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Purchasing;
using UnityEngine;
using UnityEngine.Purchasing;

public class SelfBuildPreProcessor : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    private bool isAmazon = false;

    public void OnPreprocessBuild(BuildReport report)
    {
#if UNITY_ANDROID
        //根据是否是亚马逊平台去添加删除 宏定义
        AddDefineSymbol();
        //自动根据当前的平台 切换商店
        RefreshAmazonStore();
        //获取当前的google-services.json去刷新Plugins/Android/FirebaseApp.androidlib/res/value/google-services.xml中的对应包名的firebase 数据
        RefreshFirebaseGoogleServiceXml();
        //添加或删除主androidmanifest.xml中amz内容
        AmazonAddOrRemoveXmlElement();
        //手动设置广告admob appid
#endif
    }
    
    private void RefreshFirebaseGoogleServiceXml()
    {
        //获取包名
        string appId = Application.identifier;
        //拿到Asset下google-service.json 进行解析
        string josnPath = Application.dataPath +"/google-services.json";
        dynamic dynamic = Newtonsoft.Json.JsonConvert.DeserializeObject(ReadXml(josnPath));

        string google_app_id = string.Empty;
        string default_android_client_id = string.Empty;
        string default_web_client_id=string.Empty;
        try
        {
            //获取 
            string text = dynamic["client"][0]["client_info"]["android_client_info"]["package_name"];
            Log.Info($"google-services:dynamic：{text}");
            if (text == appId)
            {
                google_app_id = dynamic["client"][0]["client_info"]["mobilesdk_app_id"];
                default_android_client_id=dynamic["client"][0]["oauth_client"][0]["client_id"];
                default_web_client_id=dynamic["client"][0]["oauth_client"][1]["client_id"];
            }
            else
            {
                google_app_id = dynamic["client"][1]["client_info"]["mobilesdk_app_id"];
                default_android_client_id=dynamic["client"][1]["oauth_client"][0]["client_id"];
                default_web_client_id=dynamic["client"][1]["oauth_client"][1]["client_id"];
            }
            Log.Info($"{appId}:" +
                     $"\n google_app_id:{google_app_id} " +
                     $"\n default_android_client_id:{default_android_client_id} " +
                     $"\n default_web_client_id:{default_web_client_id} ");
            
            //赋值
            string xmlPath = Application.dataPath + "/Plugins/Android/FirebaseApp.androidlib/res/values/google-services.xml";
            WriteToxml(xmlPath,"google_app_id",google_app_id);
            WriteToxml(xmlPath,"default_android_client_id",default_android_client_id);
            WriteToxml(xmlPath,"default_web_client_id",default_web_client_id);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void AddDefineSymbol()
    {
        string amzSymbol = "AmazonStore";
        string appId = Application.identifier;
        if (appId.Contains("amz"))
        {
            isAmazon = true;
        }
        else
        {
            isAmazon = false;
        }

        var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
        char DEFINE_SEPARATOR = ';';
        if (symbols != null)
        {
            var defines = symbols.Split(DEFINE_SEPARATOR).ToList();
            if (isAmazon&&!defines.Contains(amzSymbol))
            {
                defines.Add(amzSymbol);
                PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Android,string.Join(DEFINE_SEPARATOR.ToString(),defines.ToArray()));
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }else if (!isAmazon&&defines.Contains(amzSymbol))
            {
                defines.Remove(amzSymbol);
                PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Android,string.Join(DEFINE_SEPARATOR.ToString(),defines.ToArray()));
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
    }

    private void RefreshAmazonStore()
    {
        UnityPurchasingEditor.TargetAndroidStore(isAmazon?AppStore.AmazonAppStore:AppStore.GooglePlay);
    }
    
    public string ReadXml(string xmlPath)
    {
        string readData;
        //读取文件
        using (StreamReader sr =File.OpenText(xmlPath))
        {
            //数据保存
            readData = sr.ReadToEnd();
            sr.Close();
        }
        //返回数据
        return readData;
    }
    
    public void WriteToxml(string xmlPath,string nodeName,string nodeValue)
    {
        XmlDocument xml = new XmlDocument();
        xml.Load(xmlPath);

        bool isSave = false;
        XmlNodeList xmlNodeList = xml.SelectSingleNode("resources").ChildNodes;
        foreach (XmlElement tempNode in xmlNodeList)
        {
            if (tempNode.GetAttribute("name")==nodeName&&tempNode.InnerText!=nodeValue)
            {
                isSave = true;
                tempNode.InnerText = nodeValue;
                Log.Info($"{nodeName}:{nodeValue}");
            }
        }

        if (isSave)
        {
            xml.Save(xmlPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    
    private const string MANIFEST_RELATIVE_PATH ="Plugins/Android/AndroidManifest.xml";
    private XNamespace ns = "http://schemas.android.com/apk/res/android";
    private void AmazonAddOrRemoveXmlElement()
    {
        string manifestPath = Path.Combine(Application.dataPath, MANIFEST_RELATIVE_PATH);
        var manifest = XDocument.Load(manifestPath);
        XElement elemManifest = manifest.Element("manifest");
        XElement elemApplication = elemManifest.Element("application");
        IEnumerable<XElement> metas = elemApplication.Descendants()
            .Where( elem => elem.Name.LocalName.Equals("meta-data"));
        SetMetadataElement(elemApplication, metas, "CHANNEL", "Amazon",!isAmazon);
        elemManifest.Save(manifestPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    
    private void SetMetadataElement(XElement elemApplication,
        IEnumerable<XElement> metas,
        string metadataName,
        string metadataValue,
        bool isRemove)
    {
        XElement element = GetMetaElement(metas, metadataName);

        if (isRemove)
        {
            if(element!=null)element.Remove();
            return;
        }

        if (element == null)
        {
            elemApplication.Add(CreateMetaElement(metadataName, metadataValue));
        }
        else
        {
            element.SetAttributeValue(ns + "value", metadataValue);
        }
    }
    
    private XElement GetMetaElement(IEnumerable<XElement> metas, string metaName)
    {
        foreach (XElement elem in metas)
        {
            IEnumerable<XAttribute> attrs = elem.Attributes();
            foreach (XAttribute attr in attrs)
            {
                if (attr.Name.Namespace.Equals(ns)
                    && attr.Name.LocalName.Equals("name") && attr.Value.Equals(metaName))
                {
                    return elem;
                }
            }
        }
        return null;
    }
    
    private XElement CreateMetaElement(string name, object value)
    {
        return new XElement("meta-data",
            new XAttribute(ns + "name", name), new XAttribute(ns + "value", value));
    }
}
