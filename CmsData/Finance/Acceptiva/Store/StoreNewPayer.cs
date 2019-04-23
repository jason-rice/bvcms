﻿using CmsData.Finance.Acceptiva.Core;

namespace CmsData.Finance.Acceptiva.Store
{
    internal class StoreNewPayer : StorePayerRequest
    {
        public StoreNewPayer(string apiKey, Payer payer, string peopleId, CreditCard creditCard, Ach ach)
            :base(apiKey, payer, creditCard, ach)
        {
            Data["params[0][client_payer_id]"] = peopleId;
        }
    }
}
