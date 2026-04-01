using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BookStore.Services
{
    public class CloudinaryService
    {
        // 1. Your specific Folder ID is now set here
        private readonly string _folderId = "14iD3o18kruXZG1kNAHSUrfO52ptZpk3n";
        private readonly IWebHostEnvironment _env;
        private DriveService _driveService;

        public CloudinaryService(IWebHostEnvironment env)
        {
            _env = env;
        }

        private async Task<DriveService> GetDriveServiceAsync()
        {
            if (_driveService != null) return _driveService;

            var json = Environment.GetEnvironmentVariable("GOOGLE_DRIVE_CREDENTIALS");

            if (string.IsNullOrEmpty(json))
                throw new Exception("Google Drive credentials not found in environment variables");

            GoogleCredential credential;

            using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(DriveService.Scope.Drive);
            }

            _driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "BookStoreDriveIntegration"
            });

            return _driveService;
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            var service = await GetDriveServiceAsync();

            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = fileName,
                Parents = new List<string> { _folderId }
            };

            var request = service.Files.Create(fileMetadata, fileStream, contentType);
            request.Fields = "id";

            var uploadResult = await request.UploadAsync();
            if (uploadResult.Status == UploadStatus.Failed)
                throw new Exception($"Google Drive Upload Failed: {uploadResult.Exception?.Message}");

            var uploadedFile = request.ResponseBody;
            await SetPublicPermission(service, uploadedFile.Id);

            // Logic to return different URLs based on file type
            if (contentType.ToLower().Contains("pdf"))
            {
                // This opens the Google Drive PDF viewer
                return $"https://drive.google.com/file/d/{uploadedFile.Id}/view?usp=sharing";
            }

            // Existing working image logic
            return $"https://lh3.googleusercontent.com/d/{uploadedFile.Id}=w1000";
        }

        private async Task SetPublicPermission(DriveService service, string fileId)
        {
            var permission = new Permission
            {
                Type = "anyone",
                Role = "reader"
            };
            await service.Permissions.Create(permission, fileId).ExecuteAsync();
        }
    }
}

//hhh