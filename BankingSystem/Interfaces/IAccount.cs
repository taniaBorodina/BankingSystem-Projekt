using System;

public interface ICustomer
{
    Guid Id { get; }
    string FullName { get; set; }
    string Login { get; set; } 
    string Pin { get; set; }
}

public interface IAccount
{
    string AccountNumber { get; }
    decimal Balance { get; }
    string Currency { get; }
    Guid CustomerId { get; }
    void Deposit(decimal amount);
    void Withdraw(decimal amount);
}

public interface ITransaction
{
    Guid TransactionId { get; }
    string AccountNumber { get; }
    decimal Amount { get; }
    DateTime Date { get; }
    string Type { get; }
}