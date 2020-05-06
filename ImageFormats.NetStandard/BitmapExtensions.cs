using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

/*

This is a class library that contains image decoders for old and/or
obscure image formats (.TGA, .PCX, .PPM, RAS, etc.). Refer to the
individual source code files for each image type for more information.

Copyright 2013-2016 Dmitry Brant
http://dmitrybrant.com

Copyright 2017 redmanmale

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

*/

namespace ImageFormats.NetStandard
{
    public static class BitmapExtensions
    {
        /// <summary>
        /// Load a file into a standard Bitmap object. Will automatically
        /// detect the format of the image.
        /// </summary>
        /// <param name="fileName">Name of the file to load.</param>
        /// <returns>Bitmap that contains the decoded image, or null if it could
        /// not be decoded by any of the formats known to this library.</returns>
        public static Bitmap Load(string fileName)
        {
            Bitmap bitmap;
            using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                bitmap = Load(fileStream);

                if (bitmap != null)
                {
                    return bitmap;
                }

                var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
                if (string.IsNullOrWhiteSpace(ext))
                {
                    return null;
                }

                if (ext.EndsWith("tga"))
                {
                    bitmap = TgaReader.Load(fileStream);
                }
                else if (ext.EndsWith("cut"))
                {
                    bitmap = CutReader.Load(fileStream);
                }
                else if (ext.EndsWith("sgi") || ext.EndsWith("rgb") || ext.EndsWith("bw"))
                {
                    bitmap = SgiReader.Load(fileStream);
                }
                else if (ext.EndsWith("xpm"))
                {
                    bitmap = XpmReader.Load(fileStream);
                }
            }

            return bitmap;
        }

        /// <summary>
        /// Load a file into a standard Bitmap object. Will automatically
        /// detect the format of the image.
        /// </summary>
        /// <param name="fileName">Name of the file to load.</param>
        /// <returns>Bitmap byte array.</returns>
        public static byte[] Load(ref string fileName)
        {
            Bitmap bitmap = Load(fileName);

            return bitmap.ToBytes(ImageFormat.Bmp);
        }

        /// <summary>
        /// Create a standard Bitmap object from a Stream. Will automatically
        /// detect the format of the image.
        /// </summary>
        /// <param name="stream">Stream from which the image will be read.</param>
        /// <returns>Bitmap that contains the decoded image, or null if it could
        /// not be decoded by any of the formats known to this library.</returns>
        public static Bitmap Load(Stream stream)
        {
            //read the first few bytes of the file to determine what format it is...
            byte[] header = new byte[256];
            stream.Read(header, 0, header.Length);
            stream.Seek(0, SeekOrigin.Begin);

            Bitmap bmp;
            if (header[0] == 0xA && header[1] >= 0x3 && header[1] <= 0x5 && header[2] == 0x1 && header[4] == 0 && header[5] == 0)
            {
                bmp = PcxReader.Load(stream);
            }
            else if (header[0] == 'P' && header[1] >= '1' && header[1] <= '6' && (header[2] == 0xA || header[2] == 0xD))
            {
                bmp = PnmReader.Load(stream);
            }
            else if (header[0] == 0x59 && header[1] == 0xa6 && header[2] == 0x6a && header[3] == 0x95)
            {
                bmp = RasReader.Load(stream);
            }
            else if (header[0x80] == 'D' && header[0x81] == 'I' && header[0x82] == 'C' && header[0x83] == 'M')
            {
                bmp = DicomReader.Load(stream);
            }
            else
            {
                bmp = (Bitmap)Image.FromStream(stream);
            }

            return bmp;
        }

        /// <summary>
        /// Create a standard Bitmap object from a Stream. Will automatically
        /// detect the format of the image.
        /// </summary>
        /// <param name="stream">Stream from which the image will be read.</param>
        /// <returns>Bitmap byte array.</returns>
        public static byte[] Load(ref Stream stream)
        {
            Bitmap bitmap = Load(stream);

            return bitmap.ToBytes(ImageFormat.Bmp);
        }

        /// <summary>
        /// Bitmap To Bytes
        /// </summary>
        /// <param name="bitmap">Bitmap that contains the decoded image</param>
        /// <param name="imageFormat">image Format(.Bmp,.Png,...)</param>
        /// <returns>Bitmap byte array</returns>
        public static byte[] ToBytes(this Bitmap bitmap, ImageFormat imageFormat)
        {
            using (var ms = new MemoryStream())
            {
                bitmap.Save(ms, imageFormat);
                return ms.ToArray();
            }
        }
    }
}