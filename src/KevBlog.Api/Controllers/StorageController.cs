﻿using KevBlog.Application.Services;
using KevBlog.Domain.Repositories;
using KevBlog.Infrastructure.Extensions;
using KevBlog.Persistence.Aws.S3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KevBlog.Api.Controllers
{

    [Authorize]
    public class StorageController : BaseApiController
    {
        private readonly IAwsStorageService _awsStorageService;
        private readonly IUserService _userService;
        public StorageController(IAwsStorageService awsStorageService, IUserService userService)
        {
            _awsStorageService = awsStorageService;
            _userService = userService;
        }

        // TODO Implementation test cases. 
        [HttpPost("Bucket/{bucketName}/UploadFile")]
        public async Task<IActionResult> UploadFile(List<IFormFile> formFiles, string bucketName, CancellationToken token)
        {
            var isAuthorized = await _awsStorageService.IsAuthorized(bucketName, User.GetUserId());
            if(!isAuthorized)
                return Unauthorized($"User is not authorized to access to this bucket: {bucketName}");
            
            List<string> uploadedFiles = new List<string>();
            foreach (IFormFile postedFile in formFiles)
            {
                using (var ms = new MemoryStream())
                {
                    await postedFile.CopyToAsync(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    var result = await _awsStorageService.UploadFileAsync(ms,"KevinBucket", postedFile.FileName);
                }
            }
            return Ok();
        }

        [HttpGet("GetFile/{bucketName}")]
        public async Task<IActionResult> GetFile(string bucketName, CancellationToken token)
        {

            return Ok();
        }

        [HttpPost("Bucket")]
        public async Task<IActionResult> CreateBucket(string bucketName)
        {
            var result = await _awsStorageService.CreateBucketAsync(bucketName, User.GetUserId());
            if(result) return Ok();
            return BadRequest("Creating bucket Failure");
        }

    }
}
