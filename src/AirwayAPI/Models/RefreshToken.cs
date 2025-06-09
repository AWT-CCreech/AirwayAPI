using System;
using System.Collections.Generic;

namespace AirwayAPI.Models;

public partial class RefreshToken
{
    public int Id { get; set; }

    public string Token { get; set; } = null!;

    public string Username { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public bool IsRevoked { get; set; }

    public DateTime CreatedAt { get; set; }
}
