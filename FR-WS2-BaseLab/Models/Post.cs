using System;
using System.Collections.Generic;

namespace FR_WS2_BaseLab.Models;

public partial class Post
{
    public int Id { get; set; }

    public int TopId { get; set; }

    public string? UserId { get; set; }

    public bool Inactive { get; set; }

    public string Texte { get; set; } = null!;

    public DateTime Date { get; set; }

    public virtual Topic? Top { get; set; }

    public virtual AspNetUser? User { get; set; }
}
