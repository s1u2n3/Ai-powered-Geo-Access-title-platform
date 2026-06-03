namespace WebApplication1.Dto
{
    public class StateAndCountyResponse
    {
        // STATE

        public string StateCode { get; set; }

        public string StateName { get; set; }

        public double StateLatitude { get; set; }

        public double StateLongitude { get; set; }

        // COUNTIES

        public List<CountyItemDto> Counties { get; set; }
    }

    public class CountyItemDto
    {
        public string CountyCode { get; set; }

        public string CountyName { get; set; }

        public double CountyLatitude { get; set; }

        public double CountyLongitude { get; set; }
    }
}
