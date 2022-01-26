using System;

namespace Website.Server.Helpers
{
    public class ConversionHelper
    {
        public static byte[] HexadecimalToBytes(string hex)
        {
            hex = hex.Replace("-", string.Empty);

            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public static string BytesToHexadecimal(byte[] bytes)
        {
            return BitConverter.ToString(bytes);
        }
    }
}
