﻿using IntegrationTests.Support;
using Shouldly;
using Xunit;

namespace IntegrationTests.Areas.Manage
{
    [Collection("WebApp Collection")]
    public class AccountTests : AccountTestBase
    {
        [Fact]
        public void MyData_User_Logon_Fail_Test()
        {
            username = RandomString();
            password = RandomString();
            var user = CreateUser();

            Login(withPassword: "bad password");

            PageSource.ShouldContain("Logon Error!");
        }

        [Fact]
        public void MyData_User_Logon_Success_Test()
        {
            username = RandomString();
            password = RandomString();
            var user = CreateUser();

            Login();

            PageSource.ShouldContain(user.Person.Name);
            PageSource.ShouldContain(user.Person.EmailAddress);
        }

        [Fact]
        public void MyData_User_Logout_Success_Test()
        {
            username = RandomString();
            password = RandomString();
            var user = CreateUser();

            Login();

            Logout();
        }

        [Fact]
        public void MyData_User_ChangePassword_Test()
        {
            username = RandomString();
            password = RandomString();
            var user = CreateUser();

            Login();
            var newPassword = RandomString() + "1!";
            Find(css: profileMenu).Click();
            Find(text: "Change Password").Click();

            CurrentUrl.ShouldBe($"{rootUrl}Account/ChangePassword/");

            Find(id: "currentPassword").SendKeys(password);
            Find(id: "newPassword").SendKeys(newPassword);
            Find(id: "confirmPassword").SendKeys(newPassword);
            Find(css: "input[type=submit]").Click();

            PageSource.ShouldContain("Password Changed");

            Find(text: "Return to Home").Click();

            Logout();
            Login(withPassword: newPassword);

            PageSource.ShouldContain(user.Person.Name);
            PageSource.ShouldContain(user.Person.EmailAddress);
        }
    }
}