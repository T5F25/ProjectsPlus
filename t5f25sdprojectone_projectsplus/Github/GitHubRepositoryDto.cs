using System;

namespace t5f25sdprojectone_projectsplus.Github
{
    // Minimal DTOs mapping the GitHub concepts we cache
    // DTO for upserting/caching a GitHub repository.
    // Note: ProjectId is included so upsert operations can associate a repo with a local Project.
    public record GitHubRepositoryDto(
        long ProjectId,
        long ExternalRepoId,
        string Owner,
        string Name,
        string FullName,
        string Url,
        string? Description,
        bool Private,
        string? DefaultBranch,
        int StargazersCount,
        int ForksCount,
        int OpenIssuesCount,
        string? MetadataJson
    );

    public record GitHubCommitDto(
        string Sha,
        string AuthorName,
        string AuthorEmail,
        DateTimeOffset CommitDate,
        string Message,
        string? Url,
        string? MetadataJson
    );

    public record GitHubIssueDto(
        long ExternalIssueId,
        string Title,
        string? Body,
        string State,
        DateTimeOffset CreatedAt,
        DateTimeOffset? ClosedAt,
        string? Url,
        string? MetadataJson
    );

    public record GitHubPullRequestDto(
        long ExternalPrId,
        string Title,
        string? Body,
        string State,
        string? HeadRef,
        string? BaseRef,
        DateTimeOffset CreatedAt,
        DateTimeOffset? MergedAt,
        string? Url,
        string? MetadataJson
    );

    public record GitHubFileReferenceDto(
        string Path,
        string Sha,
        long Size,
        string? Url,
        string? MetadataJson
    );
}
