using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;

public enum ImageFormatLevel
{
    Original,
    Best,//ETC2_RGBA8Crunched 、ASTC_RGBA_6x6        IOS:ASTC_RGBA_4x4
    Good,//ETC2_RGBA8Crunched 、ASTC_RGBA_8x8        IOS:ASTC_RGBA_6x6
    Normal,//ETC2_RGBA8Crunched 、ASTC_RGBA_10x10    IOS:ASTC_RGBA_8x8
    Low,//ETC2_RGBA8Crunched 、ASTC_RGBA_12x12       IOS:ASTC_RGBA_10x10
    ETC,//Crunched ETC
}

public enum ImageFromatPlatType
{
    Android,
    iPhone,
    AndroidAndiPhone,
}

public class ImageFormatSettingTools
{
    [MenuItem("BubbleTools/图片处理工具/统一处理")]
    public static void SetAllTex()
    {
        SetEffectTex();//特效贴图
        SetSpine();//设置Spin格式
    }

    /// <summary>
    /// 特效贴图
    /// </summary>
    [MenuItem("BubbleTools/图片处理工具/处理特效贴图")]
    public static void SetEffectTex()
    {
         List<string> effectPaths = FindEffectFiles(Application.dataPath + "/Effects");
        foreach (var path in effectPaths)
        {
            string shortPath = path.Replace(Application.dataPath, "");
            SetTextureFormatNew(shortPath, false, true, ImageFormatLevel.Good, isNonPower: true,maxSize:256);
        }
        SetTextureFormatNew("/Effects/Textures/", false, true, ImageFormatLevel.Good, isNonPower: true,maxSize:256);
        SetTextureFormatNew("/Effects/UI/", false, true, ImageFormatLevel.Good, isNonPower: true, maxSize: 256);
        SetTextureFormatNew("/RaccoonRescue/Effects/", false, true, ImageFormatLevel.Good, isNonPower: true, maxSize: 256);
        SetTextureFormatNew("/Merge/Effects/", false, true, ImageFormatLevel.Good, isNonPower: true, maxSize: 256);
        //SetTextureFormatNew("/GameMain/Textures/Decoration", false, true, ImageFormatLevel.Good, isNonPower: true, maxSize: 1024);
        //SetTextureFormatNew("/TileMatch/Res/UI/NoAtlas_UI", false, true, ImageFormatLevel.Good, isNonPower: true, maxSize: 1024);
    }
    /// <summary>
    /// Spine
    /// </summary>
    [MenuItem("BubbleTools/图片处理工具/spine")]
    public static void SetSpine()
    {
        SetTextureFormatNew("/Effects/Spines/", false, true, imageFormatLevel:ImageFormatLevel.ETC, isNonPower: true);
        //SetTextureFormatNew("/Effects/Gems/", false, true, imageFormatLevel:ImageFormatLevel.ETC, isNonPower: true);
    }

