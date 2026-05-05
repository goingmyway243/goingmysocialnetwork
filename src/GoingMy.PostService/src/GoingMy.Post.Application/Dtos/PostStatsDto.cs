namespace GoingMy.Post.Application.Dtos;

/// <summary>Stats response for the admin reporting endpoint.</summary>
public record PostStatsDto(
    long TotalPosts,
    long TotalLikes,
    long TotalComments,
    long PostsLast7Days,
    long PostsLast30Days);
