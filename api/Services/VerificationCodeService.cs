using System.Collections.Concurrent;

namespace Services
{
    /// <summary>
    /// Generic in-memory verification code store with TTL-based expiry.
    /// Registered as a singleton. Codes expire after <see cref="CodeTtl"/> and are
    /// lazily cleaned on every <see cref="GenerateCode"/> / <see cref="ValidateCode"/> call.
    /// </summary>
    public class VerificationCodeService
    {
        private static readonly TimeSpan CodeTtl = TimeSpan.FromSeconds(30);

        private readonly ConcurrentDictionary<Guid, (string Code, DateTime CreatedAt)> _codes = new();

        /// <summary>
        /// Returns true if a non-expired code already exists for this key.
        /// </summary>
        public bool HasActiveCode(Guid key)
        {
            return _codes.TryGetValue(key, out var entry)
                && DateTime.UtcNow - entry.CreatedAt <= CodeTtl;
        }

        /// <summary>
        /// Generates a 6-character alphanumeric verification code for a key (e.g. reservation ID).
        /// Overwrites any existing code for the same key.
        /// </summary>
        public string GenerateCode(Guid key)
        {
            Cleanup();
            var code = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
            _codes[key] = (code, DateTime.UtcNow);
            return code;
        }

        /// <summary>
        /// Validates the code for a key. Removes the code on success.
        /// Returns false if the code is wrong, missing, or expired.
        /// </summary>
        public bool ValidateCode(Guid key, string code)
        {
            Cleanup();

            if (!_codes.TryRemove(key, out var entry))
                return false;

            if (DateTime.UtcNow - entry.CreatedAt > CodeTtl)
                return false; // expired — don't put it back

            if (!string.Equals(entry.Code, code, StringComparison.OrdinalIgnoreCase))
            {
                // Put it back if the code was wrong — don't consume the token on failure
                _codes.TryAdd(key, entry);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Lazily removes expired entries from the store.
        /// </summary>
        private void Cleanup()
        {
            var now = DateTime.UtcNow;
            foreach (var kvp in _codes)
            {
                if (now - kvp.Value.CreatedAt > CodeTtl)
                    _codes.TryRemove(kvp.Key, out _);
            }
        }
    }
}
