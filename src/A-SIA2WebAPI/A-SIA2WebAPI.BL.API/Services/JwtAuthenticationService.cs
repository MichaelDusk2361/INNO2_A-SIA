using A_SIA2WebAPI.DAL.Common;
using A_SIA2WebAPI.DAL.Neo4J.Repositories;
using A_SIA2WebAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace A_SIA2WebAPI.Services
{
    public class JwtAuthenticationService : IJwtAuthenticationService
    {
        public static byte[] TokenKey { get; } = Encoding.ASCII.GetBytes("a91069147f9bd9245cdacaef8ead4c3578ed44f179d7eb6bd4690e62ba4658f2");

        private readonly IPasswordHasher<User> _hasher;
        private readonly Neo4JUserRepository _userRepository;
        private readonly JwtSecurityTokenHandler _tokenHandler;
        private readonly ILogger<JwtAuthenticationService> _logger;

        public JwtAuthenticationService(
            IRepository<User> userRepository,
            IPasswordHasher<User> hasher,
            ILogger<JwtAuthenticationService> logger)
        {
            _logger = logger;

            // Get repository
            if (userRepository is not Neo4JUserRepository repo)
                throw new ArgumentException(nameof(repo));
            _userRepository = repo;

            // Configure Jwt
            _tokenHandler = new();
            _hasher = hasher;
        }

        public string? Authenticate(string email, string password)
        {
            try
            {
                // Retrieve user from database
                User? user = _userRepository.GetByEmail(email);

                // Check if user exists
                if (user == null)
                    return null;

                // Check if password is correct
                if (_hasher.VerifyHashedPassword(user, user.Hash, password) != PasswordVerificationResult.Success)
                    return null;

                // Create JWT
                SecurityTokenDescriptor tokenDescriptor = new()
                {
                    Subject = new ClaimsIdentity(
                        new Claim[]
                        {
                        new Claim(ClaimTypes.Name, email)
                        }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(TokenKey),
                        SecurityAlgorithms.HmacSha256Signature)
                };
                var token = _tokenHandler.CreateToken(tokenDescriptor);
                return _tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
            return null;
        }
    }
}
