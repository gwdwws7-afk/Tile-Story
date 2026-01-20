using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;

public class TileDictConverter : JsonConverter
{
    public override bool CanRead => true;
    public override bool CanWrite => true;
    public override bool CanConvert(Type objectType)
    {
        return true;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        Type type = value.GetType();
        IEnumerable keys = (IEnumerable)type.GetProperty("Keys").GetValue(value, null);
        IEnumerable values = (IEnumerable)type.GetProperty("Values").GetValue(value, null);
        IEnumerator enumerator = values.GetEnumerator();

        writer.WriteStartObject();
        foreach (object key in keys)
        {
            string keyStr = Convert.ToString(key);
            writer.WritePropertyName(keyStr);

            enumerator.MoveNext();
            Dictionary<int, TileInfo> vStr = (Dictionary<int, TileInfo>)enumerator.Current;
            Dictionary<int, string> stringDic = new Dictionary<int, string>();
            foreach (KeyValuePair<int, TileInfo> pair in vStr)
            {
                TileInfo info = pair.Value;
                string s = info.TileID.ToString();
                if (info.AttachID != 0)
                {
                    s = s + "_" + info.AttachID.ToString();
                    if (info.DirectionType != 1) 
                        s = s + "_" + info.DirectionType.ToString();
                }
                else if (info.DirectionType != 1)
                {
                    s = s + "_0_" + info.DirectionType.ToString();
                }

                stringDic.Add(pair.Key, s);
            }

            serializer.Serialize(writer, stringDic);
        }

        writer.WriteEndObject();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        Dictionary<int, Dictionary<int, TileInfo>> res = new Dictionary<int, Dictionary<int, TileInfo>>();

        object key = null;
        Dictionary<int, string> value = null;
        while (reader.Read())
        {
            if (reader.TokenType == JsonToken.EndObject)
                break;

            if (reader.TokenType == JsonToken.PropertyName)
            {
                key = reader.Value;
            }
            else
            {
                value = (Dictionary<int, string>)serializer.Deserialize(reader, typeof(Dictionary<int, string>));
                Dictionary<int, TileInfo> infoDic = new Dictionary<int, TileInfo>();
                foreach (KeyValuePair<int, string> pair in value)
                {
                    string[] splits = pair.Value.Split('_');
                    int tileId = splits.Length > 0 ? int.Parse(splits[0]) : 0;
                    int attachId = splits.Length > 1 ? int.Parse(splits[1]) : 0;
                    int directionType= splits.Length > 2 ? int.Parse(splits[2]) : 1;
                    TileInfo info = new TileInfo(tileId, attachId, directionType);
                    infoDic.Add(pair.Key, info);
                }

                res.Add(Convert.ToInt32(key), infoDic);

                key = null;
                value = null;
            }
        }

        return res;
    }
}
