﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PLM;
using Microsoft.AspNet.Identity;
using System.Data.Entity.Infrastructure;
using PLM.CutomAttributes;

namespace PLM.Controllers
{

    public class ModulesEDITController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: /ModulesEDIT/
        [AuthorizeOrRedirectAttribute(Roles = "Admin")]
        public ActionResult Index(string sortOrder, string searchString, string userSearchString)
        {
            ViewBag.NameSortParam = String.IsNullOrEmpty(sortOrder) ? "name_asc" : "";
            IEnumerable<Module> modules;
            using (Repos repo = new Repos())
            {
                modules = repo.GetModuleList();
            }

            if (!String.IsNullOrEmpty(searchString))
            {
                modules = modules.Where(m => m.Name.Contains(searchString));
            }

            if (!String.IsNullOrEmpty(userSearchString))
            {
                modules = modules.Where(m => m.User.UserName.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_asc":
                    modules = modules.OrderBy(m => m.Name);
                    break;
            }
            return View(modules);
        }
        // GET: /ModulesEDIT/
        [AuthorizeOrRedirectAttribute(Roles = "Admin")]
        public ActionResult DisabledModulesList(string sortOrder, string searchString, string userSearchString)
        {
            ViewBag.NameSortParam = String.IsNullOrEmpty(sortOrder) ? "name_asc" : "";

            IEnumerable<Module> modules;
            using (Repos repo = new Repos())
            {
                modules = repo.GetModuleList();
            }

            if (!String.IsNullOrEmpty(searchString))
            {
                modules = modules.Where(m => m.Name.Contains(searchString) && m.isDisabled == true);
            }

            if (!String.IsNullOrEmpty(userSearchString))
            {
                modules = modules.Where(m => m.User.UserName.Contains(searchString) && m.isDisabled == true);
            }

            switch (sortOrder)
            {
                case "name_asc":
                    modules = modules.OrderBy(m => m.Name);
                    break;
            }
            return View(modules);
        }
        // GET: /ModulesEDIT/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            int ID = id ?? 0;
            Module module;
            using (Repos repo = new Repos())
            {
                module = repo.GetModuleByID(ID);
                module.Answers = repo.GetAnswerList(ID).ToList();
                foreach (Answer answer in module.Answers)
                {
                    answer.Pictures = repo.GetPicturesByAnswerID(answer.AnswerID).ToList();
                }
            }
            if (module == null)
            {
                return HttpNotFound();
            }
            return View(module);
        }

        // GET: /ModulesEDIT/Create
        [AuthorizeOrRedirectAttribute(Roles = "Instructor")]
        public ActionResult Create()
        {
            PopulateCategoryDropDownList();
            return View();
        }

        // POST: /ModulesEDIT/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeOrRedirectAttribute(Roles = "Instructor")]
        public ActionResult Create([Bind(Include = "ModuleID,Name,CategoryId,Description,DefaultNumAnswers,DefaultTime,DefaultNumQuestions,isPrivate")] Module module)
        {
            if (ModelState.IsValid)
            {
                //**********NOTE****************//
                //Make sure that a user is logged in to access this page
                if (((User.IsInRole("Learner")) || (User.IsInRole("Admin"))))
                {
                    var userID = User.Identity.GetUserId();
                    ApplicationUser currentUser = db.Users.Single(x => x.Id == userID);
                    module.User = currentUser;
                }
                using (Repos repo = new Repos())
                {
                    if (!repo.AddModule(module))
                    {
                        //ERROR SAVING TO DATABASE
                    }
                }
                PopulateCategoryDropDownList(module.CategoryId);
                return RedirectToAction("Index", "Profile");
            }
            return View(module);
        }

        // GET: /ModulesEDIT/Edit/5
        [AuthorizeOrRedirectAttribute(Roles = "Instructor")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            int ID = id ?? 0;
            Module module;
            using (Repos repo = new Repos())
            {
                module = repo.GetModuleByID(ID);
                module.Answers = repo.GetAnswerList(ID).ToList();
                foreach (Answer answer in module.Answers)
                {
                    answer.Pictures = repo.GetPicturesByAnswerID(answer.AnswerID).ToList();
                }
            }
            if (module == null)
            {
                return HttpNotFound();
            }
            PopulateCategoryDropDownList(module.CategoryId);
            return View(module);
        }
        // POST: /ModulesEDIT/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeOrRedirectAttribute(Roles = "Instructor")]
        public ActionResult Edit([Bind(Include = "ModuleID,Name,CategoryId,Description,DefaultNumAnswers,DefaultTime,DefaultNumQuestions,isPrivate")] Module module)
        {
            if (ModelState.IsValid)
            {
                using (Repos repo = new Repos())
                {
                    if (!repo.UpdateModule(module))
                    {
                        //ERROR SAVING TO DATABASE
                    }
                }
                return RedirectToAction("Index", new { controller = "Profile" });
            }
            PopulateCategoryDropDownList(module.CategoryId);
            return View(module);
        }

