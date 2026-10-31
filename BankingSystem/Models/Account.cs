using System;

public class Account : IAccount
{
    public string AccountNumber { get; set; } = string.Empty;
    public decimal Balance { get; private set; }
    public string Currency { get; set; } = "PLN";
    public Guid CustomerId { get; set; }

    public void Deposit(decimal amount)
    {
        if (amount <= 0) throw new ArgumentException("Kwota wpłaty musi być większa od zera.");
        Balance += amount;
    }

    public void Withdraw(decimal amount)
    {
        if (amount <= 0) throw new ArgumentException("Kwota wypłaty musi być większa od zera.");
        if (Balance < amount)
            throw new InsufficientFundsException($"Brak wystarczających środków na koncie {AccountNumber}. Saldo: {Balance} {Currency}, próbiono wypłacić: {amount} {Currency}.");

        Balance -= amount;
    }
}
