﻿using CmsData;
using CmsWeb.Membership;
using CmsWeb.Models;
using CmsWeb.Properties;
using System;
using System.Linq;
using UtilityExtensions;

namespace CmsWeb.MobileAPI
{
    public class MobileAccount
    {
        private readonly CMSDataContext db;

        public enum ResultCode
        {
            NoMatchNewPersonUserAccount,
            FoundPersonWithDiffEmailCreatedNewPersonUser,
            FoundPersonWithSameEmail,
            FoundMultipleMatches,
            NotifiedAboutExistingAccount,
            ExistingPersonNewUserAccount,
            BadEmailAddress
        }

        public string First { get; set; }
        public string Last { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime? Birthdate { get; set; }
        public User User { get; set; }
        public Person FoundPerson { get; set; }
        public Person NewPerson { get; set; }
        public ResultCode Result { get; set; }

        public MobileAccount(CMSDataContext dbContext)
        {
            db = dbContext;
        }

        public static MobileAccount Create(CMSDataContext dataContext, string first, string last, string email, string phone, string dob)
        {
            var m = new MobileAccount(dataContext)
            {
                First = first,
                Last = last,
                Email = email?.Trim(),
                Phone = phone.GetDigits()
            };
            DateTime bd;
            if (Util.DateValid(dob, out bd))
            {
                m.Birthdate = bd;
            }

            if (!Util.ValidEmail(m.Email))
            {
                m.Result = ResultCode.BadEmailAddress;
                return m;
            }
            m.FindPersonSendAccountInfo();
            return m;
        }

        private void FindPersonSendAccountInfo()
        {
            var foundpeopleids = db.FindPerson(First, Last, Birthdate, Email, Phone).Select(vv => vv.PeopleId.Value).ToArray();
            var foundpeople = (from pp in db.People
                               where foundpeopleids.Contains(pp.PeopleId)
                               select pp).ToList();

            // the simplest case is that we did not find an existing person
            Person p = null;

            if (foundpeople.Count == 0) //Test1 OK
            {
                Result = ResultCode.NoMatchNewPersonUserAccount;
                p = CreateNewPerson();
                CreateNewUserSendNewUserWelcome(p);
                return;
            }

            if (foundpeople.Count > 1)
            {
                Result = ResultCode.FoundMultipleMatches;
                return;
            }
            // we only matched on one person
            p = foundpeople[0];
            if (p.EmailAddress.Equal(Email) || p.EmailAddress2.Equal(Email))
            {
                // they match on an email plus everything else
                if (p.Users.Any())
                {
                    Result = ResultCode.NotifiedAboutExistingAccount; // Test3 OK
                    NotifyAboutExistingAccount(p);
                }
                else
                {
                    Result = ResultCode.ExistingPersonNewUserAccount; // Test2 OK
                    CreateNewUserSendNewUserWelcome(p);
                }
            }
            else
            {
                // they match on everything except email
                NotifyAboutDuplicateUser(p); // Test4
                var p2 = CreateNewPerson();
                CreateNewUserSendNewUserWelcome(p2);
                Result = ResultCode.FoundPersonWithDiffEmailCreatedNewPersonUser;
            }
        }

        private Person CreateNewPerson()
        {
            var p = Person.Add(db, null, First, null, Last, Birthdate);
            p.PositionInFamilyId = CmsData.Codes.PositionInFamily.PrimaryAdult;
            p.EmailAddress = Email;
            p.HomePhone = Phone;
            db.SubmitChanges();
            return NewPerson = p;
        }

        private void CreateNewUserSendNewUserWelcome(Person p)
        {
            User = MembershipService.CreateUser(db, p.PeopleId);
            db.SubmitChanges();
            AccountModel.SendNewUserEmail(db, User.Username);
        }

        private Person foundPersonWithDiffEmail;

        private void NotifyAboutExistingAccount(Person p)
        {
            var message = db.ContentHtml("ExistingUserConfirmation", Resource1.CreateAccount_ExistingUser);
            message = message
                .Replace("{name}", p.Name)
                .Replace("{host}", db.CmsHost);
            db.Email(DbUtil.AdminMail, p, "Account information for " + db.Host, message);
            User = p.Users.OrderByDescending(uu => uu.LastActivityDate).FirstOrDefault()
                   ?? MembershipService.CreateUser(db, p.PeopleId);
            Result = ResultCode.FoundPersonWithSameEmail;
        }

        private void NotifyAboutDuplicateUser(Person p)
        {
            var message = db.ContentHtml("NotifyDuplicateUserOnMobile", Resource1.NotifyDuplicateUserOnMobile);
            message = message.Replace("{otheremail}", Email);
            db.Email(DbUtil.AdminMail, p, "New User Account on " + db.Host, message);
            if (foundPersonWithDiffEmail == null)
            {
                foundPersonWithDiffEmail = p;
            }
        }
    }
}
