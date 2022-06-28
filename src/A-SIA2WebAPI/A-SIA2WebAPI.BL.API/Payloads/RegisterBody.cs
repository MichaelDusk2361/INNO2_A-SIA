using System;

namespace A_SIA2WebAPI.BL.API.Payloads
{
    public partial class UserController
    {
        public class RegisterBody
        {
            public string? Email { get; set; }
            public string? Password { get; set; }
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public DateTime DateOfBirth { get; set; }
        }
    }
}
