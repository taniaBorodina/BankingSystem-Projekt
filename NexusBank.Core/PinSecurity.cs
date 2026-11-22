
using System;
using System.Security.Cryptography;

public static class PinSecurity
{
    private const int Iterations = 210000;
    private const int SaltSize = 16;
    private const int HashSize = 32;

    public static string HashPin(string pin)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            pin,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        return $"PBKDF2${Iterations}$" +
               $"{Convert.ToBase64String(salt)}$" +
               $"{Convert.ToBase64String(hash)}";
    }

    public static bool VerifyPin(string pin, string storedHash)
    {
        try
        {
            string[] parts = storedHash.Split('$');

            if (parts.Length != 4 || parts[0] != "PBKDF2")
                return false;

            int iterations = int.Parse(parts[1]);

            if (iterations != Iterations)
                return false;

            byte[] salt = Convert.FromBase64String(parts[2]);
            byte[] expectedHash = Convert.FromBase64String(parts[3]);

            if (salt.Length != SaltSize ||
                expectedHash.Length != HashSize)
                return false;

            byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
                pin,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(
                actualHash,
                expectedHash);
        }
        catch (FormatException)
        {
            return false;
        }
        catch (OverflowException)
        {
            return false;
        }
    }

    public static bool VerifyPinOrLegacy(
        string enteredPin, string storedPin)
    {
        if (string.IsNullOrEmpty(storedPin))
            return false;

        if (storedPin.StartsWith("PBKDF2$",
            StringComparison.Ordinal))
        {
            return VerifyPin(enteredPin, storedPin);
        }

        // Tymczasowa zgodność ze starymi kontami
        return enteredPin == storedPin;
    }

    public static bool IsLegacyPin(string storedPin)
    {
        return !string.IsNullOrEmpty(storedPin)
            && !storedPin.StartsWith(
                "PBKDF2$",
                StringComparison.Ordinal);
    }


}

