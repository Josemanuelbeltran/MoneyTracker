namespace MoneyTracker.Infrastructure.MySql.Dtos;

public class ExpenseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int AccountId { get; set; }
    public int CategoryId { get; set; }
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }

    public string? Description { get; set; }
    public bool IsRecurring { get; set; }
    public int? PocketId { get; set; }

    /// <summary>Mascota a la que va el gasto (id de PetRecording). NULL = no es de mascota.</summary>
    public int? PetId { get; set; }

    public decimal? TotalAmount { get; set; }   // total_amount (gasto parcial)
    public int? SplitParts { get; set; }        // split_parts   (gasto parcial)
    public string? SpreadGroup { get; set; }    // spread_group  (prorrateo)
    public int? SpreadIndex { get; set; }       // spread_index  (prorrateo)
    public int? SpreadCount { get; set; }       // spread_count  (prorrateo)

    public DateTime UpdatedAt { get; set; }
}
