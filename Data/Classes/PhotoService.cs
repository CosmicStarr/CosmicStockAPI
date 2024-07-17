using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Data.Classes
{
    public class PhotoService : IPhotoService
    {
        private readonly Cloudinary _cloud;
        public PhotoService(IOptions<CloudinaryPictures> config)
        {
            var CloudAccount = new Account
            (
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret
            );
            _cloud = new Cloudinary(CloudAccount);
        }

        public async Task<ImageUploadResult> AddPhotoAsync(IFormFile file)
        {
            var UploadedImage = new ImageUploadResult();
            if(file.Length > 0)
            {
                using var stream = file.OpenReadStream();
                var UploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName,stream),
                };
                UploadedImage = await _cloud.UploadAsync(UploadParams);
            }
            return UploadedImage;
        }

        public async Task<DeletionResult> DeletePhotoAsync(string PublicId)
        {
            var deletePic = new DeletionParams(PublicId);
            var results = await _cloud.DestroyAsync(deletePic);
            return results;
        }
    }
}