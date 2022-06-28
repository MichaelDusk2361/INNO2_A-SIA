namespace A_SIA2WebAPI.Services
{
    public interface IJwtAuthenticationService
    {
        public string? Authenticate(string email, string password);
    }
}