    static bool isHaveSetting = false;
    private static void SetTextureFormatNew(
        string path,
        bool isSprite,
        bool isJudgePowerOf4 = true, 
        ImageFormatLevel imageFormatLevel = ImageFormatLevel.Best,
        bool isNonPower = false, 
        int maxSize = 2048,
        bool IsHaveAlpha = false)
    {
        string[] allFiles = GetFileNames(Application.dataPath + path).ToArray();
        if (allFiles != null)
        {
            Texture2D texture2D;
            TextureImporter textureImporter;
            bool isPowerOf4;
            bool isPowerOf2;
            bool isMultipleOf4;

            for (int i = 0; i < allFiles.Length; i++)
            {
                EditorUtility.DisplayProgressBar(path, $"进度...({i}/{allFiles.Length})", (float)i / (float)allFiles.Length);
                var child = allFiles[i];

                if (!IsTextureFile(child)) continue;
                string strTempPath = child.Replace(@"\", "/");
                strTempPath = strTempPath.Substring(strTempPath.IndexOf("Assets"));

                texture2D = (Texture2D)AssetDatabase.LoadAssetAtPath<Texture2D>(strTempPath);
                if (texture2D == null) continue;

                textureImporter = AssetImporter.GetAtPath(strTempPath) as TextureImporter;
                if (textureImporter == null) continue;

                isHaveSetting = true;

                int width;
                int height;
                GetTextureOriginalSize(textureImporter, out width, out height);

                isMultipleOf4 = width % 4 == 0 && height % 4 == 0;
                isPowerOf2 = IsPowerOf2(width)&&IsPowerOf2(height);
                bool HaveAlpha = IsHaveAlpha ? true : textureImporter.DoesSourceTextureHaveAlpha();

                TextureImporterFormat alpha;
                TextureImporterFormat noAlpha;
                GetImageFormatLevel(imageFormatLevel, isMultipleOf4, out alpha, out noAlpha);
                SetTextureFormat(textureImporter, isSprite, HaveAlpha, alpha, noAlpha, ImageFromatPlatType.iPhone, 50, isNonPower, maxSize);

                string contentStr = "";
                if (!isHaveSetting) contentStr += "iPhone";
                if (isNonPower || isMultipleOf4)
                {
                    alpha = TextureImporterFormat.ETC2_RGBA8Crunched;
                    noAlpha = TextureImporterFormat.ETC2_RGBA8Crunched;
                    if (isPowerOf2 || (isNonPower && !isSprite))
                        noAlpha = TextureImporterFormat.ETC_RGB4Crunched;
                }
                SetTextureFormat(textureImporter, isSprite, HaveAlpha, alpha, noAlpha, ImageFromatPlatType.Android, 50, isNonPower, maxSize);
                if (!isHaveSetting) contentStr += "|Android";
                if (!isHaveSetting)
                    Debug.Log($"{contentStr}\n Change Image path \n:{strTempPath}");

                if (!isHaveSetting)
                    AssetDatabase.ImportAsset(strTempPath);
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
        }
    }
    public static void GetTextureOriginalSize(TextureImporter ti, out int width, out int height)
    {
        if (ti == null)
        {
            width = 0;
            height = 0;
            return;
        }

        object[] args = new object[2] { 0, 0 };
        MethodInfo mi = typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
        mi.Invoke(ti, args);

        width = (int)args[0];
        height = (int)args[1];
    }

    private static void SetTextureFormat(
        TextureImporter importer,
        bool isSprite,
        bool isInputTextureAlpha,
        TextureImporterFormat alphaTexFormat,
        TextureImporterFormat noAlphaTexFormat,
        ImageFromatPlatType type,
        int compressorQuality = 100,
        bool isNonPower = false,
        int maxSize = 2048,
        TextureImporterAlphaSource alphaSource = TextureImporterAlphaSource.None,
        SpriteImportMode spriteMode = SpriteImportMode.None)
    {
        bool alphaSourceFormat = true;
        bool spriteModeFormat = true;
        bool nonPower = true;
        if (spriteMode != SpriteImportMode.None && importer.spriteImportMode != spriteMode)
        {
            spriteModeFormat = false;
        }
        if (alphaSource != TextureImporterAlphaSource.None && importer.alphaSource != alphaSource)
        {
            alphaSourceFormat = false;
        }

        if (importer.npotScale != (!isNonPower || isSprite ? TextureImporterNPOTScale.None : TextureImporterNPOTScale.ToNearest))
        {
            nonPower = false;
        }

        if (nonPower && spriteModeFormat && alphaSourceFormat && IsHaveSetting(importer, isSprite, isInputTextureAlpha, alphaTexFormat, noAlphaTexFormat, type, maxSize))
        {
            return;
        }

        isHaveSetting = false;

        importer.textureType = isSprite ? TextureImporterType.Sprite : TextureImporterType.Default;
        importer.maxTextureSize = maxSize;
        importer.alphaSource = alphaSource == TextureImporterAlphaSource.None? (isInputTextureAlpha ? TextureImporterAlphaSource.FromInput : TextureImporterAlphaSource.None) : alphaSource;
        importer.mipmapEnabled = false;
        importer.compressionQuality = compressorQuality;
        if (spriteMode != SpriteImportMode.None)
        {
            importer.spriteImportMode = spriteMode;
        }
        importer.alphaIsTransparency = isInputTextureAlpha;
        importer.npotScale = !isNonPower || isSprite ? TextureImporterNPOTScale.None : TextureImporterNPOTScale.ToNearest;
        var format = isInputTextureAlpha ? alphaTexFormat : noAlphaTexFormat;
        if (isSprite)
        {
            importer.filterMode = FilterMode.Bilinear;
        }
        //importer.SetPlatformTextureSettings(SetPlatformSetting("Android", format, compressorQuality, isInputTextureAlpha, maxSize));
        //importer.SetPlatformTextureSettings(SetPlatformSetting("iPhone", format, compressorQuality, isInputTextureAlpha, maxSize));
        if (type == ImageFromatPlatType.Android)
        {
            importer.SetPlatformTextureSettings(SetPlatformSetting("Android", format, compressorQuality, isInputTextureAlpha, maxSize));
        }
        else if (type == ImageFromatPlatType.iPhone)
        {
            importer.SetPlatformTextureSettings(SetPlatformSetting("iPhone", format, compressorQuality, isInputTextureAlpha, maxSize));
        }
        else
        {
            importer.SetPlatformTextureSettings(SetPlatformSetting("Android", format, compressorQuality, isInputTextureAlpha, maxSize));
            importer.SetPlatformTextureSettings(SetPlatformSetting("iPhone", format, compressorQuality, isInputTextureAlpha, maxSize));
        }
    }

    private static TextureImporterPlatformSettings SetPlatformSetting(string platformName, TextureImporterFormat format, int compressionQuality, bool isInputTexAlpha, int maxSize)
    {

        TextureImporterPlatformSettings setting = new TextureImporterPlatformSettings();
        setting.name = platformName;
        setting.overridden = true;
        setting.maxTextureSize = maxSize;
        setting.format = format;
        setting.compressionQuality = compressionQuality;
        setting.crunchedCompression = true;
        setting.allowsAlphaSplitting = isInputTexAlpha;
        setting.androidETC2FallbackOverride = AndroidETC2FallbackOverride.UseBuildSettings;
        return setting;
    }

    private static bool IsHaveSetting(TextureImporter importer, bool isSprite, bool isAlpha,
        TextureImporterFormat alphaTexFormat, TextureImporterFormat noAlphaTexFormat, ImageFromatPlatType type,int size)
    {
        int maxSize;
        TextureImporterFormat textureImporter;
        if (type == ImageFromatPlatType.Android)
        {
            var isHavePlatform = importer.GetPlatformTextureSettings(type.ToString(), out maxSize, out textureImporter);
            return (isHavePlatform
                && textureImporter == (isAlpha ? alphaTexFormat : noAlphaTexFormat)
                && importer.textureType == (isSprite ? TextureImporterType.Sprite : TextureImporterType.Default)
                && maxSize==size);
        }
        else if (type == ImageFromatPlatType.iPhone)
        {
            var isHavePlatform = importer.GetPlatformTextureSettings(type.ToString(), out maxSize, out textureImporter);
            return (isHavePlatform 
                && textureImporter == (isAlpha ? alphaTexFormat : noAlphaTexFormat) 
                && importer.textureType == (isSprite ? TextureImporterType.Sprite : TextureImporterType.Default)
                && maxSize == size);
        }
        else
        {
            var isHavePlatform = importer.GetPlatformTextureSettings(ImageFromatPlatType.Android.ToString(), out maxSize, out textureImporter);
            if (isHavePlatform && textureImporter == (isAlpha ? alphaTexFormat : noAlphaTexFormat) && importer.textureType == (isSprite ? TextureImporterType.Sprite : TextureImporterType.Default))
            {
                isHavePlatform = importer.GetPlatformTextureSettings(ImageFromatPlatType.iPhone.ToString(), out maxSize, out textureImporter);
                return (isHavePlatform 
                    && textureImporter == (isAlpha ? alphaTexFormat : noAlphaTexFormat) 
                    && importer.textureType == (isSprite ? TextureImporterType.Sprite : TextureImporterType.Default)
                    && maxSize == size);
            }
        }
        return false;
    }

    private static void GetImageFormatLevel(ImageFormatLevel imageFormatLevel, bool isMultipleOf4, out TextureImporterFormat alpha, out TextureImporterFormat noAlpha)
    {
        alpha = TextureImporterFormat.RGBA32;
        noAlpha = TextureImporterFormat.RGB24;
        switch (imageFormatLevel)
        {
            case ImageFormatLevel.Low:
#if UNITY_ANDROID
                alpha = TextureImporterFormat.ASTC_12x12;
                noAlpha = TextureImporterFormat.ASTC_12x12;
#else
                alpha = TextureImporterFormat.ASTC_10x10;
                noAlpha = TextureImporterFormat.ASTC_10x10;
#endif
                break;
            case ImageFormatLevel.Normal:
#if UNITY_ANDROID
                alpha = TextureImporterFormat.ASTC_10x10;
                noAlpha = TextureImporterFormat.ASTC_10x10;
#else
                alpha = TextureImporterFormat.ASTC_8x8;
                noAlpha = TextureImporterFormat.ASTC_8x8;
#endif
                break;
            case ImageFormatLevel.Good:
#if UNITY_ANDROID
                alpha = TextureImporterFormat.ASTC_8x8;
                noAlpha = TextureImporterFormat.ASTC_8x8;
#else
                alpha = TextureImporterFormat.ASTC_6x6;
                noAlpha = TextureImporterFormat.ASTC_6x6;
#endif
                break;
            case ImageFormatLevel.Best:
#if UNITY_ANDROID
                alpha= noAlpha = TextureImporterFormat.ASTC_6x6;
#else
                alpha=noAlpha = TextureImporterFormat.ASTC_4x4;
#endif
                break;
            case ImageFormatLevel.ETC:
                if (isMultipleOf4)
                {
                    alpha = TextureImporterFormat.ETC2_RGBA8Crunched;
                    noAlpha = TextureImporterFormat.ETC_RGB4Crunched;
                }
                else
                {
                    alpha = TextureImporterFormat.ASTC_6x6;
                    noAlpha = TextureImporterFormat.ASTC_6x6;
                }
                
                break;
            case ImageFormatLevel.Original:
                alpha = TextureImporterFormat.RGBA32;
                noAlpha = TextureImporterFormat.RGBA32;
                break;
            default:
                alpha = TextureImporterFormat.ASTC_6x6;
                noAlpha = TextureImporterFormat.ASTC_6x6;
                break;
        }
    }


    private static bool IsTextureFile(string _path)
    {
        string path = _path.ToLower();
        return path.EndsWith(".psd") || path.EndsWith(".tga") || path.EndsWith(".png") || path.EndsWith(".jpg") || path.EndsWith(".dds") || path.EndsWith(".bmp") || path.EndsWith(".tif") || path.EndsWith(".gif");
    }

    private static bool IsAtlasFile(string _path)
    {
        string path = _path.ToLower();
        return path.EndsWith(".spriteatlas");
    }

    private static bool IsPowerOf2(int num)
    {
        return Mathf.IsPowerOfTwo(num);
    }

    private static bool IsPowerOf4(int num)
    {
        if (num <= 0)
        {
            return false;
        }
        while (num % 4 == 0)
        {
            num /= 4;
        }
        return num == 1;
    }

    private static Queue<string> GetFileNames(string directoryName)
    {
        Queue<string> queueExcel = new Queue<string>();

        if (File.Exists(directoryName))
        {
            queueExcel.Enqueue(directoryName);

            return queueExcel;
        }
        Queue<string> queueFolder = new Queue<string>();
        
        queueFolder.Enqueue(directoryName);
        while (queueFolder.Count > 0)
        {
            string path = queueFolder.Dequeue();
            if (!Directory.Exists(path))
            {
                continue;
            }
            DirectoryInfo di = new DirectoryInfo(path);
            DirectoryInfo[] ds = di.GetDirectories();
            if (ds.Count() > 0)
            {
                foreach (DirectoryInfo info in ds)
                {
                    queueFolder.Enqueue(info.FullName);
                }
            }
            FileInfo[] fisPng = di.GetFiles("*.png");
            FileInfo[] fisJpg = di.GetFiles("*.jpg");
            FileInfo[] fisTga = di.GetFiles("*.tga");
            FileInfo[] fisPsd = di.GetFiles("*.psd");
            if (fisPng.Count() > 0)
            {
                foreach (FileInfo finfo in fisPng)
                {
                    queueExcel.Enqueue(finfo.FullName);
                }
            }
            if (fisJpg.Count() > 0)
            {
                foreach (FileInfo finfo in fisJpg)
                {
                    queueExcel.Enqueue(finfo.FullName);
                }
            }
            if (fisTga.Count() > 0)
            {
                foreach (FileInfo finfo in fisTga)
                {
                    queueExcel.Enqueue(finfo.FullName);
                }
            }
            if (fisPsd.Count() > 0)
            {
                foreach (FileInfo finfo in fisPsd)
                {
                    queueExcel.Enqueue(finfo.FullName);
                }
            }
        }
        return queueExcel;
    }

    [MenuItem("BubbleTools/图片处理工具/特效情况查看")]
    public static void CheckEffect()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });

