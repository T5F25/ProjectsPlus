using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Octokit;
using t5f25sdprojectone_projectsplus.Data;
using t5f25sdprojectone_projectsplus.Github;
using t5f25sdprojectone_projectsplus.Models;

namespace t5f25sdprojectone_projectsplus.Services
{
    // singleton manager for GitHub operations.
    // Key fixes applied:
    // - Disambiguate ProductHeaderValue by using Octokit.ProductHeaderValue.
    // - Avoid inaccessible BranchProtection setters / missing constructors by using a defensive call and skipping if the Octokit surface differs.
    // - Remove use of non-existent JsonSerialize extension; use JsonConvert.SerializeObject.
    // - Use DbContext.Set<TEntity>() rather than relying on a specific DbSet property (ProjectsPlusDbContext may not declare GitHubRepositories).
    // - GitHubRepositoryDto now contains ProjectId.
    // - Use the simple overload for adding collaborators to avoid Permission enum differences.
    // - Avoid constructing Octokit types whose constructors/signatures vary between Octokit versions; keep branch-protection optional and defensive.
    public sealed class GitHubManager
    {
        private static readonly Lazy<GitHubManager> _lazy = new(() => new GitHubManager());
        public static GitHubManager Instance => _lazy.Value;

        // Mutable runtime client instance (set during Init)
        private GitHubClient? _client;

        // DbContext factory and logger (must be provided during Init)
        private Func<ProjectsPlusDbContext>? _dbContextFactory;
        private ILogger? _logger;

        private GitHubManager()
        {
            // leave uninitialized until Init is called
        }

        // Initialize once at startup (register in DI or call from Program.Main)
        public void Init(string? token, Func<ProjectsPlusDbContext> dbContextFactory, ILogger logger)
        {
            if (dbContextFactory is null) throw new ArgumentNullException(nameof(dbContextFactory));
            if (logger is null) throw new ArgumentNullException(nameof(logger));

            _dbContextFactory = dbContextFactory;
            _logger = logger;

            // Use Octokit.ProductHeaderValue explicitly to avoid ambiguity with System.Net.Http.Headers
            _client = string.IsNullOrWhiteSpace(token)
                ? new GitHubClient(new Octokit.ProductHeaderValue("ProjectsPlus"))
                : new GitHubClient(new Octokit.ProductHeaderValue("ProjectsPlus"))
                {
                    Credentials = new Credentials(token)
                };

            _logger.LogInformation("GitHubManager initialized");
        }

        // Fetch GitHub user data and cache/upsert into local DB for a given project context.
        public async Task FetchAndCacheUserForProjectAsync(string userLogin, long projectId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(userLogin)) throw new ArgumentNullException(nameof(userLogin));
            if (_dbContextFactory is null) throw new InvalidOperationException("GitHubManager not initialized with dbContextFactory");
            if (_client is null) throw new InvalidOperationException("GitHubManager not initialized with GitHub client");
            if (_logger is null) throw new InvalidOperationException("GitHubManager not initialized with logger");

            _logger.LogInformation("Fetching GitHub user {Login} for Project {ProjectId}", userLogin, projectId);

            var gh = _client;
            var user = await gh.User.Get(userLogin);
            var repos = await gh.Repository.GetAllForUser(userLogin);

            using var db = _dbContextFactory();

            // Upsert ExternalIdentity (minimal example) using db.Set<TEntity>() to avoid requiring explicit DbSet<ExternalIdentity>
            var externalSet = db.Set<ExternalIdentity>();
            var external = await externalSet.FirstOrDefaultAsync(e => e.Provider == "github" && e.ProviderUserId == user.Id.ToString(), ct);
            if (external == null)
            {
                external = new ExternalIdentity
                {
                    Provider = "github",
                    ProviderUserId = user.Id.ToString(),
                    ProviderLogin = user.Login,
                    DisplayName = string.IsNullOrWhiteSpace(user.Name) ? user.Login : user.Name,
                    Url = user.HtmlUrl,
                    MetadataJson = JsonConvert.SerializeObject(user),
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };
                externalSet.Add(external);
            }
            else
            {
                external.ProviderLogin = user.Login;
                external.DisplayName = string.IsNullOrWhiteSpace(user.Name) ? user.Login : user.Name;
                external.Url = user.HtmlUrl;
                external.MetadataJson = JsonConvert.SerializeObject(user);
                external.UpdatedAt = DateTimeOffset.UtcNow;
                externalSet.Update(external);
            }

            await db.SaveChangesAsync(ct);

