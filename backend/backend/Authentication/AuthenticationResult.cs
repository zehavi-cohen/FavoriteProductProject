using backend.DTOs.Auth;

namespace backend.Authentication;

public sealed record AuthenticationResult(
    AuthResponse Response,
    string AccessToken
);