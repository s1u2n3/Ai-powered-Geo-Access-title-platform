using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.GisEntities
{
    [Table("parcels")]
    public class Parcels
    {
        [Key]

        [Column("id")]
        public int Id { get; set; }

        [Column("parcel_id")]
        public string ParcelId { get; set; }

        [Column("apn")]
        public string APN { get; set; }

        [Column("address")]
        public string Address { get; set; }

        [Column("county_name")]
        public string CountyName { get; set; }

        [Column("state_name")]
        public string StateName { get; set; }

        [Column("latitude")]
        public double Latitude { get; set; }

        [Column("longitude")]
        public double Longitude { get; set; }

        [Column("has_starter")]
        public bool HasStarter { get; set; }

        [Column("starter_from_year")]
        public int? StarterFromYear { get; set; }

        [Column("starter_to_year")]
        public int? StarterToYear { get; set; }

        [Column("polygon")]
        public Geometry Polygon { get; set; }
    }
}