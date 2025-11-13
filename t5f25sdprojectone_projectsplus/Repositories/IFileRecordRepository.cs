// Repositories/IFileRecordRepository.cs
using System.Threading;
using System.Threading.Tasks;
using t5f25sdprojectone_projectsplus.Models.Storage;

namespace t5f25sdprojectone_projectsplus.Repositories
{
    public interface IFileRecordRepository
    {
        Task<FileRecord?> GetByIdAsync(long id, CancellationToken ct = default);
        Task<FileRecord> CreateAsync(FileRecord file, CancellationToken ct = default);
        Task UpdateAsync(FileRecord file, CancellationToken ct = default);
    }
}
