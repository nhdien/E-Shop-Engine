﻿using Newtonsoft.Json;

namespace E_Shop_Engine.Domain.DomainModel.Payment
{
    public class OperationDetails
    {
        [JsonProperty("control")]
        public string Control { get; set; }

        [JsonProperty("original_amount")]
        public decimal OriginalAmount { get; set; }

        [JsonProperty("original_currency")]
        public string OriginalCurrency { get; set; }

        [JsonProperty("type")]
        public string OperationType { get; set; }

        [JsonProperty("status")]
        public string OperationStatus { get; set; }
    }
}