using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var customerManager = new DataManager<Customer>("customers.json");
        var accountManager = new DataManager<Account>("accounts.json");
        var transactionManager = new DataManager<Transaction>("transactions.json");

        var customers = customerManager.LoadData();
        var accounts = accountManager.LoadData();
        var transactions = transactionManager.LoadData();

        // Dane startowe
        if (customers.Count == 0)
        {
            var c1 = new Customer { FullName = "Jan Kowalski", Login = "jan123", Pin = "1234" };
            var a1 = new Account { AccountNumber = "PL93102000030000110000223344", CustomerId = c1.Id, Currency = "PLN" };
            a1.Deposit(3000.00m);

            var a2 = new Account { AccountNumber = "EU55102000030000110000889966", CustomerId = c1.Id, Currency = "EUR" };
            a2.Deposit(500.00m);

            customers.Add(c1);
            accounts.Add(a1);
            accounts.Add(a2);

            customerManager.SaveData(customers);
            accountManager.SaveData(accounts);
        }

        bool appRunning = true;
        while (appRunning)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=== NEXUS BANK - SYSTEM CYFROWY ===");
            Console.ResetColor();
            Console.WriteLine("1. Zaloguj się (Login + PIN)");
            Console.WriteLine("2. Zarejestruj nowego klienta");
            Console.WriteLine("3. Wyjdź z aplikacji");
            Console.Write("\nWybierz opcję (1-3): ");

            string startChoice = Console.ReadLine();

            if (startChoice == "1")
            {
                Console.Clear();
                Console.WriteLine("--- LOGOWANIE KLIENTA ---");
                Console.Write("Podaj unikalny login: ");
                string inputLogin = Console.ReadLine()?.Trim();
                Console.Write("Podaj 4-cyfrowy PIN: ");
                string inputPin = Console.ReadLine()?.Trim();

                // Weryfikacja po loginie i pinie
                var currentCustomer = customers.FirstOrDefault(c =>
                    c.Login.Equals(inputLogin, StringComparison.OrdinalIgnoreCase) && c.Pin == inputPin);

                if (currentCustomer == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n[BŁĄD]: Nieprawidłowy login lub PIN!");
                    Console.ResetColor();
                    Console.WriteLine("\nNaciśnij dowolny klawisz, aby wrócić...");
                    Console.ReadKey();
                    continue;
                }

                // Sesja klienta
                bool userSession = true;
                while (userSession)
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Witaj, {currentCustomer.FullName} (Login: {currentCustomer.Login})!");
                    Console.ResetColor();
                    Console.WriteLine("1. Wyświetl moje konta i salda");
                    Console.WriteLine("2. Wpłać pieniądze na konto");
                    Console.WriteLine("3. Wykonaj wypłatę środków");
                    Console.WriteLine("4. Otwórz nowe konto walutowe");
                    Console.WriteLine("5. Wymiana walut (Przelicznik)");
                    Console.WriteLine("6. Historia moich transakcji");
                    Console.WriteLine("7. Wyloguj się");
                    Console.Write("\nWybierz opcję (1-7): ");

                    string menuChoice = Console.ReadLine();

                    switch (menuChoice)
                    {
                        case "1":
                            Console.Clear();
                            Console.WriteLine($"--- TWOJE KONTA ---");
                            var userAccounts = accounts.Where(a => a.CustomerId == currentCustomer.Id).ToList();

                            foreach (var acc in userAccounts)
                            {
                                Console.WriteLine($"Konto: {acc.AccountNumber} | Saldo: {acc.Balance:F2} {acc.Currency}");
                            }
                            Console.WriteLine("\nNaciśnij dowolny klawisz...");
                            Console.ReadKey();
                            break;

                        case "2":
                            Console.Clear();
                            Console.WriteLine("--- WPŁATA PIENIĘDZY ---");
                            var myAccsForDeposit = accounts.Where(a => a.CustomerId == currentCustomer.Id).ToList();

                            for (int i = 0; i < myAccsForDeposit.Count; i++)
                            {
                                Console.WriteLine($"{i + 1}. Konto: {myAccsForDeposit[i].AccountNumber} ({myAccsForDeposit[i].Balance} {myAccsForDeposit[i].Currency})");
                            }
                            Console.Write("Wybierz numer konta do wpłaty: ");
                            if (int.TryParse(Console.ReadLine(), out int dIndex) && dIndex > 0 && dIndex <= myAccsForDeposit.Count)
                            {
                                var targetAcc = myAccsForDeposit[dIndex - 1];
                                Console.Write("Podaj kwotę do wpłaty: ");
                                if (decimal.TryParse(Console.ReadLine(), out decimal depAmount) && depAmount > 0)
                                {
                                    targetAcc.Deposit(depAmount);
                                    accountManager.SaveData(accounts);

                                    var tx = new Transaction
                                    {
                                        AccountNumber = targetAcc.AccountNumber,
                                        Amount = depAmount,
                                        Type = "WPŁATA"
                                    };
                                    transactions.Add(tx);
                                    transactionManager.SaveData(transactions);

                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine($"\nWpłacono pomyślnie! Nowe saldo: {targetAcc.Balance} {targetAcc.Currency}");
                                    Console.ResetColor();
                                }
                                else
                                {
                                    Console.WriteLine("Nieprawidłowa kwota.");
                                }
                            }
                            Console.WriteLine("\nNaciśnij dowolny klawisz...");
                            Console.ReadKey();
                            break;

                        case "3":
                            Console.Clear();
                            Console.WriteLine("--- WYPŁATA ŚRODKÓW ---");
                            var myAccsForWithdraw = accounts.Where(a => a.CustomerId == currentCustomer.Id).ToList();

                            for (int i = 0; i < myAccsForWithdraw.Count; i++)
                            {
                                Console.WriteLine($"{i + 1}. Konto: {myAccsForWithdraw[i].AccountNumber} ({myAccsForWithdraw[i].Balance} {myAccsForWithdraw[i].Currency})");
                            }
                            Console.Write("Wybierz numer konta: ");
                            if (int.TryParse(Console.ReadLine(), out int wIndex) && wIndex > 0 && wIndex <= myAccsForWithdraw.Count)
                            {
                                var selectedAcc = myAccsForWithdraw[wIndex - 1];
                                Console.Write("Podaj kwotę do wypłaty: ");
                                if (decimal.TryParse(Console.ReadLine(), out decimal amount))
                                {
                                    try
                                    {
                                        selectedAcc.Withdraw(amount);
                                        accountManager.SaveData(accounts);

                                        var tx = new Transaction
                                        {
                                            AccountNumber = selectedAcc.AccountNumber,
                                            Amount = amount,
                                            Type = "WYPŁATA"
                                        };
                                        transactions.Add(tx);
                                        transactionManager.SaveData(transactions);

                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine($"\nWypłacono! Nowe saldo: {selectedAcc.Balance} {selectedAcc.Currency}");
                                        Console.ResetColor();
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine($"\n[BŁĄD]: {ex.Message}");
                                        Console.ResetColor();
                                    }
                                }
                            }
                            Console.WriteLine("\nNaciśnij dowolny klawisz...");
                            Console.ReadKey();
                            break;

                        case "4":
                            Console.Clear();
                            Console.WriteLine("--- OTWARCIE NOWEGO KONTA WALUTOWEGO ---");
                            Console.Write("Podaj walutę nowego konta (np. USD, EUR, GBP, PLN): ");
                            string newCurr = Console.ReadLine()?.ToUpper().Trim();

                            if (!string.IsNullOrEmpty(newCurr))
                            {
                                string prefix = newCurr == "EUR" ? "EU" : (newCurr == "USD" ? "US" : "PL");
                                var newAccount = new Account
                                {
                                    AccountNumber = prefix + new Random().Next(10000000, 99999999) + "00",
                                    CustomerId = currentCustomer.Id,
                                    Currency = newCurr
                                };

                                accounts.Add(newAccount);
                                accountManager.SaveData(accounts);

                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"\nSukces! Otwarto nowe konto {newCurr}. Numer: {newAccount.AccountNumber}");
                                Console.ResetColor();
                            }
                            Console.WriteLine("\nNaciśnij dowolny klawisz...");
                            Console.ReadKey();
                            break;

                        case "5":
                            Console.Clear();
                            Console.WriteLine("--- WYMIANA WALUT ---");
                            var userAccs = accounts.Where(a => a.CustomerId == currentCustomer.Id).ToList();

                            if (userAccs.Count < 2)
                            {
                                Console.WriteLine("Musisz posiadać co najmniej dwa konta w różnych walutach.");
                            }
                            else
                            {
                                foreach (var a in userAccs)
                                {
                                    Console.WriteLine($"Konto: {a.AccountNumber} | {a.Balance} {a.Currency}");
                                }
                                Console.Write("\nWaluta ŹRÓDŁOWA (np. PLN): ");
                                string fromCurr = Console.ReadLine()?.ToUpper().Trim();
                                Console.Write("Waluta DOCELOWA (np. EUR): ");
                                string toCurr = Console.ReadLine()?.ToUpper().Trim();
                                Console.Write("Kwota do wymiany: ");

                                if (decimal.TryParse(Console.ReadLine(), out decimal exAmount))
                                {
                                    var srcAcc = userAccs.FirstOrDefault(a => a.Currency == fromCurr);
                                    var tgtAcc = userAccs.FirstOrDefault(a => a.Currency == toCurr);

                                    if (srcAcc != null && tgtAcc != null)
                                    {
                                        try
                                        {
                                            decimal rate = 1.0m;
                                            if (fromCurr == "PLN" && toCurr == "EUR") rate = 1 / 4.30m;
                                            else if (fromCurr == "EUR" && toCurr == "PLN") rate = 4.30m;
                                            else if (fromCurr == "PLN" && toCurr == "USD") rate = 1 / 4.00m;
                                            else if (fromCurr == "USD" && toCurr == "PLN") rate = 4.00m;

                                            srcAcc.Withdraw(exAmount);
                                            decimal finalVal = exAmount * rate;
                                            tgtAcc.Deposit(finalVal);

                                            accountManager.SaveData(accounts);

                                            var tx = new Transaction
                                            {
                                                AccountNumber = srcAcc.AccountNumber,
                                                Amount = exAmount,
                                                Type = $"WYMIANA ({fromCurr} -> {toCurr})"
                                            };
                                            transactions.Add(tx);
                                            transactionManager.SaveData(transactions);

                                            Console.ForegroundColor = ConsoleColor.Green;
                                            Console.WriteLine($"\nWymieniono pomyślnie na {finalVal:F2} {toCurr}!");
                                            Console.ResetColor();
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine($"\n[BŁĄD]: {ex.Message}");
                                            Console.ResetColor();
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Nie znaleziono kont dla podanych walut.");
                                    }
                                }
                            }
                            Console.WriteLine("\nNaciśnij dowolny klawisz...");
                            Console.ReadKey();
                            break;

                        case "6":
                            Console.Clear();
                            Console.WriteLine("--- HISTORIA TRANSAKCJI ---");
                            var myAccountNumbers = accounts.Where(a => a.CustomerId == currentCustomer.Id).Select(a => a.AccountNumber).ToList();
                            var myTransactions = transactions.Where(t => myAccountNumbers.Contains(t.AccountNumber)).ToList();

                            if (myTransactions.Count == 0)
                            {
                                Console.WriteLine("Brak historii transakcji dla Twoich kont.");
                            }
                            else
                            {
                                foreach (var t in myTransactions)
                                {
                                    Console.WriteLine($"[{t.Date}] Typ: {t.Type} | Konto: {t.AccountNumber} | Kwota: {t.Amount}");
                                }
                            }
                            Console.WriteLine("\nNaciśnij dowolny klawisz...");
                            Console.ReadKey();
                            break;

                        case "7":
                            userSession = false;
                            break;
                    }
                }
            }
            else if (startChoice == "2")
            {
                Console.Clear();
                Console.WriteLine("--- REJESTRACJA NOWEGO KLIENTA ---");
                Console.Write("Podaj imię i nazwisko: ");
                string newName = Console.ReadLine()?.Trim();

                Console.Write("Podaj UNIKALNY login: ");
                string newLogin = Console.ReadLine()?.Trim();

                // Sprawdzanie czy login już istnieje w bazie!
                bool loginExists = customers.Any(c => c.Login.Equals(newLogin, StringComparison.OrdinalIgnoreCase));
                if (loginExists)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n[BŁĄD]: Ten login jest już zajęty! Wybierz inny.");
                    Console.ResetColor();
                }
                else
                {
                    Console.Write("Ustaw 4-cyfrowy PIN: ");
                    string newPin = Console.ReadLine()?.Trim();

                    if (!string.IsNullOrEmpty(newName) && !string.IsNullOrEmpty(newLogin) && !string.IsNullOrEmpty(newPin))
                    {
                        var newCust = new Customer { FullName = newName, Login = newLogin, Pin = newPin };
                        var newAcc = new Account { AccountNumber = "PL" + new Random().Next(10000000, 99999999) + "00", CustomerId = newCust.Id, Currency = "PLN" };
                        newAcc.Deposit(500.00m);

                        customers.Add(newCust);
                        accounts.Add(newAcc);

                        customerManager.SaveData(customers);
                        accountManager.SaveData(accounts);

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"\nKonto utworzone! Login: {newLogin}, PIN: {newPin}");
                        Console.ResetColor();
                    }
                }
                Console.WriteLine("\nNaciśnij dowolny klawisz...");
                Console.ReadKey();
            }
            else if (startChoice == "3")
            {
                appRunning = false;
            }
        }
    }
}