        [AuthorizeOrRedirectAttribute(Roles = "Admin")]
        public ActionResult DisableModule(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            int ID = id ?? 0;
            Module module;
            using (Repos repo = new Repos())
            {
                module = repo.GetModuleByID(ID);
                module.Answers = repo.GetAnswerList(ID).ToList();
                foreach (Answer answer in module.Answers)
                {
                    answer.Pictures = repo.GetPicturesByAnswerID(answer.AnswerID).ToList();
                }
            }
            var model = new DisableModuleViewModel(module);
            if (module == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }

        // POST: /ModulesEDIT/ModuleDisable/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeOrRedirectAttribute(Roles = "Admin")]
        public ActionResult DisableModule([Bind(Include = "Name, isDisabled, DisableModuleNote, DisableReason")] DisableModuleViewModel userModule)
        {
            if (ModelState.IsValid)
            {
                Module module;
                using (Repos repo = new Repos())
                {
                    module = repo.GetModuleByID(userModule.ModuleID);
                    module.Answers = repo.GetAnswerList(userModule.ModuleID).ToList();
                    foreach (Answer answer in module.Answers)
                    {
                        answer.Pictures = repo.GetPicturesByAnswerID(answer.AnswerID).ToList();
                    }
                }

                module.isDisabled = userModule.isDisabled;
                module.DisableModuleNote = userModule.DisableModuleNote;
                module.DisableReason = userModule.DisableReason;

                using (Repos repo = new Repos())
                {
                    if (!repo.UpdateModule(module))
                    {
                        //ERROR SAVING TO DATABASE
                    }
                }
                return RedirectToAction("Index", new { controller = "ModulesEdit" });
            }
            return View(userModule);
        }

        private void PopulateCategoryDropDownList(object selectedCategory = null)
        {
            IEnumerable<Category> categoryQuery;
            using (Repos repo = new Repos())
            {
                categoryQuery = repo.GetCategoryList();
            }
            ViewBag.CategoryId = new SelectList(categoryQuery.Distinct().ToList(), "CategoryId", "CategoryName", selectedCategory);
        }

        // GET: /ModulesEDIT/Delete/5
        [AuthorizeOrRedirectAttribute(Roles = "Instructor")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            int ID = id ?? 0;
            Module module;
            using (Repos repo = new Repos())
            {
                module = repo.GetModuleByID(ID);
                module.Answers = repo.GetAnswerList(ID).ToList();
                foreach (Answer answer in module.Answers)
                {
                    answer.Pictures = repo.GetPicturesByAnswerID(answer.AnswerID).ToList();
                }
            }
            if (module == null)
            {
                return HttpNotFound();
            }
            return View(module);
        }
        // POST: /ModulesEDIT/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeOrRedirectAttribute(Roles = "Instructor")]
        public ActionResult DeleteConfirmed(int id)
        {
            using (Repos repo = new Repos())
            {
                if (!repo.DeleteModule(id))
                {
                    //ERROR DELETING MODULE
                }
            }
            return RedirectToAction("Index", new { controller = "Profile" });
        }

        [AuthorizeOrRedirectAttribute(Roles = "Admin")]
        public ActionResult AdminDelete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            int ID = id ?? 0;
            Module module;
            using (Repos repo = new Repos())
            {
                module = repo.GetModuleByID(ID);
                module.Answers = repo.GetAnswerList(ID).ToList();
                foreach (Answer answer in module.Answers)
                {
                    answer.Pictures = repo.GetPicturesByAnswerID(answer.AnswerID).ToList();
                }
            }
            if (module == null)
            {
                return HttpNotFound();
            }
            return View(module);
        }
        // POST: /ModulesEDIT/Delete/5
        [HttpPost, ActionName("AdminDelete")]
        [ValidateAntiForgeryToken]
        [AuthorizeOrRedirectAttribute(Roles = "Admin")]
        public ActionResult AdminDeleteConfirmed(int id)
        {
            using (Repos repo = new Repos())
            {
                if (!repo.DeleteModule(id))
                {
                    //ERROR DELETING MODULE
                }
            }
            return RedirectToAction("Index", new { controller = "Profile" });
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}