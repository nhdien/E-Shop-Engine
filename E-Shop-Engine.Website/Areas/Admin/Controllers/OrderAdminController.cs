﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using AutoMapper;
using E_Shop_Engine.Domain.DomainModel;
using E_Shop_Engine.Domain.Interfaces;
using E_Shop_Engine.Website.Areas.Admin.Models;
using E_Shop_Engine.Website.Controllers;
using E_Shop_Engine.Website.CustomFilters;
using E_Shop_Engine.Website.Extensions;
using NLog;
using X.PagedList;

namespace E_Shop_Engine.Website.Areas.Admin.Controllers
{
    [RouteArea("Admin", AreaPrefix = "Admin")]
    [RoutePrefix("Order")]
    [Route("{action}")]
    [ReturnUrl]
    public class OrderAdminController : BaseController
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMailingRepository _mailingRepository;

        public OrderAdminController(IOrderRepository orderRepository, IMailingRepository mailingRepository)
        {
            _orderRepository = orderRepository;
            _mailingRepository = mailingRepository;
            logger = LogManager.GetCurrentClassLogger();
        }

        // GET: Admin/Order
        public ActionResult Index(int? page, string sortOrder, bool descending = true)
        {
            ReverseSorting(ref descending, sortOrder);
            IQueryable<Order> model = _orderRepository.GetAll();
            IEnumerable<OrderAdminViewModel> mappedModel = PagedListHelper.SortBy<Order, OrderAdminViewModel>(model, "Created", sortOrder, descending);

            int pageNumber = page ?? 1;
            IPagedList<OrderAdminViewModel> viewModel = mappedModel.ToPagedList(pageNumber, 25);

            SaveSortingState(sortOrder, descending);

            return View(viewModel);
        }

        public ActionResult Details(int id)
        {
            Order order = _orderRepository.GetById(id);
            OrderAdminViewModel model = Mapper.Map<OrderAdminViewModel>(order);

            return View(model);
        }

        public ViewResult Edit(int id)
        {
            Order order = _orderRepository.GetById(id);
            OrderAdminViewModel model = Mapper.Map<OrderAdminViewModel>(order);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(OrderAdminViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Order order = _orderRepository.GetById(model.Id);
            order.Finished = model.Finished;
            order.OrderStatus = model.OrderStatus;

            _orderRepository.Update(Mapper.Map<Order>(order));
            _mailingRepository.OrderChangedStatusMail(order.AppUser.Email, order.OrderNumber, order.OrderStatus.ToString(), "Order " + order.OrderNumber + " status updated");

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Search(string text, int? page)
        {
            if (!string.IsNullOrEmpty(text))
            {
                TempData["search"] = text;
            }
            else if (TempData.ContainsKey("search"))
            {
                text = TempData["search"].ToString();
                TempData.Keep("search");
            }

            int pageNumber = page ?? 1;
            Expression<Func<Order, string>> sortCondition = x => x.OrderNumber;

            IEnumerable<Order> resultOrderNumber = _orderRepository.FindByOrderNumber(text);
            IEnumerable<Order> resultTransactionNumber = _orderRepository.FindByTransactionNumber(text);
            IEnumerable<Order> model = resultOrderNumber.Union(resultTransactionNumber);

            IPagedList<OrderAdminViewModel> viewModel = PagedListHelper.IQueryableToPagedList<Order, OrderAdminViewModel, string>(model.AsQueryable(), sortCondition, pageNumber, 3, true);

            return View("Index", viewModel);
        }
    }
}