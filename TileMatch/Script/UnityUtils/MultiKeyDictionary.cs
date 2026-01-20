using System.Collections.Generic;

namespace MySelf.Tools
{
    /// <summary>
    /// 双主键字典
    /// </summary>
    /// <typeparam name="Tkey1"></typeparam>
    /// <typeparam name="Tkey2"></typeparam>
    /// <typeparam name="Tvalue"></typeparam>
    public class MultiKeyDictionary<Tkey1, Tkey2, Tvalue> : Dictionary<Tkey1, Dictionary<Tkey2, Tvalue>>
    {
        //new public Dictionary<Tkey2, Tvalue> this[Tkey1 key]
        //{
        //    get
        //    {
        //        if (!ContainsKey(key))
        //            Add(key, new Dictionary<Tkey2, Tvalue>());

        //        Dictionary<Tkey2, Tvalue> returnObj;
        //        TryGetValue(key, out returnObj);

        //        return returnObj;
        //    }
        //}
        public bool IsContain(Tkey1 key1,Tkey2 key2,out Tvalue tvalue)
        {
            if (this.ContainsKey(key1) && this[key1].TryGetValue(key2, out tvalue))
            {
                return true;
            }
            tvalue = default(Tvalue);
            return false;
        }

        public Tvalue this[Tkey1 key1, Tkey2 key2]
        {
            get
            {
                Tvalue returnObj;
                Dictionary<Tkey2, Tvalue> temObj = this[key1];
                temObj.TryGetValue(key2, out returnObj);
                return returnObj;
            }
        }
        public virtual void Add(Tkey1 key1, Tkey2 key2, Tvalue value)
        {
            this[key1].Add(key2, value);
        }
    }
}
