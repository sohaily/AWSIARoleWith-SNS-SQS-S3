using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AwsIAMDeveloper.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        IAmazonS3 _s3Client { get; set; }
        public HomeController(IAmazonS3 s3Client)
        {
            this._s3Client = s3Client;
        }
        [HttpPost("CreateFolder")]
         public async Task<int> CreateFolder(string bucketName,string newFolderName,string prefix ="")
        {
            PutObjectRequest request = new PutObjectRequest();
            request.BucketName = bucketName;
            request.Key = (prefix.TrimEnd('/') + "/" + newFolderName.TrimEnd('/') + "/").TrimEnd('/');
            var response = await _s3Client.PutObjectAsync(request);
            return (int)response.HttpStatusCode;
        }
        [HttpPost("UploadFile")]
        public async Task<int> UploadFile(string bucketName, string newFileName, string prefix = "")
        {
            FileInfo fileInfo = new FileInfo(newFileName);
            FileStream fileStream = fileInfo.OpenRead();
            PutObjectRequest request = new PutObjectRequest();
            request.BucketName = bucketName;
            request.Key = (prefix.TrimEnd('/') + "/" + Path.GetFileName(newFileName).TrimEnd('/')).TrimEnd('/');
            request.InputStream = fileStream;
            var response = await _s3Client.PutObjectAsync(request);
            return (int)response.HttpStatusCode;
        }
        //public async Task<int> DeleteFolder(string bucketName, string newFolderName, string prefix = "")
        //{
        //    PutObjectRequest request = new PutObjectRequest();
        //    request.BucketName = bucketName;
        //    request.Key = (prefix.TrimEnd('/') + "/" + newFolderName.TrimEnd('/') + "/").TrimEnd('/');
        //    var response = await _s3Client.PutObjectAsync(request);
        //    return (int)response.HttpStatusCode;
        //}
    }
}
