using DataWarehouse.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DataWarehouse.Domain.Entities.Actors
{
    public class BusinessPartner
    {
        public int BusinessPartnerId { get; set; }
        public string CardCode { get; set; } = string.Empty;
        public string CardName { get; set; } = string.Empty;
        public BusinessPartnerType CardType { get; set; }
        public string? Phone1 { get; set; }
        public string? EmailAddress { get; set; }
    }
}
