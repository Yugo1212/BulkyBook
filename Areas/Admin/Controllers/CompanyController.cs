﻿using BulkyBookApp.DataAccess.Repository.IRepository;
using BulkyBookApp.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BulkyBookApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upsert(int? id)
        {
            Company company = new Company();

            if(id == null)
            {
                return View(company);
            }

            company = _unitOfWork.Company.Get(id.GetValueOrDefault());
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Company company)
        {
            if (ModelState.IsValid)
            {
                if (company.Id == 0)
                {
                    _unitOfWork.Company.Add(company);
                }
                else
                {
                    _unitOfWork.Company.Update(company);
                }

                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            return View(company);
        }


        #region API CALLS

        [HttpGet]

        public IActionResult GetALl()
        {
            var allObj = _unitOfWork.Company.GetAll();
            return  Json(new { data = allObj });
        }

        [HttpDelete]

        public IActionResult Delete(int id)
        {
            var objFromDb = _unitOfWork.Company.Get(id);

            if (objFromDb == null)
                return Json(new { success = false, message = "Error while deleting" });
            _unitOfWork.Company.Remove(objFromDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete succesful" });

        }

        #endregion
    }
}
