using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using t5f25sdprojectone_projectsplus.Github;
using t5f25sdprojectone_projectsplus.Models;

namespace t5f25sdprojectone_projectsplus.Github
{
    // Contract for fetching live data from GitHub and performing CRUD on cached entities.
    public interface IGitHubService
    {
        // Fetch and upsert repository metadata (returns cached entity)
        Task<GitHubRepository> UpsertRepositoryAsync(GitHubRepositoryDto repoDto, CancellationToken ct = default);

        // Fetch repository from GitHub API (live) and optionally cache it
        Task<GitHubRepositoryDto> FetchRepositoryFromGitHubAsync(string owner, string repo, CancellationToken ct = default);

        // Fetch and upsert commits for a repository
        Task UpsertCommitsAsync(long gitHubRepositoryId, IEnumerable<GitHubCommitDto> commits, CancellationToken ct = default);

        // Fetch issues and upsert
        Task UpsertIssuesAsync(long gitHubRepositoryId, IEnumerable<GitHubIssueDto> issues, CancellationToken ct = default);

        // Fetch PRs and upsert
        Task UpsertPullRequestsAsync(long gitHubRepositoryId, IEnumerable<GitHubPullRequestDto> prs, CancellationToken ct = default);

        // File operations: fetch file content (raw) from GitHub and cache pointer
        Task<GitHubFileReferenceDto> FetchFileAsync(string owner, string repo, string path, CancellationToken ct = default);

        // CRUD helpers for local cache (simple signatures)
        Task<GitHubRepository?> GetRepositoryByIdAsync(long id, CancellationToken ct = default);
        Task<IEnumerable<GitHubRepository>> QueryRepositoriesAsync(string? owner = null, string? name = null, CancellationToken ct = default);

        Task DeleteRepositoryAsync(long id, CancellationToken ct = default);
        Task DeleteCommitAsync(long id, CancellationToken ct = default);
        Task DeleteIssueAsync(long id, CancellationToken ct = default);
        Task DeletePullRequestAsync(long id, CancellationToken ct = default);
    }
}
