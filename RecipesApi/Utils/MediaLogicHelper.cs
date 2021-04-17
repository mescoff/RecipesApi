using Castle.Core.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RecipesApi.DTOs;
using RecipesApi.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace RecipesApi.Utils
{
    /// <summary>
    /// Media Logic Helper : Help upload and download pictures from server
    /// MediaPath will be in format :  <BaseDirectory>/<User>/RecipeImages/<RecipeId>/imageName.png
    /// </summary>
    public class MediaLogicHelper : IMediaLogicHelper, IDisposable
    {
        /// Quick Glossary for terms I use here an feel similar
        /// Directory = Single Folder name  (ex: MediaFolder)
        /// Path = Full address path (ex: C:/xx/xx)
        /// File/FileName = File name with extension (ex: Food.jpeg)

        /// <summary>
        /// Base directory for users media
        /// </summary>
        private readonly string _mediasBasePath;

        /// <summary>
        /// The user that triggered the use of this helper (and will be used for paths)
        /// </summary>
        public int CurrentUserId { get; }  // TODO: longer term should not be part of this class but come with user info

        /// <summary>
        /// Max size of files saved
        /// </summary>
        public long MAX_IMAGESIZE {get;}

        /// <summary>
        /// Full path for this request (MediaDirectory+UserSpecificFolder)
        /// </summary>
        public string UserMediasPath { get; }

        private readonly ILogger<MediaLogicHelper> _logger;


        public MediaLogicHelper(IConfiguration configuration, ILogger<MediaLogicHelper> logger)
        {
            this._logger = logger;
            try
            {
                this.MAX_IMAGESIZE = Convert.ToInt64(configuration["MediasSettings:MaxImageSize"]);

                // TODO: Should this logic be here? Should we just move everything to SaveMediaLocally ? And Make this class static
                this._mediasBasePath = configuration["MediasSettings:BasePath"];
                this.CurrentUserId = 2301; // FOR DEBUG PURPOSES
                var imagesDirectoryName = configuration["MediasSettings:ImagesDirectory"]; // Fixed for now because we only handle image. But in future we'll have directories for Audio/Pdf ?
                this.UserMediasPath = GenerateUserMediasPath(imagesDirectoryName);
                // Create directory if not exist
                Directory.CreateDirectory(this.UserMediasPath);
                this._logger.LogInformation($"Recipe image will be saved in {this.UserMediasPath}");
            }
            catch (Exception e)
            {
                this._logger.LogError($"Something happened: {e}");
                throw e;
            }
        }

        /// <summary>
        /// Run through all media provided and save the ones of type image
        /// </summary>
        /// <param name="medias"></param>
        /// <param name="savingStatus"></param>
        /// <returns></returns>
        public IList<Media> SaveImagesLocally(IEnumerable<MediaDto> medias, out ServiceResponse savingStatus)
        {
            savingStatus = new ServiceResponse();
            var result = new List<Media>(); // to return path for each media
            foreach (var mediaDto in medias)
            {
                string recipeDirectory;
                var mediaFilePath = GenerateSingleMediaPath(this.UserMediasPath, mediaDto.Recipe_Id, mediaDto.Id, out recipeDirectory);          
                try
                {                 
                    // Parse data URL and retrieve image bytes from it. base64UrlSplit should be split in 2 parts (info + imageinBase64String)
                    var base64UrlSplit = mediaDto.MediaDataUrl.Split(',');
                    byte[] dataBytes;
                    dataBytes = base64UrlSplit.Length == 2 ? Convert.FromBase64String(base64UrlSplit[1]) : null;
                    if (dataBytes == null) {
                        // If we can't read dataURL, track error
                        savingStatus.Success = false;
                        var msg = $"Unable to load MediaDto [ID:{mediaDto.Id}, Title:{mediaDto.Title}, RecipeID:{mediaDto.Recipe_Id}]. Something is wrong with dataUrl received";
                        this._logger.LogError(msg);
                        savingStatus.Message = savingStatus.Message == null ? msg : savingStatus.Message.Concat(msg).ToString();
                        continue;
                    }

                    // verify it's a valid image, otherwise exception will be caught 
                    var image = ByteArrayToImage(dataBytes);
                    double fileSize = dataBytes.Length;
                    if (image != null)
                    {
                        // we will allow a max size of 800kb
                        // TODO: [Temporary Fix] Jpeg saved with quality 100L will be bigger than original size and can go > 800K... so saving at 80  
                        double qualityLevel = (fileSize > MAX_IMAGESIZE) ? ((MAX_IMAGESIZE / fileSize) * 100) : 80;
                        var qualityLevelConverted = Convert.ToInt64(qualityLevel);
                 
                        // FOR NOW we only support JPEG and we Compress.  [In long run we could do lossless PNG if img is < specific size ?]
                        // 1- We need to Compress our Image (maybe resize it but that'll be for later)
                        // 2- Convert it to JPEG (--for now-- even if already JPEG since we need to perform the compression anyway)
                        // 3- Then Write bytes to file.
                        var imageBytesPrepped = Compress(dataBytes, qualityLevelConverted, ImageFormat.Jpeg);

                        // Making sure the directory exist or create it
                        Directory.CreateDirectory(recipeDirectory);
                        // Save the file
                        File.WriteAllBytes(mediaFilePath, imageBytesPrepped);
                        this._logger.LogInformation($"Media [ID:{mediaDto.Id}, Title:{mediaDto.Title}, RecipeID:{mediaDto.Recipe_Id}] was successfully save to Path:{mediaFilePath} ");
                        result.Add(new Media { Id = mediaDto.Id, MediaPath = mediaFilePath });
                        savingStatus.Success = true;
                    }
                }
                catch (Exception e)
                {
                    this._logger.LogError($"Something happened when Saving the mediaDto [ID:{mediaDto.Id}, Title:{mediaDto.Title}, RecipeID:{mediaDto.Recipe_Id}]. Error:", e);
                    savingStatus.Success = false;
                    var msg = $"MediaDto [ID:{mediaDto.Id}, Title:{mediaDto.Title}, RecipeID:{mediaDto.Recipe_Id}] failed to save"; // TODO: test this scenario and provide more details in error message
                    savingStatus.Message = savingStatus.Message == null ? msg : savingStatus.Message.Concat(msg).ToString();
                }
            }

            return result;
        }

        public IList<MediaDto> LocateAndLoadMedias(IEnumerable<Media> medias)
        {
            var result = new List<MediaDto>();
            foreach (var media in medias)
            {
                if (File.Exists(media.MediaPath))
                {
                    try
                    {
                        //var bytes = await File.ReadAllBytesAsync(media.MediaPath); // Not needed at the moment, not loading from remote server
                        var bytes = File.ReadAllBytes(media.MediaPath);
                        // Check that its indeed an image (by converting it to Image object)
                        var image = ByteArrayToImage(bytes);
                        if (image != null)
                        {                         
                            // convert bytes to base64 and create a dataURL object.
                            var b64String = Convert.ToBase64String(bytes);
                            var mimeType = "image/jpeg";
                            var dataUrl = $"data:{mimeType};base64,{b64String}";  // jpeg is default. We convert all images to jpeg on receive
                            var convertedMedia = new MediaDto { Id = media.Id, MediaDataUrl = dataUrl, Recipe_Id = media.Recipe_Id, Tag = media.Tag, Title = media.Title };
                            result.Add(convertedMedia);
                            image.Dispose();
                        }
                    }
                    catch (Exception e)
                    {
                        this._logger.LogError($"Something happened when Loading media [[ID:{media.Id}, Title:{media.Title}, RecipeID:{media.Recipe_Id}] ");
                    }
                }
                else
                {
                    this._logger.LogWarning($"Path doesn't exist on Media [[ID:{media.Id}, Title:{media.Title}, RecipeID:{media.Recipe_Id}, path:{media.MediaPath}]. Won't load");
                }
            }
            return result;
        }

        public void Dispose()
        {

        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
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

        /// <summary>
        /// Convert byte array into image
        /// </summary>
        /// <param name="byteArray">Byte array provided</param>
        /// <returns>An Image if conversion was successfull </returns>
        // TODO: make this static. Generate a static logger for these scenarios ?
        // https://stackoverflow.com/questions/48676152/asp-net-core-web-api-logging-from-a-static-class
        public static Image ByteArrayToImage(byte[] byteArray)
        {
            try
            {
                using (MemoryStream mStream = new MemoryStream(byteArray))
                {
                    var image = Image.FromStream(mStream);
                    return image;
                }
            }
            catch (Exception e)
            {

                throw (e);
                //this._logger.LogError("Converting byte array to image failed", e);
            }
        }

        /// <summary>
        /// Convert Image to byte array
        /// </summary>
        /// <param name="img">Image provided</param>
        /// <returns> Byte array if conversion is successfull </returns>
        // TODO: make this static. Generate a static logger for these scenarios?
        // https://stackoverflow.com/questions/48676152/asp-net-core-web-api-logging-from-a-static-class
        public static byte[] ImgToByteArray(Image img)
        {
            ImageConverter imgConverter = new ImageConverter();
            try
            {
                var result = imgConverter.ConvertTo(img, typeof(byte[]));
                return (byte[])result;
            }
            catch (Exception e)
            {
                throw (e);
                //this._logger.LogError("Converting image to bytes failed", e);
            }
        }

        //public static byte[] ConvertToPNG(Image img)
        //{
        //    // https://www.andrewhoefling.com/Blog/Post/basic-image-manipulation-in-c-sharp
        //    using (var outstream = new MemoryStream())
        //    {
        //        img.Save(outstream, ImageFormat.Png);
        //        return outstream.ToArray();
        //    }
        //}

        public static byte[] Resize(byte[] data, int width)
        {
            using (var stream = new MemoryStream(data))
            {
                var image = Image.FromStream(stream);

                var height = (width * image.Height) / image.Width;
                var thumbnail = image.GetThumbnailImage(width, height, null, IntPtr.Zero);

                using (var thumbnailStream = new MemoryStream())
                {
                    thumbnail.Save(thumbnailStream, ImageFormat.Png);
                    return thumbnailStream.ToArray();
                }
            }
        }

        public static byte[] Compress(byte[] data, long value, ImageFormat format = null)
        {
            // TODO: Value should always be 100L... To actually reduce the size/weights of the image we need to resize it
            // Follow this https://www.codeproject.com/Tips/602135/Resize-Images-to-Size-Limit
            // OR check the resize function above


            format = format ?? ImageFormat.Png; // default format
            var formatEncoder = GetEncoder(format);

            using (var inStream = new MemoryStream(data))
            using (var outStream = new MemoryStream())
            {
                var image = Image.FromStream(inStream);

                // if we aren't able to retrieve our encoder
                // we should just save the current image and
                // return to prevent any exceptions from happening
                if (formatEncoder == null)
                {
                    image.Save(outStream, ImageFormat.Jpeg);
                }
                else
                {
                    // Save the image as a PNG file with adapted quality (100L max, 0L min)
                    var qualityEncoder = Encoder.Quality;
                    //// Create an Encoders Param array with length 1 (for Quality we'll add)
                    var encoderParameters = new EncoderParameters(1);
                    // Save the Quality parameter category in it
                    encoderParameters.Param[0] = new EncoderParameter(qualityEncoder, value);
                                    
                    image.Save(outStream, formatEncoder, encoderParameters);
                }

                return outStream.ToArray();
            }
        }

        /// <summary>
        /// Generates the full path where to save the media, including media filename
        /// </summary>
        /// <param name="userMediasPath">Full path to User Media directory (.../.../UserId/Medias/)</param>
        /// <param name="recipeId">The recipe Id</param>
        /// <param name="mediaId">The media Id</param>
        /// <param name="userRecipePath">Returns recipe path if needed</param>
        /// <returns>Full path to save media (.../.../UserId/Medias/RecipeId/MediaId.jpg)</returns>
        public static string GenerateSingleMediaPath(string userMediasPath, int recipeId, int mediaId, out string userRecipePath)
        {
            // For now we force JPEG format
            var mediaFileName = String.Concat(Convert.ToString(mediaId), ".jpg"); ;
            userRecipePath = Path.Combine(userMediasPath, Convert.ToString(recipeId));
            var mediaFilePath = Path.Combine(userRecipePath, mediaFileName);
            return mediaFilePath;
        }

        /// <summary>
        /// Generates the full path to the user's medias directory
        /// </summary>
        /// <param name="mediasDirectoryName">The medias directory name (Images/Pdf/Audio)</param>
        /// <returns></returns>
        public string GenerateUserMediasPath(string mediasDirectoryName)
        {
            var userMediasPath = Path.Combine(this._mediasBasePath, Convert.ToString(this.CurrentUserId), mediasDirectoryName);
            return userMediasPath;
        }

      
        // standardize size of image. This will be at most on a laptop. Image should not weight more than 1Mo
        // Get general path from config, then set path per user.   UserId/Media/RecipeImages
    }
}
