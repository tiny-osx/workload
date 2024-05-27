using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TinyOS.Build.Device
{
    public abstract class DeviceClient : IDisposable
    {
        private HttpClient HttpClient { get; set; } = new HttpClient();

        public event EventHandler<MessageEventArgs>? MessageReceived;

        public DeviceClient()
        {        
             DeviceUrl = new Uri("http://tinyos:8920");
        }

        public Uri DeviceUrl
        {   
            get { return HttpClient.BaseAddress; }
            set { HttpClient.BaseAddress = value; }
        }

        public TimeSpan Timeout
        {   
            get { return HttpClient.Timeout; }
            set { HttpClient.Timeout = value; }
        }

        public string? AssemblyName {get; set;}

        protected async Task<HttpResponseMessage> GetStatusAsync()
        {
            var response = await HttpClient.GetAsync($"/");
            response.EnsureSuccessStatusCode();

            return response;
        }

        protected async Task<HttpResponseMessage> SyncClockAsync(DateTime dateTime)
        {
            var json = JsonSerializer.Serialize(dateTime, JsonContext.Default.DateTime);

            using (StringContent jsonContent = new StringContent(json, Encoding.UTF8, "application/json"))
            {
                var response = await HttpClient.PutAsync($"/clock/set", jsonContent);
                response.EnsureSuccessStatusCode();

                if (response.Content is not null && response.Content.Headers.ContentType.MediaType == "application/json")
                {
                    var contentStream = await response.Content.ReadAsStreamAsync();         
                    var date = await JsonSerializer.DeserializeAsync(contentStream, JsonContext.Default.DateTime);
                    
                    LogMessage(1, $"Remote device '{DeviceUrl.Host}' date is set to {date}.");

                    if (date < DateTime.UtcNow.AddMinutes(-1) && date > DateTime.UtcNow.AddMinutes(1))
                    {
                        throw new Exception($"Remote device date failed to set correctly. The device has the current date set '{date}'.");
                    }
                }

                return response;
            }
        }

        protected async Task<HttpResponseMessage> UploadFileAsync(Guid applicationId, FileMeta metadata, bool verifyHash)
        {
            var content = new MultipartFormDataContent();
            
            using (var fileStream = File.OpenRead(metadata.LocalPath))
            {
                // LogMessage(1, "");
                
                using (var fileContent = new StreamContent(fileStream))
                {
                    // LogMessage(1, "");
                    
                    var mimeType =  GetMimeType(Path.GetExtension(metadata.RemotePath));
                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(mimeType);

                    
                    if (verifyHash)
                    {
                        content.Add(new StringContent(AssemblyName), "assemblyname"); 
                        content.Add(new StringContent(metadata.Hash.ToHex()), "filehash");  
                    }
                   
                    content.Add(fileContent, "file", metadata.RemotePath);
                
                    var response = await HttpClient.PostAsync($"/apps/upload/{applicationId}", content);
                    response.EnsureSuccessStatusCode();

                    return response;
                }
            }
        }

        protected async Task<HttpResponseMessage> UploadCacheFileAsync(List<FileMeta> metadata, Guid applicationId)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new GZipStream(memoryStream, CompressionMode.Compress, true))
                {
                    using (var streamWriter = new StreamWriter(archive))
                    {
                        streamWriter.Write(JsonSerializer.Serialize(metadata, JsonContext.Default.ListFileMeta));
                    }
                }

                memoryStream.Position = 0;

                var multipartContent = new MultipartFormDataContent();

                using (var fileContent = new StreamContent(memoryStream))
                {
                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                    multipartContent.Add(fileContent, "file", "filecache.bin");

                    var response = await HttpClient.PostAsync($"/apps/upload/{applicationId}", multipartContent);
                    response.EnsureSuccessStatusCode();

                    return response;
                }
            }
        }

        protected async Task<List<FileMeta>> DownloadCacheFileAsync(Guid applicationId)
        {
            try
            {
                var response = await HttpClient.GetAsync($"/apps/download/{applicationId}", HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                using (var contentStream = await response.Content.ReadAsStreamAsync())
                {
                    using (var archive = new GZipStream(contentStream, CompressionMode.Decompress, true))
                    {
                        return await JsonSerializer.DeserializeAsync<List<FileMeta>>(archive) ?? new List<FileMeta>();
                    }
                }
            }
            catch
            {
                return new List<FileMeta>();
            }
        }

        protected async Task<HttpResponseMessage> CleanFilesAsync(Guid applicationId)
        {
            var response = await HttpClient.GetAsync($"/apps/clean/{applicationId}");
            response.EnsureSuccessStatusCode();
            
            return response;
        }

        protected async Task<HttpResponseMessage> DeleteFileAsync(Guid applicationId, string remotePath)
        {
            var response = await HttpClient.GetAsync($"/apps/delete/{applicationId}/file/{remotePath}");
            response.EnsureSuccessStatusCode();
            
            return response;
        }

        protected async Task<HttpResponseMessage> DebugArgsAsync(string[] args)
        {
            var json = JsonSerializer.Serialize(args, JsonContext.Default.StringArray);

            using (StringContent jsonContent = new StringContent(json, Encoding.UTF8, "application/json"))
            {
                var response = await HttpClient.PutAsync($"/debug/args/set", jsonContent);
                response.EnsureSuccessStatusCode();
                
                return response;
            }
        }

        private string GetMimeType(string fileName)
        {
            var provider = new ContentTypeProvider();
            if (!provider.TryGetContentType(fileName, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            return contentType!;            
        }

        #region Message Event 

        protected virtual void LogMessage(string message)
        {
            LogMessage(0, message);
        }

        protected virtual void LogMessage(int importance, string message)
        {
            var args = new MessageEventArgs(importance, message);

            MessageReceived?.Invoke(this, args);
        }  
        
        #endregion
        
        #region IDisposable

        public bool IsDisposed { get; set; }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    HttpClient?.Dispose();
                    LogMessage(1, $"Disconnected successfully from remote device '{DeviceUrl.Host}:{DeviceUrl.Port}'.");
                }

                IsDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}