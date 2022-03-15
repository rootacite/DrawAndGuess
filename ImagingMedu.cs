using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawAndGuess
{
    static internal class ImagingMedu
    {
        private static ImageCodecInfo GetEncoder(string format)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; j++)
            {
                if (encoders[j].MimeType == format)
                    return encoders[j];
            }

            return null;
        }
        static public byte[] CompressionImage(this Bitmap bm, long quality)
        {

            ImageCodecInfo CodecInfo = GetEncoder("image/jpeg");
            System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
            EncoderParameters myEncoderParameters = new EncoderParameters(1);
            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, quality);
            myEncoderParameters.Param[0] = myEncoderParameter;
            using (MemoryStream ms = new MemoryStream())
            {
                bm.Save(ms, CodecInfo, myEncoderParameters);
                myEncoderParameters.Dispose();
                myEncoderParameter.Dispose();
                return ms.ToArray();
            }
        }

        public static Bitmap GetBitmap(byte[] Source)
        {
            return new Bitmap(new MemoryStream(Source, false), true);
        }
    }
}
