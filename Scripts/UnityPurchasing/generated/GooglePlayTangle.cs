// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("1mTnxNbr4O/MYK5gEevn5+fj5uVk5+nm1mTn7ORk5+fmMtAEpgEAETt+ghMCFg8L1duzM38N2ZMuDVEdRB+ZNS/JPdt4msPCvXaPHgMFxtgyx577LqrZ9uSdI7RYL3J5oSjeroosUlY2wdJnqc5S7uPINty/+RJFG8uZDSce0jfXXiI0gEgDm4fC5pbUI67S+RuC3Ay/S7BTou+ZiHflbxdNHpyAOjpPobg6Zrg4AAkU9fe2J+OxkluHKEI+q0solo+wmvRFAm2aAKjsZ0kS62gVC3eOhjAHtunubx7ay0HEvXiGN3kpJ0WLpZt5n4349aViDBB5BXrKmsfmVO5Vgb+sK9+/Wohi0KBjXeNL/Ggf4jIL9AScx3Uzda29O6JUceTl5+bn");
        private static int[] order = new int[] { 0,1,2,7,9,13,7,9,13,10,13,13,12,13,14 };
        private static int key = 230;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
