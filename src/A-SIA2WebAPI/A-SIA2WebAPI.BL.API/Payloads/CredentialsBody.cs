namespace A_SIA2WebAPI.BL.API.Payloads
{
    public partial class UserController
    {
        public class CredentialsBody
        {
            public string? Email { get; set; }
            public string? Password { get; set; }
        }
    }
}
