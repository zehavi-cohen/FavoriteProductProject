namespace backend.DTOs.Admin;

public sealed record AdminUserDto
(
     int UserId,

     string UserName,

     string Email,

     bool IsActive,

     List<string> Roles
);