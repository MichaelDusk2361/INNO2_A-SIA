using A_SIA2WebAPI.DAL.Common;
using A_SIA2WebAPI.DAL.Neo4J;
using A_SIA2WebAPI.DAL.Neo4J.Repositories;
using A_SIA2WebAPI.Models;
using A_SIA2WebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;

namespace A_SIA2WebAPI.BL.API.Controllers
{
    public abstract class ExtendedControllerBase : ControllerBase
    {
        protected Neo4JUserRepository _userRepository { get; }

        protected ExtendedControllerBase(IRepository<User> userRepository)
        {
            // Get repository
            if (userRepository is not Neo4JUserRepository repo)
                throw new ArgumentException(nameof(repo));
            _userRepository = repo;
        }
        private User? _loggedInUser;
        protected User? LoggedInUser
        {
            get
            {
                if (_loggedInUser != null)
                    return _loggedInUser;

                if (User == null)
                    return null;

                string? email = User.Claims.Where(c => c.Type == ClaimTypes.Name).FirstOrDefault()?.Value;

                if (email == null)
                    return null;

                // Retrieve user from database
                return _loggedInUser = _userRepository.GetByEmail(email);
            }
        }       
    }
}
