using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace ServicioTecnicoReporte
{
    public class Compression
    {
        public Bitmap RotateImage(Bitmap originalImage)
        {
            if (!originalImage.PropertyIdList.Contains(0x112))
            {
            }
            else
            {

                var prop = originalImage.GetPropertyItem(0x112);
                int val = BitConverter.ToUInt16(prop.Value, 0);
                var rot = RotateFlipType.RotateNoneFlipNone;

                if (val == 3 || val == 4)
                    rot = RotateFlipType.Rotate180FlipNone;
                else if (val == 5 || val == 6)
                    rot = RotateFlipType.Rotate90FlipNone;
                else if (val == 7 || val == 8)
                    rot = RotateFlipType.Rotate270FlipNone;

                if (val == 2 || val == 4 || val == 5 || val == 7)
                    rot |= RotateFlipType.RotateNoneFlipX;

                if (rot != RotateFlipType.RotateNoneFlipNone)
                    originalImage.RotateFlip(rot);

            }
            return originalImage;
        }

        public string GenerateThumbnail(string initialPath, string FolderToSave, Bitmap originalImage, int width, int height)
        {
            string imagePathGenerated = "";
            try
            {
                imagePathGenerated= FolderToSave + "\\" + "thumb_" + Guid.NewGuid() + ".jpg";
                originalImage = RotateImage(originalImage);
                var thumbnail = originalImage.GetThumbnailImage(width, height, () => false, IntPtr.Zero);
                Bitmap m = new Bitmap(thumbnail);
                thumbnail = RotateImage(m);
                m.Save(initialPath + "\\" + imagePathGenerated, System.Drawing.Imaging.ImageFormat.Jpeg);

                return imagePathGenerated;
            }
            catch (Exception ex) { }

            return imagePathGenerated;
        }

        public string GenerateImage(string initialPath, string FolderToSave,Bitmap originalImage, long maxFileSize, int width, int height)
        {
            string imagePathGenerated = "";
            try
            {

    
                Bitmap newImageSized = ImageNewSize(originalImage, width, height);
                MemoryStream ms = new MemoryStream();
                newImageSized.Save(ms, ImageFormat.Jpeg);
                long length = ms.Length / 1024;    
                int calidad = 90;
                int minQuality = 10;
              
                imagePathGenerated = FolderToSave + "\\" + Guid.NewGuid() + ".jpeg";
                if (length> maxFileSize)
                {

                    while (calidad >= minQuality)
                    {

                        Bitmap myBitmap;
                        ImageCodecInfo myImageCodecInfo;
                        System.Drawing.Imaging.Encoder myEncoder;
                        EncoderParameter myEncoderParameter;
                        EncoderParameters myEncoderParameters;

                        // Create a Bitmap object based on a BMP file.
                        myBitmap = newImageSized;

                        // Get an ImageCodecInfo object that represents the JPEG codec.
                        myImageCodecInfo = GetEncoderInfo("image/jpeg");

                        // Create an Encoder object based on the GUID

                        // for the Quality parameter category.
                        myEncoder = System.Drawing.Imaging.Encoder.Quality;

                        // Create an EncoderParameters object.

                        // An EncoderParameters object has an array of EncoderParameter

                        // objects. In this case, there is only one

                        // EncoderParameter object in the array.
                        myEncoderParameters = new EncoderParameters(1);

                        // Save the bitmap as a JPEG file with quality
                        myEncoderParameter = new EncoderParameter(myEncoder, calidad);
                        myEncoderParameters.Param[0] = myEncoderParameter;


                        MemoryStream stream = new MemoryStream();
                        myBitmap.Save(stream, myImageCodecInfo, myEncoderParameters);
                        long fileSize = stream.Length / 1024;
                        if (fileSize <= maxFileSize || calidad == minQuality)
                        {
                           // myBitmap = RotateImage(myBitmap);
                            myBitmap.Save(initialPath + "\\" +  imagePathGenerated, myImageCodecInfo, myEncoderParameters);
                            break;
                        }

                        calidad = calidad - 10;
                    }
                }
                else {
                   // newImageSized = RotateImage(newImageSized);
                    newImageSized.Save(initialPath + "\\" + imagePathGenerated, ImageFormat.Jpeg);
                }
            }
            catch (Exception ex) { }

            return imagePathGenerated;
        }

      
        //---------------------------------
        public  Bitmap ImageNewSize(Bitmap originalBmp, int lnWidth, int lnHeight)
        {
            System.Drawing.Bitmap bmpOut = null;
            try
            {



                
                Bitmap loBMP = originalBmp;




                ImageFormat loFormat = loBMP.RawFormat;

                decimal lnRatio;
                int lnNewWidth = 0;
                int lnNewHeight = 0;

                //*** If the image is smaller than a thumbnail just return it
                if (loBMP.Width < lnWidth && loBMP.Height < lnHeight)
                    return loBMP;

                if (loBMP.Width > loBMP.Height)
                {
                    lnRatio = (decimal)lnWidth / loBMP.Width;
                    lnNewWidth = lnWidth;
                    decimal lnTemp = loBMP.Height * lnRatio;
                    lnNewHeight = (int)lnTemp;
                }
                else
                {
                    lnRatio = (decimal)lnHeight / loBMP.Height;
                    lnNewHeight = lnHeight;
                    decimal lnTemp = loBMP.Width * lnRatio;
                    lnNewWidth = (int)lnTemp;
                }
                bmpOut = ResizeImage(originalBmp, lnNewWidth, lnNewHeight); //new Bitmap(lnNewWidth, lnNewHeight);


                if (originalBmp.PropertyIdList.Contains(0x112))
                {


                    PropertyItem it = originalBmp.GetPropertyItem(0x112);
                    // it.Value = BitConverter.ToUInt16(prop.Value, 0);
                    bmpOut.SetPropertyItem(it);
                }



                // Size size = new Size(lnNewWidth, lnNewHeight);
                //bmpOut(Image)(new Bitmap(originalBmp, size));

            }
            catch
            {
                return null;
            }

            return bmpOut;
        }

        public Bitmap ResizeImage(Image image, int width, int height)
        {

        

            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }


        public  byte[] ImageToByteStream(Image img)
        {
            byte[] data = new byte[0];
            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, ImageFormat.Png);
                ms.Seek(0, 0);
                data = ms.ToArray();
            }
            return data;
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            var codecs = ImageCodecInfo.GetImageDecoders();
            foreach (var codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }

            return null;
        }

        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }

    

    public byte[] Compress(byte[] data, long value)
        {
            var jpgEncoder = GetEncoder(ImageFormat.Jpeg);

            using (var inStream = new MemoryStream(data))
            using (var outStream = new MemoryStream())
            {
                var image = Image.FromStream(inStream);

                // if we aren't able to retrieve our encoder
                // we should just save the current image and
                // return to prevent any exceptions from happening
                if (jpgEncoder == null)
                {
                    image.Save(outStream, ImageFormat.Jpeg);
                }
                else
                {
                    var qualityEncoder = System.Drawing.Imaging.Encoder.Quality;
                    var encoderParameters = new EncoderParameters(1);
                    encoderParameters.Param[0] = new EncoderParameter(qualityEncoder, 50L);
                    image.Save(outStream, jpgEncoder, encoderParameters);
                }

                return outStream.ToArray();
            }
        }
    }

    public static class ImageExtensions
    {
        public static byte[] ToByteArray(this Image image, ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, format);
                return ms.ToArray();
            }
        }
    }
}
