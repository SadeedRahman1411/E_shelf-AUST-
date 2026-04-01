using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace BookStore.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration config)
        {
            var account = new Account(
                config["Cloudinary:CloudName"],
                config["Cloudinary:ApiKey"],
                config["Cloudinary:ApiSecret"]
            );

            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
        {
            var uploadParams = new RawUploadParams()
            {
                File = new FileDescription(fileName, fileStream),
                Type = "upload",          // ensures public access
                AccessMode = "public",
                UseFilename = true,
                UniqueFilename = true,
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error != null)
                throw new Exception(result.Error.Message);

            Console.WriteLine($"=== CLOUDINARY UPLOAD ===");
            Console.WriteLine($"Access Mode: {result.AccessMode}");
            Console.WriteLine($"URL: {result.SecureUrl}");
            Console.WriteLine($"Type: {result.JsonObj}");

            return result.SecureUrl.ToString();
        }
    }
}