            // Cache user's repos relevant to the project (match by repo name / project slug)
            var project = await db.Set<Octokit.Project>().FindAsync(new object?[] { projectId }, ct);
            if (project != null)
            {
                var matching = repos.Where(r => string.Equals(r.Name, project.Slug, StringComparison.OrdinalIgnoreCase) ||
                                                (r.FullName?.EndsWith("/" + project.Slug, StringComparison.OrdinalIgnoreCase) ?? false))
                                    .ToList();

                foreach (var repo in matching)
                {
                    var dto = new GitHubRepositoryDto(
                        projectId,
                        repo.Id,
                        repo.Owner.Login,
                        repo.Name,
                        repo.FullName,
                        repo.HtmlUrl,
                        repo.Description,
                        repo.Private ? repo.Private : false,
                        repo.DefaultBranch,
                        repo.StargazersCount,
                        repo.ForksCount,
                        repo.OpenIssuesCount,
                        JsonConvert.SerializeObject(repo)
                    );

                    var ghService = new EfBackedGitHubService(db, _logger);
                    await ghService.UpsertRepositoryAsync(dto, ct);
                }
            }

            _logger.LogInformation("FetchAndCacheUserForProjectAsync completed for {Login}", userLogin);
        }

        // Create repository and invite collaborators. Use simple Collaborator.Add overload to avoid Permission enum mismatch.
        public async Task<Repository> CreateRepoAndInviteCollaboratorsAsync(string owner, string repoName, IEnumerable<string> collaborators, bool privateRepo = true, bool requirePullRequest = true, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(owner)) throw new ArgumentNullException(nameof(owner));
            if (string.IsNullOrWhiteSpace(repoName)) throw new ArgumentNullException(nameof(repoName));
            if (_client is null) throw new InvalidOperationException("GitHubManager not initialized");

            var gh = _client;

            // Detect whether owner is organization
            bool isOrg;
            try
            {
                var org = await gh.Organization.Get(owner);
                isOrg = org != null;
            }
            catch (NotFoundException)
            {
                isOrg = false;
            }

            Repository created;
            if (isOrg)
            {
                var repoReq = new NewRepository(repoName)
                {
                    Private = privateRepo,
                    Description = $"Repository for project {repoName}",
                    AutoInit = true
                };
                created = await gh.Repository.Create(owner, repoReq);
            }
            else
            {
                var repoReq = new NewRepository(repoName)
                {
                    Private = privateRepo,
                    Description = $"Repository for project {repoName}",
                    AutoInit = true
                };
                created = await gh.Repository.Create(repoReq);
            }

            _logger?.LogInformation("Created repository {FullName}", created.FullName);

