namespace api_backend.Interfaces
{
    public interface IAuthService
    {
        Task<(string Token, DateTime expiration)> AuthenticateAsync(string Email, string Password);
    }
}
