﻿using CmsData;
using CmsData.Codes;
using CmsData.Finance;
using CmsData.Registration;
using CmsWeb.Areas.OnlineReg.Models;
using CmsWeb.Common;
using CmsWeb.Lifecycle;
using CmsWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using TransactionGateway;
using TransactionGateway.ApiModels;
using UtilityExtensions;

namespace CmsWeb.Areas.Setup.Controllers
{
    [RouteArea("Setup", AreaPrefix = "Pushpay")]
    public class PushpayController : CMSBaseController
    {
        private PushpayConnection _pushpay;
        private PushpayPayment _pushpayPayment;
        private PushpayResolver _resolver;

        private string _givingLink;
        private string _defaultMerchantHandle;
        private string _state;
        private string _ru;
        private bool isTesting;

        public PushpayController(IRequestManager requestManager) : base(requestManager)
        {
            PaymentProcessTypes processType = PaymentProcessTypes.OneTimeGiving;
            try
            {
                processType = (PaymentProcessTypes)int.Parse(requestManager.SessionProvider.Get<string>("PaymentProcessType"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }


            _pushpay = new PushpayConnection(
                CurrentDatabase.GetSetting("PushPayAccessToken", ""),
                CurrentDatabase.GetSetting("PushPayRefreshToken", ""),
                CurrentDatabase,
                Configuration.Current.PushpayAPIBaseUrl,
                Configuration.Current.PushpayClientID,
                Configuration.Current.PushpayClientSecret,
                Configuration.Current.OAuth2TokenEndpoint,
                Configuration.Current.TouchpointAuthServer,
                Configuration.Current.OAuth2AuthorizeEndpoint);

            _pushpayPayment = new PushpayPayment(_pushpay, CurrentDatabase, processType);
            _resolver = new PushpayResolver(_pushpay, CurrentDatabase);
            _defaultMerchantHandle = _pushpayPayment._merchantHandle;
            _givingLink = Configuration.Current.PushpayGivingLinkBase;
            _state = CurrentDatabase.Host;

            isTesting = MultipleGatewayUtils.GatewayTesting(CurrentDatabase, processType);
            if (isTesting)
                _ru = "touchpointest";
            else
                _ru = Configuration.Current.PushpayRU;
        }

        /// <summary>
        ///     Opens the developer console in a separate VIEW
        /// </summary>
        /// <returns></returns>
        public ActionResult DeveloperConsole()
        {
            return View();
        }

        /// <summary>
        ///     Entry point / home page into the application
        /// </summary>
        /// <returns></returns>
        [Route("~/Pushpay")]
        public ActionResult Index()
        {
            string redirectUrl = Configuration.Current.OAuth2AuthorizeEndpoint
                + "?client_id=" + Configuration.Current.PushpayClientID
                + "&response_type=code"
                + "&redirect_uri=" + Configuration.Current.TouchpointAuthServer
                + "&scope=" + Configuration.Current.PushpayScope
                + "&state=" + _state; //Get  xsrf_token:tenantID

            return Redirect(redirectUrl);
        }

        [Route("~/Pushpay/Complete")]
        public async Task<ActionResult> Complete(string state)
        {
            string redirectUrl;
            var tenantHost = state;
#if DEBUG
            redirectUrl = "http://" + Configuration.Current.TenantHostDev + "/Pushpay/Save";
#else
            redirectUrl = "https://" + tenantHost + "." + Configuration.Current.OrgBaseDomain + "/Pushpay/Save";
#endif            

            //Received authorization code from authorization server
            var authorizationCode = Request["code"];
            if (authorizationCode != null && authorizationCode != "")
            {
                //Get code returned from Pushpay                
                var at = await _pushpay.AuthorizationCodeCallback(authorizationCode);
                return Redirect(redirectUrl + "?_at=" + at.access_token + "&_rt=" + at.refresh_token);
            }
            return Redirect("~/Home/Index");
        }

        [Route("~/Pushpay/CompletePayment")]
        public ActionResult CompletePayment(string paymentToken, string sr)
        {
            string redirectUrl = "~/Home/Index";
#if DEBUG
            redirectUrl = $"http://{Configuration.Current.TenantHostDev}/Pushpay/ProcessPayment?paymentToken={paymentToken}&sr={sr}";
#else
            if (!string.IsNullOrEmpty(sr)&&!string.IsNullOrEmpty(paymentToken))
            {
                var state = sr.Split('_')[1];
                redirectUrl = $"https://{state}.{Configuration.Current.OrgBaseDomain}/Pushpay/ProcessPayment?paymentToken={paymentToken}&sr={sr}";
            }
#endif
            return Redirect(redirectUrl);
        }

        [Route("~/Pushpay/Save")]
        public ActionResult Save(string _at, string _rt)
        {
            string idAccessToken = "PushpayAccessToken", idRefreshToken = "PushpayRefreshToken";
            //var dbContext = Db;
            //var m = CurrentDatabase.Settings.AsQueryable();
            if (!Regex.IsMatch(idAccessToken, @"\A[A-z0-9-]*\z"))
            {
                return View("Invalid characters in setting id");
            }

            if (!CurrentDatabase.Settings.Any(s => s.Id == idAccessToken))
            {
                //Create access token
                var s = new Setting { Id = idAccessToken, SettingX = _at };
                CurrentDatabase.Settings.InsertOnSubmit(s);
                CurrentDatabase.SubmitChanges();
                CurrentDatabase.SetSetting(idAccessToken, _at);
            }
            else
            { // Update access token
                CurrentDatabase.SetSetting(idAccessToken, _at);
                CurrentDatabase.SubmitChanges();
                DbUtil.LogActivity($"Edit Setting {idAccessToken} to {_at}", userId: CurrentDatabase.UserId);
            }
            if (!CurrentDatabase.Settings.Any(s => s.Id == idRefreshToken))
            { //Create refresh token
                var s = new Setting { Id = idRefreshToken, SettingX = _rt };
                CurrentDatabase.Settings.InsertOnSubmit(s);
                CurrentDatabase.SubmitChanges();
                CurrentDatabase.SetSetting(idRefreshToken, _rt);
            }
            else
            { // Update refresh token
                CurrentDatabase.SetSetting(idRefreshToken, _rt);
                CurrentDatabase.SubmitChanges();
                DbUtil.LogActivity($"Edit Setting {idRefreshToken} to {_rt}", userId: CurrentDatabase.UserId);
            }
            return RedirectToAction("Finish");
        }

        [Route("~/Pushpay/Finish")]
        public ActionResult Finish()
        {
            ViewBag.Host = CurrentDatabase.Host;
            return View();
        }

        [Route("~/Pushpay/OneTime/{PeopleId:int}")]
        public ActionResult OneTime(int PeopleId)
        {
            var oid = CmsData.API.APIContribution.OneTimeGiftOrgId(CurrentDatabase);
            if (oid == null)
                return View("OnePageGiving/NotConfigured");

            SetGivingLink(oid.Value);
            string mobile = CurrentDatabase.People.SingleOrDefault(p => p.PeopleId == PeopleId).CellPhone;
            return Redirect($"{_givingLink}?ru={_ru}&sr=ot_{_state}_{PeopleId}&rcv=false&r=no&up={mobile}");
        }

        [Route("~/Pushpay/OnePage/{OrgId:int}")]
        public ActionResult OnePage(int OrgId)
        {
            SetGivingLink(OrgId);
            return Redirect($"{_givingLink}?rcv=false");
        }

        [Route("~/Pushpay/RecurringManagment/{PeopleId:int}")]
        public async Task<ActionResult> RecurringManagment(int PeopleId)
        {
            var oid = (from o in CurrentDatabase.Organizations
                       where o.RegistrationTypeId == RegistrationTypeCode.ManageGiving
                       select o.OrganizationId).FirstOrDefault();

            if (oid <= 0)
                return View("OnePageGiving/NotConfigured");

            ViewBag.PeopleId = PeopleId;
            ViewBag.OrgId = oid;

            string payerKey = PushpayResolver.ResolvePayerKey(CurrentDatabase, PeopleId);
            IEnumerable<RecurringPayment> rpList = await _pushpayPayment.GetRecurringPaymentsForAPayer(payerKey);
            if (rpList == null || rpList.Count() == 0)
            {
                string mobile = CurrentDatabase.People.SingleOrDefault(p => p.PeopleId == PeopleId).CellPhone;
                return NewRecurringGiving(PeopleId, oid);
            }
            List<RecurringManagment> model = new List<RecurringManagment>();
            foreach (var item in rpList)
            {
                RecurringManagment mg = new RecurringManagment()
                {
                    NextPayment = item.Schedule.NextPaymentDate,
                    Amount = item.Amount.Amount,
                    Fund = item.Fund.Name,
                    Frequency = item.Schedule.Frequency,
                    LinkToEdit = item.Links["donorviewrecurringpayment"].Href
                };
                model.Add(mg);
            }
            return View(model);
        }

        [Route("~/Pushpay/NewRecurringGiving/{PeopleId:int}/{OrgId:int}")]
        public ActionResult NewRecurringGiving(int PeopleId, int OrgId)
        {
            SetGivingLink(OrgId);
            string mobile = CurrentDatabase.People.SingleOrDefault(p => p.PeopleId == PeopleId).CellPhone;
            return Redirect($"{_givingLink}?ru={_ru}&sr=rp_{_state}_{PeopleId}&r=monthly&up={mobile}");
        }

        [Route("~/Pushpay/Registration/{DatumId:int}")]
        public async Task<ActionResult> Registration(int DatumId)
        {
            OnlineRegModel m = new OnlineRegModel(CurrentDatabase);
            decimal? Amount = 0;
            string mobile = string.Empty;
            string fundName = string.Empty;
            string merchantHandle = string.Empty;
            RegistrationDatum datum = CurrentDatabase.RegistrationDatas.SingleOrDefault(d => d.Id == DatumId);
            if (datum != null)
            {
                m = Util.DeSerialize<OnlineRegModel>(datum.Data);
                Amount = m.PayAmount() + (m.donation ?? 0);
                mobile = CurrentDatabase.People.SingleOrDefault(p => p.PeopleId == m.UserPeopleId)?.CellPhone;
                fundName = await _resolver.GetOrgFund(m.List.FirstOrDefault().setting.PushpayFundName);
                SetGivingLink(m.List.FirstOrDefault().org.OrganizationId);
                //Needs to redirect in case cupons are enable.
            }
            else
            {
                ViewBag.Message = "Something went wrong";
                CurrentDatabase.LogActivity($"No datum founded with id: {DatumId}");
                return View("~/Views/Shared/PageError.cshtml");
            }
            return Redirect($"{_givingLink}?ru={_ru}&sr=re_{_state}_{DatumId}&rcv=false&r=no&up={mobile}&a={Amount}&fnd={fundName}&al=true&fndv=lock");
        }

        [Route("~/Pushpay/PayAmtDue/{transactionId:int}/{amtdue:decimal}/{OrgId:int}")]
        public ActionResult PayAmtDue(int transactionId, decimal amtdue, int OrgId)
        {
            SetGivingLink(OrgId);
            return Redirect($"{_givingLink}?ru={_ru}&sr=pd_{_state}_{transactionId}&rcv=false&r=no&a={amtdue}&fndv=lock");
        }

        [Route("~/Pushpay/ProcessPayment")]
        public async Task<ActionResult> ProcessPayment(string paymentToken, string sr)
        {
            try
            {
                var reference = sr.Split('_')[0];
                int refId = Int32.Parse(sr.Split('_')[2]);
                Payment payment;
                switch (reference)
                {
                    case "ot":
                        var oid = CmsData.API.APIContribution.OneTimeGiftOrgId(CurrentDatabase);
                        if (oid == null)
                            return View("OnePageGiving/NotConfigured");

                        return await OneTimeProcess(paymentToken, oid.Value);

                    case "rp":
                        RecurringPayment recurringPayment = await _pushpayPayment.GetRecurringPayment(paymentToken);
                        return RecurringProcess(recurringPayment);

                    case "re":
                        return await RegistrationProcess(paymentToken, refId);

                    case "pd":
                        payment = await _pushpayPayment.GetPayment(paymentToken);
                        return PayAmtDueProcess(payment, refId);
                }
                throw new Exception("sr reference is not correct");
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Something went wrong";
                CurrentDatabase.LogActivity($"Error in pushpay payment process: {ex.Message}");
                return View("~/Views/Shared/PageError.cshtml");
            }
        }

        private async Task<ActionResult> OneTimeProcess(string paymentToken, int orgId)
        {
            Payment payment = await _pushpayPayment.GetPayment(paymentToken);
            if (payment != null && !_resolver.TransactionAlreadyImported(payment))
            {
                // determine the batch to put the payment in
                BundleHeader bundle;
                if (payment.Settlement?.Key.HasValue() == true)
                {
                    bundle = await _resolver.ResolveSettlement(payment.Settlement);
                }
                else
                {
                    // create a new bundle for each payment not part of a PushPay batch or settlement
                    bundle = _resolver.CreateBundle(payment.CreatedOn.ToLocalTime(), payment.Amount.Amount, null, null, payment.TransactionId, BundleReferenceIdTypeCode.PushPayStandaloneTransaction);
                }
                ContributionFund fund = _resolver.ResolveFund(payment.Fund);
                int? pid = _resolver.ResolvePersonId(payment.Payer);
                Contribution contribution = _resolver.ResolvePayment(payment, fund, pid, bundle);
                Transaction transaction = _resolver.ResolveTransaction(payment, pid.Value, orgId, "Online Giving");
            }
            ViewBag.Message = "Thank you, your transaction is complete for Online Giving.";
            return View();
        }

        private ActionResult RecurringProcess(RecurringPayment recurringPayment)
        {
            if (recurringPayment?.Schedule != null)
            {
                _resolver.ResolvePersonId(recurringPayment.Payer);
                ViewBag.Message = "Thanks for set up your recurring giving.";
                return View();
            }
            else
            {
                ViewBag.Message = "Something went wrong";
                CurrentDatabase.LogActivity($"Payment not found");
                return View("~/Views/Shared/PageError.cshtml");
            }
        }

        private async Task<ActionResult> RegistrationProcess(string paymentToken, int datumId)
        {
            OnlineRegModel m = new OnlineRegModel(CurrentDatabase);
            RegistrationDatum datum = CurrentDatabase.RegistrationDatas.SingleOrDefault(d => d.Id == datumId);
            m = Util.DeSerialize<OnlineRegModel>(datum.Data);
            m.CurrentDatabase = CurrentDatabase;
            PaymentForm pf = PaymentForm.CreatePaymentForm(m);

            Payment payment = await _pushpayPayment.GetPayment(paymentToken);
            if (payment == null || _resolver.TransactionAlreadyImported(payment))
            {
                ViewBag.Message = "Payment not valid or expired";
                return View("~/Views/Shared/PageError.cshtml");
            }

            m.transactionId = CreateTransaction(payment, pf);
            m.UpdateDatum();

            return Redirect($"/OnlineReg/ProcessExternalPayment/dat_{datumId}");
        }

        private ActionResult PayAmtDueProcess(Payment payment, int transactionId)
        {
            if (payment == null || _resolver.TransactionAlreadyImported(payment))
            {
                ViewBag.Message = "Payment not valid or expired";
                return View("~/Views/Shared/PageError.cshtml");
            }

            Transaction ti = CurrentDatabase.Transactions.Where(p => p.Id == transactionId).FirstOrDefault();
            PaymentForm pf = PaymentForm.CreatePaymentFormForBalanceDue(CurrentDatabase, ti, payment.Amount.Amount, payment.Payer.emailAddress);
            int tranId = CreateTransaction(payment, pf);
            return Redirect($"/OnlineReg/ProcessExternalPayment/tra_{tranId}");
        }

        private int CreateTransaction(Payment payment, PaymentForm pf)
        {
            int? PersonId = _resolver.ResolvePersonId(payment.Payer);
            Person person = CurrentDatabase.LoadPersonById(PersonId.Value);
            decimal amount = payment.Amount.Amount;

            decimal? amtdue = null;
            if (pf.Amtdue > 0)
            {
                amtdue = pf.Amtdue - amount;
            }

            var ti = new Transaction();

            ti.TransactionId = payment.TransactionId;
            ti.Name = person.Name;
            ti.First = person.FirstName;
            ti.MiddleInitial = !string.IsNullOrEmpty(person.MiddleName) ? person.MiddleName[0].ToString() : null;
            ti.Last = person.LastName;
            ti.Suffix = person.SuffixCode;
            ti.Donate = pf.Donate;
            ti.Amtdue = pf.AmtToPay;
            ti.Amt = amount;
            ti.Emails = person.EmailAddress;
            ti.Testing = false;
            ti.Description = pf.Description;
            ti.OrgId = pf.OrgId;
            ti.Url = pf.URL;
            ti.Address = person.AddressLineOne;
            ti.TransactionGateway = "pushpay";
            ti.City = person.CityName;
            ti.State = person.StateCode;
            ti.Zip = person.ZipCode;
            ti.DatumId = pf.DatumId;
            ti.Phone = person.HomePhone;
            ti.OriginalId = pf.OriginalId;
            ti.Financeonly = pf.FinanceOnly;
            ti.TransactionDate = Util.Now;
            ti.PaymentType = payment.PaymentMethodType == "CreditCard" ? PaymentType.CreditCard : PaymentType.Ach;
            ti.LastFourCC =
                payment.PaymentMethodType == "CreditCard" ? payment.Card.Reference.Substring(payment.Card.Reference.Length - 4) : null;
            ti.LastFourACH = null;
            ti.Approved = true;

            CurrentDatabase.Transactions.InsertOnSubmit(ti);
            CurrentDatabase.SubmitChanges();
            if (pf.OriginalId == null) // first transaction
            {
                ti.OriginalId = ti.Id;
            }

            return ti.Id;
        }

        private void SetGivingLink(int orgId)
        {
            var merchantHandle = GetMerchantHandle(orgId);

            if (string.IsNullOrEmpty(merchantHandle))
                merchantHandle = _defaultMerchantHandle;

            _givingLink += merchantHandle;
        }

        private string GetMerchantHandle(int orgId)
        {
            var regSettingsXml = CurrentDatabase.Organizations.FirstOrDefault(o => o.OrganizationId == orgId).RegSettingXml;
            return Util.DeSerialize<Settings>(regSettingsXml).PushpayMerchantName;
        }
    }
}
