using Microsoft.AspNetCore.Identity;

namespace CustodialTaskList.Security;

public class SupervisorPasswordService
{
    private readonly PasswordHasher<string> _hasher = new();

    // Convert a password into a secure hash.
    public string HashPassword(string password)
    {
        return _hasher.HashPassword("Supervisor", password);
    }

    // Verify an entered password against the saved hash.
    public bool VerifyPassword(
        string enteredPassword,
        string storedHash)
    {
        try
        {
            var result = _hasher.VerifyHashedPassword(
                "Supervisor",
                storedHash,
                enteredPassword);

            return result != PasswordVerificationResult.Failed;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}