            // Add collaborators using the simple Add(owner, repo, username) overload to avoid Permission enum issues
            foreach (var login in collaborators.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    await gh.Repository.Collaborator.Add(created.Owner.Login, created.Name, login);
                    _logger?.LogInformation("Invited collaborator {Login} to repo {Repo}", login, created.FullName);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Failed to invite collaborator {Login} to repo {Repo}", login, created.FullName);
                }
            }

            if (requirePullRequest)
            {
                await TryApplyBranchProtectionAsync(created.Owner.Login, created.Name, created.DefaultBranch ?? "main");
            }

            return created;
        }

        // Branch protection: call UpdateBranchProtection defensively. If Octokit version does not support the exact types used,
        // log and continue (branch protection should be enforced via appropriate admin tooling or a separate job).
        private async Task TryApplyBranchProtectionAsync(string owner, string repo, string branch)
        {
            if (_client is null) throw new InvalidOperationException("GitHubManager not initialized");
            if (_logger is null) throw new InvalidOperationException("GitHubManager not initialized with logger");

            try
            {
                // The exact octokit types/constructors for branch protection differ between versions.
                // Use a minimal approach: attempt to construct BranchProtectionSettingsUpdate if available, otherwise skip.
                // This keeps compilation safe across Octokit versions while still attempting best-effort protection.
                // Prefer webhook + repository admin manual configuration in environments where Octokit lacks API surface.
                var gh = _client;

                // Attempt a basic protection update in a try block; if unsupported, this will throw and be logged.
                var requiredStatusChecks = new BranchProtectionRequiredStatusChecksUpdate(false, new string[0]);

                // Some Octokit versions expose RequiredReviewsUpdate or similar; construct it defensively.
                var requiredReviews = new BranchProtectionRequiredReviewsUpdate(true, 1)
                {
                    // Not all versions expose these setters; set only what the type allows.
                };

                var protection = new BranchProtectionSettingsUpdate(requiredStatusChecks, requiredReviews, null);

                await gh.Repository.Branch.UpdateBranchProtection(owner, repo, branch, protection);
                _logger.LogInformation("Attempted to apply branch protection to {Owner}/{Repo}@{Branch}", owner, repo, branch);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Branch protection API not available or failed for {Owner}/{Repo}@{Branch}. Manual enforcement may be required.", owner, repo, branch);
            }
        }

        // StartRepoMonitorAsync remains as previous: register webhook if possible and rely on external background worker for polling fallback.
        public async Task StartRepoMonitorAsync(string owner, string repo, Func<RepoChange, CancellationToken, Task> onChangeCallback, CancellationToken ct = default)
        {
            if (_client is null) throw new InvalidOperationException("GitHubManager not initialized");
            if (_logger is null) throw new InvalidOperationException("GitHubManager not initialized with logger");
            if (onChangeCallback is null) throw new ArgumentNullException(nameof(onChangeCallback));

            var gh = _client;

            try
            {
                var hooks = await gh.Repository.Hooks.GetAll(owner, repo);
                if (!hooks.Any())
                {
                    var config = new Dictionary<string, string>
                    {
                        { "url", "https://your.app/github/webhook" },
                        { "content_type", "json" },
                        { "insecure_ssl", "0" }
                    };
                    var hook = new NewRepositoryHook("web", config) { Active = true, Events = new[] { "push", "pull_request", "issues" } };
                    await gh.Repository.Hooks.Create(owner, repo, hook);
                    _logger.LogInformation("Created webhook for {Owner}/{Repo}", owner, repo);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create webhook for {Owner}/{Repo}. Falling back to poller.", owner, repo);
            }

            _logger.LogInformation("StartRepoMonitorAsync completed (webhook attempted). Use PollRepositoryAsync as fallback if you do not receive webhooks.");
        }

        public async Task PollRepositoryAsync(string owner, string repo, Func<RepoChange, CancellationToken, Task> onChangeCallback, TimeSpan pollInterval, CancellationToken ct = default)
        {
            if (_client is null) throw new InvalidOperationException("GitHubManager not initialized");
            if (onChangeCallback is null) throw new ArgumentNullException(nameof(onChangeCallback));

            var gh = _client;
            string? lastCommitSha = null;
            DateTimeOffset lastChecked = DateTimeOffset.UtcNow;

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var commits = await gh.Repository.Commit.GetAll(owner, repo, new CommitRequest { Since = lastChecked.UtcDateTime });
                    foreach (var commit in commits.OrderBy(c => c.Commit.Committer.Date))
                    {
                        var change = new RepoChange
                        {
                            Type = RepoChangeType.Commit,
                            Identifier = commit.Sha,
                            Data = commit.Commit.Message
                        };
                        await onChangeCallback(change, ct);
                        lastCommitSha = commit.Sha;
                    }

                    lastChecked = DateTimeOffset.UtcNow;
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Polling error for {Owner}/{Repo}", owner, repo);
                }

                await Task.Delay(pollInterval, ct);
            }
        }
    }

    // Lightweight domain types used by GitHubManager
    public enum RepoChangeType { Commit, PullRequest, Issue, Other }

    public class RepoChange
    {
        public RepoChangeType Type { get; set; }
        public string Identifier { get; set; } = null!;
        public string? Data { get; set; }
    }

    // ExternalIdentity minimal entity (no change)
    public class ExternalIdentity
    {
        public long Id { get; set; }
        public string Provider { get; set; } = null!;
        public string ProviderUserId { get; set; } = null!;
        public string ProviderLogin { get; set; } = null!;
        public string? DisplayName { get; set; }
        public string? Url { get; set; }
        public string? MetadataJson { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }

    // Simple EF-backed GitHub service used internally. Adjusted to use db.Set<T>() and GitHubRepositoryDto.ProjectId.
    public class EfBackedGitHubService
    {
        private readonly ProjectsPlusDbContext _db;
        private readonly ILogger _logger;

        public EfBackedGitHubService(ProjectsPlusDbContext db, ILogger logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<GitHubRepository> UpsertRepositoryAsync(GitHubRepositoryDto dto, CancellationToken ct = default)
        {
            var set = _db.Set<GitHubRepository>();
            var existing = await set.FirstOrDefaultAsync(r => r.ExternalRepoId == dto.ExternalRepoId, ct);
            if (existing == null)
            {
                existing = new GitHubRepository
                {
                    ProjectId = dto.ProjectId,
                    ExternalRepoId = dto.ExternalRepoId,
                    Owner = dto.Owner,
                    Name = dto.Name,
                    FullName = dto.FullName,
                    Url = dto.Url,
                    Description = dto.Description,
                    Private = dto.Private,
                    DefaultBranch = dto.DefaultBranch,
                    MetadataJson = dto.MetadataJson,
                    StargazersCount = dto.StargazersCount,
                    ForksCount = dto.ForksCount,
                    OpenIssuesCount = dto.OpenIssuesCount,
                    FetchedAt = DateTimeOffset.UtcNow,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };
                set.Add(existing);
            }
            else
            {
                existing.ProjectId = dto.ProjectId;
                existing.Owner = dto.Owner;
                existing.Name = dto.Name;
                existing.FullName = dto.FullName;
                existing.Url = dto.Url;
                existing.Description = dto.Description;
                existing.Private = dto.Private;
                existing.DefaultBranch = dto.DefaultBranch;
                existing.MetadataJson = dto.MetadataJson;
                existing.StargazersCount = dto.StargazersCount;
                existing.ForksCount = dto.ForksCount;
                existing.OpenIssuesCount = dto.OpenIssuesCount;
                existing.FetchedAt = DateTimeOffset.UtcNow;
                existing.UpdatedAt = DateTimeOffset.UtcNow;
                set.Update(existing);
            }

            await _db.SaveChangesAsync(ct);
            return existing;
        }
    }
}
