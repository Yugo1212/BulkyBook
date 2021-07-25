using BulkyBookApp.DataAccess.Repository.IRepository;
using BulkyBookApp.Models;
using BulkyBookApp.Models.ViewModels;
using BulkyBookApp.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BulkyBookApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]

    public class OrderController : Controller
    {
        public IUnitOfWork _unitOfWork { get; set; }

        [BindProperty]
        public OrderDetailsVM OrderVM { get; set; }

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int id)
        {
            OrderVM = new OrderDetailsVM()
            {
                OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id, includeProperties: "ApplicationUser"),
                OrderDetails = _unitOfWork.OrderDetails.GetAll(o => o.OrderId == id, includeProperties: "Product")
            };
            return View(OrderVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Details(string stripeToken)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id, includeProperties:"ApplicationUser");
            if(stripeToken != null)
            {
                var options = new ChargeCreateOptions
                {
                    Amount = Convert.ToInt32(orderHeader.OrderTotal * 100),
                    Currency = "usd",
                    Description = "Order ID : " + orderHeader.Id,
                    Source = stripeToken
                };

                var service = new ChargeService();
                Charge charge = service.Create(options);

                if (charge.Id == null)
                {
                    orderHeader.PaymentStatus = SD.PaymentStatusRejected;
                }
                else
                {
                    orderHeader.TransactionId = charge.Id;
                }
                if (charge.Status.ToLower() == "succeeded")
                {
                    orderHeader.PaymentStatus = SD.PaymentStatusApproved;
                    orderHeader.PaymentDate = DateTime.Now;
                }
                _unitOfWork.Save();
            }
            return RedirectToAction("Details", "Order", new { id = orderHeader.Id });
        }

        [Authorize(Roles = SD.Role_User_Admin+","+SD.Role_User_Employee)]
        public IActionResult StartPocessing(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(o => o.Id == id);
            orderHeader.OrderStatus = SD.StatusInProcess;
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_User_Admin + "," + SD.Role_User_Employee)]
        public IActionResult ShipOrder()
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(o => o.Id == OrderVM.OrderHeader.Id);
            orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            orderHeader.ShippingDate = DateTime.Now;
            orderHeader.OrderStatus = SD.StatusShipped;
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = SD.Role_User_Admin + "," + SD.Role_User_Employee)]
        public IActionResult CancelOrder(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(o => o.Id == id);
            if(orderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Amount = Convert.ToInt32(orderHeader.OrderTotal * 100),
                    Reason = RefundReasons.RequestedByCustomer,
                    Charge = orderHeader.TransactionId
                };
                var service = new RefundService();
                Refund refund = service.Create(options);

                orderHeader.OrderStatus = SD.StatusRefunded;
                orderHeader.PaymentStatus = SD.StatusRefunded;
            }
            else
            {
                orderHeader.OrderStatus = SD.StatusCanceled;
                orderHeader.OrderStatus = SD.StatusCanceled;
            }
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetOrderList(string status)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            IEnumerable<OrderHeader> orderHeaderList;

            if(User.IsInRole(SD.Role_User_Admin) || User.IsInRole(SD.Role_User_Employee))
            {
                orderHeaderList = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");

            }
            else
            {
                orderHeaderList = _unitOfWork.OrderHeader.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "ApplicationUser");
            }

            switch (status)
            {
                case "pending":
                    orderHeaderList = orderHeaderList.Where(o => o.PaymentStatus == SD.PaymentStatusPending);
                    break;
                case "completed":
                    orderHeaderList = orderHeaderList.Where(o => o.OrderStatus == SD.StatusShipped);
                    break;
                case "rejected":
                    orderHeaderList = orderHeaderList.Where(o => o.PaymentStatus == SD.PaymentStatusRejected ||
                                                            o.OrderStatus == SD.StatusCanceled ||
                                                            o.OrderStatus == SD.StatusRefunded);
                    break;
                case "inprocess":
                    orderHeaderList = orderHeaderList.Where(o => o.OrderStatus == SD.StatusApproved || 
                                                            o.OrderStatus == SD.StatusInProcess || 
                                                            o.OrderStatus == SD.StatusPending);
                    break;
                default:
                    break;

            }

            return Json(new { data = orderHeaderList });
        }
        #endregion
    }
}
