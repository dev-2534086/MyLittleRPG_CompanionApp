public class LoginResponse
{
    public UserDto user { get; set; }
}

public class UserDto
{
    public bool IsConnected { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
}
