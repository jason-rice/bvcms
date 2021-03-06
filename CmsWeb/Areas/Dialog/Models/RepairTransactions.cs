﻿using CmsData;
using System;
using System.Linq;
using System.Web.Hosting;

namespace CmsWeb.Areas.Dialog.Models
{
    public class RepairTransactions : LongRunningOperation
    {
        public const string Op = "RepairTransactions";

        public int OrgId { get; set; }
        public RepairTransactions() { }
        public RepairTransactions(int id, CMSDataContext db)
        {
            Host = db.Host;
            QueryId = Guid.NewGuid();
            OrgId = id;
            Count = db.OrganizationMembers.Count(m => m.OrganizationId == id);
        }

        public void Process(CMSDataContext db)
        {
            var lop = new LongRunningOperation
            {
                Host = db.Host,
                Started = DateTime.Now,
                Count = db.OrganizationMembers.Count(m => m.OrganizationId == OrgId),
                Processed = 0,
                QueryId = QueryId,
                Operation = Op
            };
            db.LongRunningOperations.InsertOnSubmit(lop);
            db.SubmitChanges();

            HostingEnvironment.QueueBackgroundWorkItem(ct => DoWork(this));
        }

        public static void DoWork(RepairTransactions model)
        {
            var db = CMSDataContext.Create(model.Host);
            db.RepairTransactions(model.OrgId);
            // finished
            var lop = FetchLongRunningOperation(db, Op, model.QueryId);
            lop.Completed = DateTime.Now;
            db.SubmitChanges();
        }
    }
}
