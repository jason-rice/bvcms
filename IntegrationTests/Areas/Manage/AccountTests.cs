﻿using IntegrationTests.Support;
using SharedTestFixtures;
using Shouldly;
using System.Linq;
using Xunit;

namespace IntegrationTests.Areas.Manage
{
    [Collection(Collections.Webapp)]
    public class AccountTests : AccountTestBase
    {
        [Fact, FeatureTest]
        public void MyData_User_Logon_Fail_Test()
        {
            username = RandomString();
            password = RandomString();
            var user = CreateUser();

            Login(withPassword: "bad password");

            PageSource.ShouldContain("Logon Error!");
        }

        [Fact, FeatureTest]
        public void MyData_User_Logon_Success_Test()
        {
            username = RandomString();
            password = RandomString();
            var user = CreateUser();

            Login();

            PageSource.ShouldContain(user.Person.Name);
            PageSource.ShouldContain(user.Person.EmailAddress);
        }

        [Fact, FeatureTest]
        public void MyData_User_Logout_Success_Test()
        {
            username = RandomString();
            password = RandomString();
            var user = CreateUser();

            Login();

            Logout();
        }

        [Fact, FeatureTest]
        public void MyData_User_ChangePassword_Test()
        {
            username = RandomString();
            password = RandomString();
            var user = CreateUser();

            Login();
            var newPassword = RandomString() + "1!";
            RepeatUntil(() => Find(css: profileMenu).Click(),
                condition: () => (Find(text: "Change Password") != null));
            Find(text: "Change Password").Click();

            CurrentUrl.ShouldBe($"{rootUrl}Account/ChangePassword/");
            Wait(1);

            Find(id: "currentPassword").SendKeys(password);
            Find(id: "newPassword").SendKeys(newPassword);
            Find(id: "confirmPassword").SendKeys(newPassword);
            Find(css: "input[type=submit]").Click();

            PageSource.ShouldContain("Password Changed");

            RepeatUntil(() => Find(text: "Return to Home").Click(),
                condition: () => Find(css: profileMenu) != null);

            Logout();
            Login(withPassword: newPassword);

            PageSource.ShouldContain(user.Person.Name);
            PageSource.ShouldContain(user.Person.EmailAddress);
        }

        [Fact, FeatureTest]
        public void Delete_Role_Test()
        {
            username = RandomString();
            password = RandomString();
            string roleName = "role_" + RandomString();
            var role = new CmsData.Role { RoleName = roleName };
            db.Roles.InsertOnSubmit(role);
            db.SubmitChanges();
            var user = CreateUser(username, password, roles: new string[] { "Access", "Edit", "Admin" });
            Login();

            Open($"{rootUrl}Roles");
            PageSource.ShouldContain(roleName);

            Find(css: $"a[id=\"{role.RoleId}\"].delete").Click();
            Wait(0.5);
            Find(css: "div.showSweetAlert.visible button.confirm").Click();
            WaitFor(d => PageSource.Contains("<h2>Deleted!</h2>"));

            role = db.Copy().Roles.SingleOrDefault(r => r.RoleName == roleName);
            role.ShouldBeNull();
        }

        [Fact, FeatureTest]
        public void MyData_User_ForgotPassword_Test()
        {
            username = RandomString();
            password = RandomString();

            var newPassword = RandomString() + "1!";
            var user = CreateUser();

            Open(rootUrl);
            WaitForElement("#inputEmail", 30);

            Find(text: "Forgot?").Click();
            CurrentUrl.ShouldBe($"{rootUrl}Account/ForgotPassword");

            var input = "[name=UsernameOrEmail]";
            var button = "input[type=submit]";

            RepeatUntil(() => {
                Find(css: input)?.SendKeys(username);
                Find(css: button)?.Click();
            }, () => PageSource.Contains("Password Sent"));

            db.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, user);
            user.ResetPasswordCode.ShouldNotBeNull();

            Open($"{rootUrl}Account/SetPassword/{user.ResetPasswordCode}");
            PageSource.ShouldContain("Confirm Password Reset");
            Find(css: "button[type=submit]").Click();
            CurrentUrl.ShouldBe($"{rootUrl}Account/SetPasswordConfirm");

            RepeatUntil(() => {
                Find(id: "newPassword").SendKeys(newPassword);
                Find(id: "confirmPassword").SendKeys(newPassword);
                Find(css: button).Click();
            }, () => PageSource.Contains("Password Changed"));

            Find(text: "Return to Home").Click();
            WaitForPageLoad();

            Logout();
            Login(withPassword: newPassword);

            PageSource.ShouldContain(user.Person.Name);
            PageSource.ShouldContain(user.Person.EmailAddress);
        }
    }
}
