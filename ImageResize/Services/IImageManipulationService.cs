using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageResize.Services
{
    internal interface IImageManipulationService
    {
        Task<Stream> GetThumbnailFromImageAsync(Stream image);
        string GetThumbnailPath(string key);
    }
}
