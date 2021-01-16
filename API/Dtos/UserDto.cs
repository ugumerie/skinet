namespace API.Dtos
{
    //Q/A - scenerios where u will use 2 databases
    // - identity and angular materials for email verification, 3rd party login, password reset etc.
    // - maybe identity course with angular
    // - refresh token
    // why did we install Microsoft.AspNetCore.Identity cos its not updated since 2018 
    //Microsoft.AspNetCore.Identity.Entityframworkcore has all
    public class UserDto
    {
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string Token { get; set; }
    }
}