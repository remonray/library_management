﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LibraryManagement.DAL;
using LibraryManagement.Models;
using LibraryManagement.Models.ViewModels;

namespace LibraryManagement.Controllers
{
    public class BookController : ApplicationBaseController
    {
        private LibraryContext db = new LibraryContext();

        // GET: /Book/
        public ActionResult Index()
        {
            var books = db.Books.Include(b => b.Publisher);
            return View(books.ToList());
        }

        // GET: /Book/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            return View(book);
        }

        // GET: /Book/Create
        public ActionResult Create()
        {
            var bookViewModel = new BookViewModel();

            var allAuthorsList = db.Authors.ToList();
            ViewBag.AllAuthors = allAuthorsList.Select(o => new SelectListItem
            {
                Text = o.first_name + " " + o.last_name,
                Value = o.authorID.ToString()
            });
            var allBookCategoriesList = db.BookCategories.ToList();
            ViewBag.AllBookCategories = allBookCategoriesList.Select(o => new SelectListItem
            {
                Text = o.category_name,
                Value = o.bookCategoryID.ToString()
            });
            ViewBag.publisherID = new SelectList(db.Publishers, "publisherID", "name");
            return View();
        }

        // POST: /Book/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="bookID,title,published_date,standard_charge,penalty_charge,publisherID")] Book book, BookViewModel bookView)
        {

            if (ModelState.IsValid)
            {
                var bookToAdd = db.Books
                    .Include(i => i.Authors)
                    .Include(i => i.BookCategories)
                    .First();

                if (TryUpdateModel(bookToAdd, "book", new string[] { "bookID", "title", "published_date", "standard_charge", "penalty_charge", "publisherID" }))
                {
                    var updatedAuthors = new HashSet<int>(bookView.SelectedAuthors);
                    foreach (Author author in db.Authors)
                    {
                        if (!updatedAuthors.Contains(author.authorID))
                        {
                            bookToAdd.Authors.Remove(author);
                        }
                        else
                        {
                            bookToAdd.Authors.Add(author);
                        }
                    }

                    var updatedBookCategories = new HashSet<int>(bookView.SelectedBookCategories);
                    foreach (BookCategory bookCategory in db.BookCategories)
                    {
                        if (!updatedBookCategories.Contains(bookCategory.bookCategoryID))
                        {
                            bookToAdd.BookCategories.Remove(bookCategory);
                        }
                        else
                        {
                            bookToAdd.BookCategories.Add(bookCategory);
                        }
                    }
                }
                db.Books.Add(bookToAdd);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.publishedID = new SelectList(db.Publishers, "publisherID", "name", bookView.Book.publisherID);
            return View(bookView);
        }

        // GET: /Book/Edit/5
        public ActionResult Edit(int? id)
        {


            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var bookViewModel = new BookViewModel
            {
                Book = db.Books.Include(i => i.Authors).Include(i => i.BookCategories).First(i => i.bookID == id),
            };

            if (bookViewModel.Book == null)
                return HttpNotFound();

            var allAuthorsList = db.Authors.ToList();
            var allBookCategoriesList = db.BookCategories.ToList();
            bookViewModel.AllAuthors = allAuthorsList.Select(o => new SelectListItem
            {
                Text = o.first_name+" "+o.last_name,
                Value = o.authorID.ToString()
            });
            bookViewModel.AllBookCategories = allBookCategoriesList.Select(o => new SelectListItem
            {
                Text = o.category_name,
                Value = o.bookCategoryID.ToString()
            });   
            ViewBag.publisherID = new SelectList(db.Publishers, "publisherID", "name", bookViewModel.Book.publisherID);
            return View(bookViewModel);
        }

        // POST: /Book/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="bookID,title,published_date,standard_charge,penalty_charge,publisherID")] Book book, BookViewModel bookView)
        {
            if (bookView == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            if (ModelState.IsValid)
            {
                var bookToUpdate = db.Books
                    .Include(i => i.Authors).Include(i => i.BookCategories).First(i => i.bookID == bookView.Book.bookID);

                if (TryUpdateModel(bookToUpdate, "Book", new string[] { "title", "published_date", "standard_charge", "penalty_charge", "publisherID"}))
                {
                    // Update Authors
                    var newAuthors = db.Authors.Where(
                        m => bookView.SelectedAuthors.Contains(m.authorID)).ToList();
                    var updatedAuthors = new HashSet<int>(bookView.SelectedAuthors);
                    foreach (Author author in db.Authors)
                    {
                        if (!updatedAuthors.Contains(author.authorID))
                        {
                            bookToUpdate.Authors.Remove(author);
                        }
                        else
                        {
                            bookToUpdate.Authors.Add(author);
                        }
                    }

                    // Update Book Categories
                    var newBookCategories = db.BookCategories.Where(
                        m => bookView.SelectedBookCategories.Contains(m.bookCategoryID)).ToList();
                    var updatedBookCategories = new HashSet<int>(bookView.SelectedBookCategories);
                    foreach (BookCategory bookCategory in db.BookCategories)
                    {
                        if (!updatedBookCategories.Contains(bookCategory.bookCategoryID))
                        {
                            bookToUpdate.BookCategories.Remove(bookCategory);
                        }
                        else
                        {
                            bookToUpdate.BookCategories.Add(bookCategory);
                        }
                    }

                    db.Entry(bookToUpdate).State = EntityState.Modified;
                    db.SaveChanges();
                }

                return RedirectToAction("Index");
            }

              ViewBag.publishedID = new SelectList(db.Publishers, "publisherID", "name", bookView.Book.publisherID);
             return View(bookView);
        }

        // GET: /Book/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            return View(book);
        }

        // POST: /Book/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Book book = db.Books.Find(id);
            db.Books.Remove(book);
            db.SaveChanges();
            return RedirectToAction("Index");
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
