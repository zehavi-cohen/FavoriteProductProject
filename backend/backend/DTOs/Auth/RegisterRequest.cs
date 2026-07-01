namespace backend.DTOs.Auth;

public sealed record RegisterRequest
(
     string UserName,

    string Email,

    string Password
);