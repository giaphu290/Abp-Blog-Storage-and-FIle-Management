using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.FileManagement.Files;
using Volo.FileManagement;
using Volo.Abp.BlobStoring;
using Volo.Abp.Caching;
using System.IO;
using HQSOFT.SystemAdministration.Containers;
using Volo.Abp;
using Elsa;

namespace HQSOFT.SystemAdministration.BlobStorage
{
    public class CustomFileDescriptorAppService : FileDescriptorAppService
    {
        private readonly IContainerAppService _containerAppSerivce;
        private readonly IBlobContainerFactory _blobContainerFactory;
        private readonly IFileDescriptorRepository _fileDescriptorRepository;
        public CustomFileDescriptorAppService(
         IFileManager fileManager,
         IFileDescriptorRepository fileDescriptorRepository,
         IBlobContainer<FileManagementContainer> blobContainer,
         IDistributedCache<FileDownloadTokenCacheItem, string> downloadTokenCache,
         IContainerAppService containerAppSerivce,
         IBlobContainerFactory blobContainerFactory)
         : base(fileManager, fileDescriptorRepository, blobContainer, downloadTokenCache)
        {
            _containerAppSerivce = containerAppSerivce;
            _blobContainerFactory = blobContainerFactory;
            _fileDescriptorRepository = fileDescriptorRepository;
        }

        public override async Task<FileDescriptorDto> CreateAsync(Guid? directoryId, CreateFileInputWithStream inputWithStream)
        {
            var fileExtension = inputWithStream.File.FileName;
            var container = await _containerAppSerivce.GetContainerByPath(fileExtension);
            if (container == null)
            {
                throw new BusinessException("No matching container found for the given path.");
            }
            using var stream = inputWithStream.File.GetStream();
            var blobContainer = _blobContainerFactory.Create(container.Name);

            //var existingFile = await _fileDescriptorRepository.FindAsync(inputWithStream.Name);
            //if (existingFile != null)
            //{
            //    var originalFileName = Path.GetFileNameWithoutExtension(inputWithStream.Name);
            //    var fileExtensionOnly = Path.GetExtension(inputWithStream.Name);
            //    int i = 1;

            //    // Kiểm tra và tạo tên mới cho file không trùng lặp
            //    while (await _fileDescriptorRepository.FindAsync(inputWithStream.Name) != null)
            //    {
            //        inputWithStream.Name = $"{originalFileName}({i}){fileExtensionOnly}";
            //        i++;
            //    }
            //}
            await blobContainer.SaveAsync(inputWithStream.Name, stream);

            //            if (container.TypeStorage == "FileSystem")
            //            {
            //                // Tạo Blob Container với FileSystem và cấu hình BasePath
            //                var blobContainer = _blobContainerFactory.Create(container.Name);
            //\
            //                //// Lưu file vào FileSystem Blob Container
            //                await blobContainer.SaveAsync(inputWithStream.Name, stream);
            //            }
            //            else if (container.TypeStorage == "Database")
            //            {
            //                // Tạo Blob Container với Database
            //                var blobContainer = _blobContainerFactory.Create(container.Name);
            //                // Lưu file vào Database Blob Container
            //                await blobContainer.SaveAsync(inputWithStream.Name, stream);
            //            }
            //            else
            //            {
            //                throw new BusinessException($"Unsupported storage type: {container.TypeStorage}");
            //             }

            return await base.CreateAsync(directoryId, inputWithStream);
        }
        public override async Task DeleteAsync(Guid id)
        {
            var fileDescriptor = _fileDescriptorRepository.GetAsync(id);
            if (fileDescriptor == null)
            {
                throw new BusinessException("File not found.");
            }
            var container = await _containerAppSerivce.GetContainerByPath(fileDescriptor.Result.Name);
            if (container == null)
            {
                throw new BusinessException("Container not found.");
            }

            var blobContainer = _blobContainerFactory.Create(container.Name);
            // Xóa file từ Blob Container
            await blobContainer.DeleteAsync(fileDescriptor.Result.Name);
            await base.DeleteAsync(id);
        }
        //public override async Task<FileDescriptorDto> MoveAsync(MoveFileInput input)
        //{ }
        //public override async Task<FileDescriptorDto> RenameAsync(Guid id, RenameFileInput input)
        //{ }
    }
    
}
