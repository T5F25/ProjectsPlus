// Repositories/IUserRepository.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using t5f25sdprojectone_projectsplus.Models.Identity;


namespace t5f25sdprojectone_projectsplus.Repositories
{
    /// <summary>
    /// Minimal repository contract for User persistence and lookup.
    /// - Implementations must be explicit about UpdatedAt/Version maintenance.
    /// - Methods avoid exposing DbContext directly and return null when not found.
    /// - Update methods should throw a concurrency exception on stale Version.
    /// </summary>
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(long id, CancellationToken ct = default);
        Task<User?> FindByEmailAsync(string normalizedEmail, CancellationToken ct = default);
        Task<User?> FindByGithubIdAsync(long githubId, CancellationToken ct = default);
        Task<User> CreateAsync(User user, CancellationToken ct = default);
        /// <summary>
        /// Update expects caller to set UpdatedAt = UtcNow and increment Version (or allow repository to do so atomically).
        /// Returns updated entity or throws if concurrency conflict.
        /// </summary>
        Task<User> UpdateAsync(User user, CancellationToken ct = default);
    }
}
