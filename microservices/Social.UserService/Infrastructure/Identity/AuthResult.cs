namespace Social.UserService.Infrastructure.Identity
{
    public class AuthResult
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string JwtToken { get; set; }

        public AuthResult(Guid id, string userName, string jwtToken)
        {
            Id = id;
            UserName = userName;
            JwtToken = jwtToken;
        }
    }
}
