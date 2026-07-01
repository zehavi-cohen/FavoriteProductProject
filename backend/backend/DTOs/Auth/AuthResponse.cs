namespace backend.DTOs.Auth;

public sealed record AuthResponse
(
     int UserId,

     string UserName,

     string Email,

     List<string> Roles,

     string Token,

     bool IsImpersonating=false,

     int? ImpersonatedByUserId = null,

     string? ImpersonatedByUserName=null
);