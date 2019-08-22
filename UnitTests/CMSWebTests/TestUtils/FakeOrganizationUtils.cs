﻿using CmsData;
using CmsData.Codes;
using CmsData.Registration;
using CmsWeb.Areas.Dialog.Models;
using System.Collections.Generic;
using CmsWeb.Areas.OnlineReg.Models;
using UtilityExtensions;
using System.Linq;
using Xunit;

namespace CMSWebTests
{
    [Collection("Database collection")]
    public class FakeOrganizationUtils
    {
        public static NewOrganizationModel FakeNewOrganizationModel { get; set; }
        public static Organization FakeOrganization { get; set; }
        public static int? OrgId { get; set; }

        public static NewOrganizationModel MakeFakeOrganization(int registrationType)
        {
            if (FakeNewOrganizationModel == null)
            {
                var controller = new CmsWeb.Areas.Dialog.Controllers.AddOrganizationController(FakeRequestManager.FakeRequest());
                var routeDataValues = new Dictionary<string, string> { { "controller", "AddOrganization" } };
                controller.ControllerContext = ControllerTestUtils.FakeContextController(controller, routeDataValues);

                var NewOrganizationIndex = controller.Index();
                OrgId = ((NewOrganizationModel)((System.Web.Mvc.ViewResultBase)NewOrganizationIndex).Model).OrganizationId;

                FakeNewOrganizationModel = new NewOrganizationModel();
                FakeOrganization = new Organization() { OrganizationName = "MockName", RegistrationTitle = "MockTitle", Location = "MockLocation", RegistrationTypeId = registrationType };

                //Add fake registration settings
                SetFakeSettings(registrationType, FakeOrganization.OrganizationId);

                FakeNewOrganizationModel.org = FakeOrganization;
                controller.Submit((int)OrgId, FakeNewOrganizationModel);
            }
            else if (!OrgExists(OrgId))
            {
                FakeNewOrganizationModel = null;
                MakeFakeOrganization(registrationType );
            }
            return FakeNewOrganizationModel;
        }
        public static Settings SetFakeSettings(int regType, int orgId)
        {
            var m = new Settings();

            if (regType == RegistrationTypeCode.ChooseVolunteerTimes)
            {
                var timeSlots = new TimeSlots();
                var ts1 = new TimeSlots.TimeSlot() { DayOfWeek = 0, Name = "MockTimeSlot", Time = System.DateTime.Now };
                timeSlots.list.Add(ts1);
                m.TimeSlots = timeSlots;
            }

            m.OrgId = orgId;

            return m;
        }
        public static OnlineRegModel GetFakeOnlineRegModel(int OrgId)
        {
            var m = new OnlineRegModel(ContextTestUtils.FakeHttpContext().Request, CMSDataContext.Create(Util.Host), OrgId, null, null, null, null);
            m.UserPeopleId = 1;
            return m;
        }

        public static void DeleteOrg(int OrgId)
        {
            if (OrgExists(OrgId))
            {
                FakeOrganization.PurgeOrg(DbUtil.Db);
                FakeNewOrganizationModel = null;
            }
        }

        private static bool OrgExists(int? OrgId)
        {
            DbUtil.Db = CMSDataContext.Create(Util.Host);
            return DbUtil.Db.Organizations.Where(x => x.OrganizationId == OrgId).IsNotNull() ? true : false;
        }
    }
}
