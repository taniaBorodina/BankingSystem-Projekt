using System;

using System;

public class Customer : ICustomer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty; // Nowość
    public string Pin { get; set; } = "1234";
}