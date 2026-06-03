namespace WebApplication1.entites

    {
        public class Client
        {
            public int Id { get; set; }

            public string ClientCode { get; set; }

            public string ClientName { get; set; }

            public string ClientType { get; set; }

            // Login
            public string Email { get; set; }

            public string PhoneNumber { get; set; }

            public string PasswordHash { get; set; }

            // Verification
            public bool IsEmailVerified { get; set; }

            public string? EmailVerificationToken
            { get; set; }

            public DateTime?
                VerificationTokenExpiry
            { get; set; }

            // Security
            public bool IsActive { get; set; }
                = true;

            public bool IsLocked { get; set; }

            public DateTime? LastLoginDate
            { get; set; }

            // Audit
            public DateTime CreatedDate
            { get; set; }
                = DateTime.UtcNow;

            // Navigation
            public ICollection<ClientCompanyBranchAccess>
                ClientCompanyBranchAccesses
            { get; set; }
        }
    }


