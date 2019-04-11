using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace Lykke.Job.SolarCoinCashinGrabber.Core
{
    [BsonIgnoreExtraElements]
    public class TransactionMongoEntity
    {
        public TransactionMongoEntity()
        {
            Vouts = new List<MongoVout>();
            Vins = new List<MongoVin>();
        }

        [BsonId]
        public object Id { set; get; }

        [BsonElement("txid")]
        public string TxId { set; get; }

        [BsonElement("blockhash")]
        public string BlockHash { set; get; }

        [BsonElement("blockindex")]
        public long BlockIndex { set; get; }

        [BsonElement("timestamp")]
        public long Timestamp { set; get; }

        [BsonElement("total")]
        public decimal Total { set; get; }

        [BsonElement("vout")]
        public List<MongoVout> Vouts { set; get; }

        [BsonElement("vin")]
        public List<MongoVin> Vins { set; get; }

        [BsonElement("wasprocessed")]
        public bool WasProcessed { set; get; }
        
        public TransitQueueMessage ToTransitQueueMessage()
        {
            var result = new TransitQueueMessage { TxId = this.TxId };

            foreach (var vin in this.Vins)
                result.Vins.Add(new Vin { Address = vin.Addresses, Amount = vin.Amount });

            foreach (var vout in this.Vouts)
                result.Vouts.Add(new Vout { Address = vout.Addresses, Amount = vout.Amount });

            return result;
        }
    }

    public class MongoVout
    {
        [BsonElement("amount")]
        public long Amount { set; get; }

        [BsonElement("addresses")]
        public string Addresses { set; get; }
    }

    public class MongoVin
    {
        [BsonElement("amount")]
        public long Amount { set; get; }

        [BsonElement("addresses")]
        public string Addresses { set; get; }
    }
}
