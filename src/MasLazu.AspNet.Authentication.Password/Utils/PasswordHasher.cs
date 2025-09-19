using BCrypt.Net;

namespace MasLazu.AspNet.Authentication.Password.Utils;

/// <summary>
/// Utility class for hashing and verifying passwords using BCrypt
/// </summary>
public static class PasswordHasher
{
    private const int WorkFactor = 12; // BCrypt work factor (2^12 = 4096 iterations)

    /// <summary>
    /// Hashes a plain text password using BCrypt
    /// </summary>
    /// <param name="password">The plain text password to hash</param>
    /// <returns>The BCrypt hashed password</returns>
    public static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    /// <summary>
    /// Verifies a plain text password against a BCrypt hashed password
    /// </summary>
    /// <param name="hashedPassword">The BCrypt hashed password to verify against</param>
    /// <param name="providedPassword">The plain text password to verify</param>
    /// <returns>True if the password matches, false otherwise</returns>
    public static bool VerifyPassword(string hashedPassword, string providedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword);
    }

    /// <summary>
    /// Hashes a password with enhanced entropy (slower but more secure)
    /// </summary>
    /// <param name="password">The plain text password to hash</param>
    /// <returns>The BCrypt hashed password with higher work factor</returns>
    public static string HashPasswordEnhanced(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, 14); // 2^14 = 16384 iterations
    }

    /// <summary>
    /// Checks if a hash needs to be upgraded to a higher work factor
    /// </summary>
    /// <param name="hashedPassword">The hashed password to check</param>
    /// <returns>True if the hash should be upgraded, false otherwise</returns>
    public static bool NeedsRehash(string hashedPassword)
    {
        return BCrypt.Net.BCrypt.PasswordNeedsRehash(hashedPassword, WorkFactor);
    }

    /// <summary>
    /// Rehashes a password if it needs upgrading
    /// </summary>
    /// <param name="hashedPassword">The current hashed password</param>
    /// <param name="plainPassword">The plain text password</param>
    /// <returns>The rehashed password if upgrade needed, otherwise returns the original hash</returns>
    public static string RehashIfNeeded(string hashedPassword, string plainPassword)
    {
        if (NeedsRehash(hashedPassword))
        {
            return HashPassword(plainPassword);
        }
        return hashedPassword;
    }
}
