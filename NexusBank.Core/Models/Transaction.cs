using System;

public class Transaction : ITransaction
{
    public Guid TransactionId { get; set; } = Guid.NewGuid();
    public string AccountNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public string Type { get; set; } = string.Empty; // np. "WPŁATA", "WYPŁATA", "WYMIANA"
}