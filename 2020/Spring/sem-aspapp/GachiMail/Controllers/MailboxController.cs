﻿using System.Linq;
using Microsoft.AspNetCore.Mvc;
using GachiMail.Utilities;
using MailDatabase;
using GachiMail.Models;
using MailDatabase.LetterTypes;
using Microsoft.AspNetCore.Http;
using System;
using System.Reflection;
using MailDatabase.Exceptions;
namespace GachiMail.Controllers
{
    public class MailboxController : MailAccountController
    {
        private string user
        {
            get
            {
                return HttpContext.Session.GetString("User");
            }
        }
        private string box
        {
            get
            {
                   return DatabaseOperations
                        .GetMailboxesByUser(user)
                        .FirstOrDefault();
            }
        }
        public IActionResult Index()
        {
            return View(DatabaseOperations.GetMailboxesByUser(HttpContext.Session.GetString("User")));
        }
        public IActionResult ProceedToMailbox(string mailbox)
        {
            if (box == null)
                return RedirectToAction("MailboxCreate");
            if (mailbox == null)
                mailbox = box;
            HttpContext.Session.SetString("Box", mailbox);
            return RedirectToAction("ListMessages", new { mtype = "Incoming"});
        }


        public IActionResult ListMessages(string mtype)
        {
            return (IActionResult)GetType().GetMethod(mtype).Invoke(this, null);
        }
        [NonAction]
        public IActionResult Incoming()
        {
            var links = DatabaseOperations
               .GetMailIdsFromFolder<IncomingLetters>(HttpContext.Session.GetString("Box"))
               .Select(a => new LetterPreview(a))
               .ToList();
            ViewData["MessageType"] = "Incoming";
            return View("ListMessages", links);
        }
        [NonAction]
        public IActionResult Sent()
        {
            var links = DatabaseOperations
                .GetMailIdsFromFolder<SentLetters>(HttpContext.Session.GetString("Box"))
                .Select(a => new LetterPreview(a))
                .ToList();
            ViewData["MessageType"] = "Sent";
            return View("ListMessages", links);
        }

        public IActionResult MailboxCreate()
        {
            return View();
        }
        [HttpPost]
        public IActionResult MBGo(string mailbox, string user)
        {
            try
            {
                DatabaseOperations.AddMailbox(user, mailbox);
            }
            catch (Exception ex)
            {
                if (ex is DatabaseException)
                {
                    HttpContext.Session.SetString("ErrorMessage", ex.Message);
                    return RedirectToAction("MailboxCreate");
                }
            }
            if (HttpContext.Session.Keys.Contains("ErrorMessage"))
                HttpContext.Session.Remove("ErrorMessage");
            return RedirectToAction("ProceedToMailbox");
        }
    }
}