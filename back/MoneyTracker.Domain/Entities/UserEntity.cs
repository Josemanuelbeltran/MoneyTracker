namespace MoneyTracker.Domain.Entities;

    public sealed class UserEntity
    {

        public int Id { get; set; }

        public string Name { get; set; } = default!;
        
        public string Email { get; set; } = default!;

        public string PasswordHash { get; set; } = default!;

        public string? PhoneNumber { get; set; }

        public bool? VerifiedPhone { get; set; }

        /// <summary>Administrador: decide qué categorías son comunes para todos.</summary>
        public bool IsAdmin { get; set; }
        
        public UserEntity() { }

        public UserEntity(string name, string passwordHash, string? phoneNumber = null, bool verifiedPhone = false)
        {
            Name = name;
            PasswordHash = passwordHash;
            PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber;
            VerifiedPhone = verifiedPhone;
        }
}
