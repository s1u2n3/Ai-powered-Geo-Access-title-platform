namespace WebApplication1.Dto.Client
{
    public class ClientCompanyAccessDto
    {
        public string CompanyName { get; set; }

        public List<ClientBranchAccessDto>
            Branches
        { get; set; }
    }
}
