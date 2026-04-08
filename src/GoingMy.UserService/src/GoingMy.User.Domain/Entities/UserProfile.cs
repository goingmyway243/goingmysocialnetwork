using GoingMy.User.Domain.Enums;

namespace GoingMy.User.Domain.Entities;

/// <summary>
/// Represents a user's social profile. Owns all non-authentication user data —
/// profile info, social counters, privacy settings, and activity timestamps.
/// The Id mirrors the corresponding ApplicationUser.Id in AuthService (same Guid).
/// </summary>
public class UserProfile
{
    public Guid Id { get; set; }

    public string Username { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? Bio { get; set; }

    public string? AvatarUrl { get; set; }

    public string? CoverUrl { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public Gender Gender { get; set; }

    public string? Location { get; set; }

    public string? WebsiteUrl { get; set; }

    public int FollowersCount { get; set; }

    public int FollowingCount { get; set; }

    public int PostsCount { get; set; }

    public bool IsVerified { get; set; }

    public bool IsPrivate { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    /// <summary>Creates a new user profile bootstrapped from AuthService registration data.</summary>
    public UserProfile(Guid id, string username, string firstName, string lastName)
    {
        Id = id;
        Username = username;
        FirstName = firstName;
        LastName = lastName;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>Required by EF Core.</summary>
    private UserProfile() { }

    public void UpdateProfile(
        string? firstName,
        string? lastName,
        string? bio,
        DateTime? dateOfBirth,
        Gender? gender,
        string? location,
        string? websiteUrl,
        bool? isPrivate)
    {
        if (firstName is not null) FirstName = firstName;
        if (lastName is not null) LastName = lastName;
        if (bio is not null) Bio = bio;
        if (dateOfBirth.HasValue) DateOfBirth = dateOfBirth.Value;
        if (gender.HasValue) Gender = gender.Value;
        if (location is not null) Location = location;
        if (websiteUrl is not null) WebsiteUrl = websiteUrl;
        if (isPrivate.HasValue) IsPrivate = isPrivate.Value;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAvatar(string avatarUrl)
    {
        AvatarUrl = avatarUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateCover(string coverUrl)
    {
        CoverUrl = coverUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementFollowers() => FollowersCount++;
    public void DecrementFollowers() => FollowersCount = Math.Max(0, FollowersCount - 1);
    public void IncrementFollowing() => FollowingCount++;
    public void DecrementFollowing() => FollowingCount = Math.Max(0, FollowingCount - 1);
    public void IncrementPosts() => PostsCount++;
    public void DecrementPosts() => PostsCount = Math.Max(0, PostsCount - 1);
}