        foreach (var child in guids)
        {
            string filePath = AssetDatabase.GUIDToAssetPath(child);
            GameObject fileObject = AssetDatabase.LoadAssetAtPath(filePath, typeof(Object)) as GameObject;

            if (fileObject != null)
            {
                var particles = fileObject.GetComponentsInChildren<ParticleSystem>();

                bool isHaveError = false;
                foreach (var particle in particles)
                {
                    if (particle.shape.enabled &&
                        particle.shape.shapeType == ParticleSystemShapeType.Mesh &&
                        particle.shape.mesh == null)
                    {
                        isHaveError = true;
                    }
                }
                if (isHaveError)
                {
                    Debug.LogError($"Particle Name is:{fileObject.name}");
                }
            }
        }
    }
    
        public static List<string> FindEffectFiles(string rootDir)
        {
            var result = new List<string>();
    
            if (!Directory.Exists(rootDir))
            {
                Debug.LogWarning($"Directory not found: {rootDir}");
                return result;
            }
    
            // 找所有子目录（递归）
            var directories = Directory.GetDirectories(
                rootDir,
                "*",
                SearchOption.TopDirectoryOnly
            );
    
            foreach (var dir in directories)
            {
                // 目录名包含 Effect
                if (!Path.GetFileName(dir).Contains("Effects_"))
                    continue;
                
                result.Add(dir);
            }
    
            return result;
        }
}