namespace MoneyTracker.Infrastructure.MySql.Dtos;

public class AccountPathDto
{
    public string? Name { get; set; }            
    public int? AccountTypeId { get; set; }      
    public decimal? Balance { get; set; }        

}