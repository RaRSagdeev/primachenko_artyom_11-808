﻿using System;
using MailDatabase;
using MailDatabase.Exceptions;
using GachiMail.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Linq;
using Microsoft.AspNetCore.Http;
namespace GachiMail.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Go(User user, bool savecookies)
        {
            try
            {
                DatabaseOperations.PasswordCheck(user.Login, user.Password);
            }
            catch(Exception ex)
            {
                if (ex is DatabaseException)
                {
                    HttpContext.Session.SetString("ErrorMessage", ex.Message);
                    return RedirectToAction("Index", "Login");
                }
            }
            HttpContext.Session.SetString("LI", "true");
            HttpContext.Session.SetString("User", user.Login);
            if(HttpContext.Session.Keys.Contains("ErrorMessage"))
            {
                HttpContext.Session.Remove("ErrorMessage");
            }
            if (savecookies)
                HttpContext.Response.Cookies
                    .Append("LP", JsonSerializer.Serialize<User>(user));
            return RedirectToAction("ProceedToMailbox", "Mailbox");
        }
    }
}