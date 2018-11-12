using CmsData;
using CmsWeb.Areas.Dialog.Models;
using CmsWeb.Lifecycle;
using System.Linq;
using System.Web.Mvc;
using UtilityExtensions;

namespace CmsWeb.Areas.Dialog.Controllers
{
    // todo: use bootstrap
    [RouteArea("Dialog", AreaPrefix = "SearchUsers"), Route("{action=index}/{id?}")]
    public class SearchUsersController : CmsStaffController
    {
        public SearchUsersController(RequestManager requestManager) : base(requestManager)
        {
        }

        [HttpGet]
        public ActionResult Index(bool? singlemode, bool? ordered, int? topid)
        {
            Response.NoCache();
            var m = new SearchUsersModel
            {
                singlemode = singlemode ?? false,
                ordered = ordered ?? false,
                topid = topid
            };
            return View(m);
        }
        [HttpPost]
        public ActionResult Results(SearchUsersModel m)
        {
            return View("Index", m);
        }
        [HttpPost]
        public ActionResult MoveToTop(SearchUsersModel m)
        {
            return View("Results", m);
        }
        [HttpPost]
        public ActionResult TagUntag(int id, bool ischecked, bool isordered)
        {
            var t = DbUtil.Db.FetchOrCreateTag(Util.SessionId, Util.UserPeopleId, DbUtil.TagTypeId_AddSelected);
            var count = t.PersonTags.Count();
            var topid = "";
            var tp = DbUtil.Db.TagPeople.SingleOrDefault(tt => tt.PeopleId == id && tt.Id == t.Id);
            if (ischecked)
            {
                if (tp != null)
                {
                    DbUtil.Db.TagPeople.DeleteOnSubmit(tp);
                }
            }
            else if (tp == null)
            {
                if (count == 0 && isordered)
                {
                    topid = id.ToString();
                }

                tp = new TagPerson() { Id = t.Id, PeopleId = id };
                DbUtil.Db.TagPeople.InsertOnSubmit(tp);
            }
            DbUtil.Db.SubmitChanges();
            return Content(topid);
        }
    }
}
