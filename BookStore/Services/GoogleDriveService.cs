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
    public class GoogleDriveService
    {
        // 1. Your specific Folder ID is now set here
        private readonly string _folderId = "14iD3o18kruXZG1kNAHSUrfO52ptZpk3n";
        private readonly IWebHostEnvironment _env;
        private DriveService _driveService;

        public GoogleDriveService(IWebHostEnvironment env)
        {
            _env = env;
        }

        private async Task<DriveService> GetDriveServiceAsync()
        {
            if (_driveService != null) return _driveService;

            var credPath = Path.Combine(_env.WebRootPath, "credentials", "oauth-client.json");

            if (!System.IO.File.Exists(credPath))
                throw new FileNotFoundException($"JSON file not found at {credPath}");

            UserCredential credential;
            using (var stream = new FileStream(credPath, FileMode.Open, FileAccess.Read))
            {
                // Keeping the fixed port 5000 to prevent future "policy" errors
                var receiver = new LocalServerCodeReceiver("http://127.0.0.1:5000/authorize/");

                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    new[] { DriveService.Scope.DriveFile, DriveService.Scope.Drive },
                    "user",
                    CancellationToken.None,
                    new FileDataStore("Drive.Api.Token"),
                    receiver
                );
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