
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.Text;

namespace ImageResize.Services
{
    public class ImageManipulationService : IImageManipulationService
    {
        public async Task<Stream> GetThumbnailFromImageAsync(Stream sourceImageStream)
        {
            using Image image = Image.Load(sourceImageStream);
            Stream imageStream = new MemoryStream();

            image.Mutate(x => x.Grayscale().Resize(200, 200));

            await image.SaveAsJpegAsync(imageStream, new JpegEncoder());

            imageStream.Seek(0, SeekOrigin.Begin);

            return imageStream;
        }

        public string GetThumbnailPath(string key)
        {
            const string thumbnailBasePath = "thumbnail/";

            int endSlash = key.LastIndexOf('/');

            StringBuilder thumbnailPathBuilder = new();

            if (endSlash <= 0)
                thumbnailPathBuilder
                    .Append(thumbnailBasePath)
                    .Append(key)
                    .ToString();

            string objName = key.Substring(endSlash + 1);

            int beginSlash = key.Substring(0, endSlash - 1).LastIndexOf('/');

            if (beginSlash <= 0)
                return thumbnailPathBuilder
                           .Append(thumbnailBasePath)
                           .Append(objName)
                           .ToString();

            return thumbnailPathBuilder
                        .Append(key.Substring(0, beginSlash))
                        .Append(thumbnailBasePath)
                        .Append(objName)
                        .ToString();
        }
    }
}
