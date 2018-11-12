﻿using CmsData;
using CmsWeb.Areas.Dialog.Models;
using CmsWeb.Areas.Search.Models;
using CmsWeb.Lifecycle;
using System.Web.Mvc;

namespace CmsWeb.Areas.Dialog.Controllers
{
    [RouteArea("Dialog", AreaPrefix = "RepairTransactionsOrgs"), Route("{action}/{id?}")]
    public class RepairTransactionsOrgsController : CmsStaffController
    {
        public RepairTransactionsOrgsController(RequestManager requestManager) : base(requestManager)
        {
        }

        [HttpPost, Route("~/RepairTransactionsOrgs")]
        public ActionResult Index(OrgSearchModel osm)
        {
            var model = new RepairTransactionsOrgs(osm);
            return View(model);
        }

        [HttpPost]
        public ActionResult Process(RepairTransactionsOrgs model)
        {
            model.UpdateLongRunningOp(DbUtil.Db, RepairTransactionsOrgs.Op);

            if (!model.Started.HasValue)
            {
                DbUtil.LogActivity("Repair Transactions for Orgs");
                model.Process(DbUtil.Db);
            }
            return View(model);
        }
    }
}
