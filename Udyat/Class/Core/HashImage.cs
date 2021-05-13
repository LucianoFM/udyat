using System.Drawing;
using System.Security.Cryptography;

namespace Udyat.Class
{
    public class HashImage
    {
        public static string GetHashImage(Bitmap bmp1)
        {
            System.Drawing.ImageConverter ic = new System.Drawing.ImageConverter();
            byte[] btImage1 = new byte[1];
            btImage1 = (byte[])ic.ConvertTo(bmp1, btImage1.GetType());
            SHA256Managed shaM = new SHA256Managed();
            // Retorna string de base 64 
            return System.Convert.ToBase64String((shaM.ComputeHash(btImage1)));
        }

        public static bool HashBelongsImage(string pHashImage, Bitmap pImage)
        {
            string imgHash = GetHashImage(pImage);
            return pHashImage == imgHash;
        }

    }
}
