using System;
using System.Collections.Generic;

namespace AirwayAPI.Models;

public partial class PortalUserFavorite
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int ItemId { get; set; }

    public int Ordering { get; set; }

    public virtual PortalItem Item { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
