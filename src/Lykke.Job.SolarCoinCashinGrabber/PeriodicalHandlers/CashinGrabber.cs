using Common;
using Lykke.Common.Log;
using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using AzureStorage.Queue;
using Common.Log;
using Lykke.Job.SolarCoinCashinGrabber.Core;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace Lykke.Job.SolarCoinCashinGrabber.PeriodicalHandlers
{
    public class CashinGrabber : IStartable, IStopable
    {
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IQueueExt _queueExt;
        private readonly TimerTrigger _timerTrigger;
        private readonly ILog _log;
        
        private const string ComponentName = "SolarCoinApi.CashInGrabber.Job";

        public CashinGrabber(
            IMongoDatabase mongoDatabase,
            IQueueExt queueExt,
            ILogFactory logFactory)
        {
            _mongoDatabase = mongoDatabase;
            _queueExt = queueExt;
            _log = logFactory.CreateLog(this);
            _timerTrigger = new TimerTrigger(nameof(CashinGrabber), TimeSpan.FromSeconds(10), logFactory);
            _timerTrigger.Triggered += Execute;
        }

        public void Start()
        {
            _timerTrigger.Start();
        }
        
        public void Stop()
        {
            _timerTrigger.Stop();
        }

        public void Dispose()
        {
            _timerTrigger.Stop();
            _timerTrigger.Dispose();
        }

        private async Task Execute(ITimerTrigger timer, TimerTriggeredHandlerArgs args, CancellationToken cancellationToken)
        {
            _log.Info(ComponentName, "Begining to process");
            
            var collection = _mongoDatabase.GetCollection<TransactionMongoEntity>("txes");
            
            var filterBuilder = Builders<TransactionMongoEntity>.Filter;
            var txesFilter = filterBuilder.Eq(x => x.WasProcessed, false) | filterBuilder.Exists(x => x.WasProcessed, false);
            
            var newTxes = collection.Find(txesFilter)
                .Limit(100)
                .ToList();
            
            foreach (var tx in newTxes)
            {
                try
                {
                    _log.Info(ComponentName, "Preparing to process", tx.TxId);

                    if(tx.Vins.Count < 500 && tx.Vouts.Count < 500)
                        await _queueExt.PutRawMessageAsync(JsonConvert.SerializeObject(tx.ToTransitQueueMessage()));

                    var filter = Builders<TransactionMongoEntity>.Filter.Eq("txid", tx.TxId);

                    var update = Builders<TransactionMongoEntity>.Update.Set("wasprocessed", true);

                    await collection.UpdateOneAsync(filter, update);

                    _log.Info(ComponentName, "Successfully processed", tx.TxId);
                }
                catch (Exception e)
                {
                    _log.Error(ComponentName, e, null, tx.TxId);
                }
            }

            _log.Info(ComponentName, $"{newTxes.Count} tx-es successfully processed!");
        }
    }
}
