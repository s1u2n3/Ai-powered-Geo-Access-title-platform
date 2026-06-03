namespace WebApplication1.Dto.Client
{
    public class CreateClientDto
    {
        public string ClientCode { get; set; }

        public string ClientName { get; set; }

        public string ClientType { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public List<ClientCompanyAccessDto>
            Companies
        { get; set; }
    }
}
