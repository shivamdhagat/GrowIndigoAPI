﻿using GrowIndigo.Common;
using GrowIndigo.Data;
using GrowIndigo.Models;
using Newtonsoft.Json;
using Razorpay.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using static GrowIndigo.Models.Mandi_OrderViewModel;

namespace GrowIndigo.Controllers
{
    //[Authorize]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class Mandi_OrderController : ApiController
    {
        #region Dependencies

        GrowIndigoAPIDBEntities dbContext = new GrowIndigoAPIDBEntities();
        Authentication auth = new Authentication();
        CommonClasses objCommonClasses = new CommonClasses();

        SuccessResponse objResponse = new SuccessResponse();
        OrderResponse objOrderResponse = new OrderResponse();

        #endregion


         #region Order History


        #region to call razor pay API


        [Route("api/Mandi_Order/GetRazorPayOrderID")]
        public HttpResponseMessage GetRazorPayOrderID(string amount = "")
        {
            try
            {
                double stringAmount = Convert.ToDouble(amount);
                //string stringReceipt = receipt.ToString();
                Dictionary<string, object> input = new Dictionary<string, object>();
                input.Add("amount", stringAmount); // this amount should be same as transaction amount
                input.Add("currency", "INR");
                input.Add("receipt", "Receipt");
                input.Add("payment_capture", 1);

                string key = "rzp_test_ylJutnAV7FfgPR";
                string secret = "EJ39qrYrFu6J0ENIPSogx1d6";

                RazorpayClient client = new RazorpayClient(key, secret);

                Razorpay.Api.Order order = client.Order.Create(input);

                var orderId = order["id"].ToString();

                objOrderResponse.RazorPayOrderId = orderId;
                return Request.CreateResponse(HttpStatusCode.OK, objOrderResponse);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }


        #endregion

        /// <summary>
        /// To insertOrder Details.
        /// </summary>
        /// <param name="objOrderDetailsViewModel"></param>
        /// <returns></returns>
        [HttpPost]
        // [Authorize]
        [Route("api/Mandi_Order/AddOrderDetails")]
        public HttpResponseMessage AddOrderDetails(OrderBookingViewModel objOrderBookingViewModel)
        {

            try
            {
                Mandi_OrderDetails objMandi_OrderDetails = new Mandi_OrderDetails();
                MandiUserController objMandiUserController = new MandiUserController();
                string mobileNumber = objOrderBookingViewModel.Buyer_Mobile;
                //get mobileNumber from user table
                var number = (from user in dbContext.Mandi_UserInfo where user.MobileNumber == mobileNumber select user).FirstOrDefault();
                if (number != null && objOrderBookingViewModel.Payment_Mode == "Pay on delivery")
                {
                    objMandi_OrderDetails.Buyer_Mobile = objOrderBookingViewModel.Buyer_Mobile;
                    objMandi_OrderDetails.TotalPrice = objOrderBookingViewModel.Totalprice;
                    objMandi_OrderDetails.Payment_Mode = objOrderBookingViewModel.Payment_Mode;
                    objMandi_OrderDetails.OrderDate = DateTime.Now;
                    objMandi_OrderDetails.TransactionStatus = "Submitted";
                    objMandi_OrderDetails.Shipping_Address_Id = objOrderBookingViewModel.Shipping_Address_Id;
                    objMandi_OrderDetails.Order_Status = objOrderBookingViewModel.Order_Status;
                    objMandi_OrderDetails.GeoAddress = objOrderBookingViewModel.GeoCoordinates;
                    objMandi_OrderDetails.SelectedTotalQty = objOrderBookingViewModel.SelectedTotalQty;
                    //objMandi_OrderDetails.TransactionStatus = objOrderBookingViewModel.TransactionStatus;
                    objMandi_OrderDetails.Buyer_Name = objOrderBookingViewModel.Buyer_Name;
                    objMandi_OrderDetails.ServiceTax = objOrderBookingViewModel.ServiceTax;
                    objMandi_OrderDetails.TotalAmount = objOrderBookingViewModel.TotalAmount;

                    dbContext.Mandi_OrderDetails.Add(objMandi_OrderDetails);
                    var i = dbContext.SaveChanges();
                    int OrderId = objMandi_OrderDetails.Order_Id; // Get OrderId After Save Changes
                    if (i != 0)
                    {
                        objOrderBookingViewModel.Order_Id = OrderId;
                        AddOrderProductDetail(objOrderBookingViewModel);

                        if (objOrderBookingViewModel.Order_Status == "Submitted")
                        {
                            #region new notification functionality

                            var getcsvfile = objOrderBookingViewModel.csvfile.Table1;




                            //ntofication and email
                            foreach (var j in getcsvfile)
                            {
                                objMandiUserController.SendFCMNotificationToUsers(number.DeviceToken, "Congratulations! Your deal of "+ j.CropName+" with Deal ID "+ OrderId + " has been booked successfully. It will be delivered to you soon. For more details, please call our help-center +91 91689 43563.", "Grow Mandi Order Detail");
                                AddNotification(objOrderBookingViewModel, "Congratulations! Your deal of " + j.CropName + " with Deal ID " + OrderId + " has been booked successfully. It will be delivered to you soon. For more details, please call our help-center +91 91689 43563.");
                            

                                var getSellerMobileNumber = (from product in dbContext.Mandi_ProductMaster where product.Tr_Id == j.Product_Id select product.MobileNumber).FirstOrDefault();
                                if (getSellerMobileNumber != null)
                                {
                                    var getSellerTokenNumbner = (from mobile in dbContext.Mandi_UserInfo where mobile.MobileNumber == getSellerMobileNumber select mobile.DeviceToken).FirstOrDefault();
                                    //send notification to seller now 




                                    objMandiUserController.SendFCMNotificationToUsers(getSellerTokenNumbner, "Congratulations! Your deal of "+ j.CropName +" with Deal ID "+ OrderId + " has been booked successfully. It will be delivered to you soon. For more details, please call our help-center +91 91689 43563.", "Grow Mandi Order Detail");
                                    AddNotification(objOrderBookingViewModel, "Congratulations! Your deal of " + j.CropName + " with Deal ID " + OrderId + " has been booked successfully. It will be delivered to you soon. For more details, please call our help-center +91 91689 43563.");

                                    #region Email to admin
                                    //FOR SENDING MAIL TO ADMIN
                                    EmailController objEmailController = new EmailController();
                                    EmailModel objEmailModel = new EmailModel();

                                    //objEmailModel.SellerName = objOrderBookingViewModel.SellerName;
                                    //objEmailModel.SellerContact = objOrderBookingViewModel.SellerContact;
                                    objEmailModel.orderId = OrderId;
                                    objEmailModel.ProductId = j.Product_Id.ToString();
                                    objEmailModel.CropName = j.CropName;
                                    objEmailModel.Qty = j.TotalQuantity;
                                    // objEmailModel.SellerName = (from mobile in dbContext.Mandi_UserInfo where mobile.MobileNumber == getSellerMobileNumber select mobile.DeviceToken).ToString().FirstOrDefault()); ;
                                    objEmailModel.BuyerName = j.BuyerName;
                                    objEmailModel.BuyerContact = j.BuyerContact;
                                    objEmailModel.Price = j.Price.ToString();
                                    objEmailModel.ServiceTax = j.ServiceTax;
                                    objEmailModel.TotalAmount = j.TotalAmount;

                                    objEmailModel.PaymentStatus = j.PaymentStatus;





                                    objEmailController.sendEmailViaWebApi(objEmailModel, "OrderDetail");

                                    #endregion

                                }
                            }





                            #endregion
                        }
                        objOrderResponse.DAP = objOrderBookingViewModel.TotalAmount;
                        objOrderResponse.OrderId = OrderId.ToString();
                     
                        return Request.CreateResponse(HttpStatusCode.OK, objOrderResponse);
                    }
                    else
                    {
                        objResponse.Message = "Failed";
                        objMandiUserController.SendFCMNotificationToUsers(number.DeviceToken, "Your order for amount ₹" + objOrderBookingViewModel.Totalprice + " has been Failed against Order Id " + objOrderBookingViewModel.Order_Id, "Grow Mandi");
                        AddNotification(objOrderBookingViewModel, "Your order for amount ₹" + objOrderBookingViewModel.Totalprice + " has been Failed against Order Id " + objOrderBookingViewModel.Order_Id);
                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }

                }

                else if (number != null && objOrderBookingViewModel.Payment_Mode == "UPI")
                {
                    objMandi_OrderDetails.Buyer_Mobile = objOrderBookingViewModel.Buyer_Mobile;
                    objMandi_OrderDetails.TotalPrice = objOrderBookingViewModel.Totalprice;
                    objMandi_OrderDetails.Payment_Mode = objOrderBookingViewModel.Payment_Mode;
                    objMandi_OrderDetails.OrderDate = DateTime.Now;
                    objMandi_OrderDetails.Shipping_Address_Id = objOrderBookingViewModel.Shipping_Address_Id;
                    objMandi_OrderDetails.Order_Status = objOrderBookingViewModel.Order_Status;
                    objMandi_OrderDetails.GeoAddress = objOrderBookingViewModel.GeoCoordinates;
                    objMandi_OrderDetails.SelectedTotalQty = objOrderBookingViewModel.SelectedTotalQty;
                    objMandi_OrderDetails.Buyer_Name = objOrderBookingViewModel.Buyer_Name;



                    dbContext.Mandi_OrderDetails.Add(objMandi_OrderDetails);
                    var i = dbContext.SaveChanges();
                    int OrderId = objMandi_OrderDetails.Order_Id; // Get OrderId After Save Changes
                    if (i != 0)
                    {
                        objOrderBookingViewModel.Order_Id = OrderId;
                        AddOrderProductDetail(objOrderBookingViewModel);

                        if (objOrderBookingViewModel.Order_Status == "Submitted")
                        {
                            objMandiUserController.SendFCMNotificationToUsers(number.DeviceToken, "Congratulations! Your deal of " + objOrderBookingViewModel.CropName + " with Deal ID " + OrderId + " has been booked successfully. It will be delivered to you soon. For more details, please call our help-center +91 91689 43563.", "Grow Mandi");
                            AddNotification(objOrderBookingViewModel, "Congratulations! Your deal of " + objOrderBookingViewModel.CropName + " with Deal ID " + OrderId + " has been booked successfully. It will be delivered to you soon. For more details, please call our help-center +91 91689 43563.");
                        }

                        objResponse.Message = "Success";
                        objOrderResponse.DAP = "";
                        objOrderResponse.UPI = objOrderBookingViewModel.Order_Id;
                        return Request.CreateResponse(HttpStatusCode.OK, objOrderResponse);
                    }
                    else
                    {
                        objResponse.Message = "Failed";
                        objMandiUserController.SendFCMNotificationToUsers(number.DeviceToken, "Your order for amount ₹" + objOrderBookingViewModel.Totalprice + " has been Failed against Order Id " + objOrderBookingViewModel.Order_Id, "Test");
                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }

                }
                else if (number != null && objOrderBookingViewModel.Payment_Mode == "RazorPay")
                {
                    objMandi_OrderDetails.Buyer_Mobile = objOrderBookingViewModel.Buyer_Mobile;
                    objMandi_OrderDetails.TotalPrice = objOrderBookingViewModel.Totalprice;
                    objMandi_OrderDetails.Payment_Mode = objOrderBookingViewModel.Payment_Mode;
                    objMandi_OrderDetails.OrderDate = DateTime.Now;
                    objMandi_OrderDetails.ShippingAddress = objOrderBookingViewModel.ShippingAddress;
                    objMandi_OrderDetails.Shipping_Address_Id = objOrderBookingViewModel.Shipping_Address_Id;
                    objMandi_OrderDetails.Order_Status = objOrderBookingViewModel.Order_Status;
                    objMandi_OrderDetails.GeoAddress = objOrderBookingViewModel.GeoCoordinates;
                    objMandi_OrderDetails.SelectedTotalQty = objOrderBookingViewModel.SelectedTotalQty;
                    objMandi_OrderDetails.Buyer_Name = objOrderBookingViewModel.Buyer_Name;



                    dbContext.Mandi_OrderDetails.Add(objMandi_OrderDetails);
                    var i = dbContext.SaveChanges();
                    int OrderId = objMandi_OrderDetails.Order_Id; // Get OrderId After Save Changes
                    if (i != 0)
                    {
                        objOrderBookingViewModel.Order_Id = OrderId;
                        AddOrderProductDetail(objOrderBookingViewModel);

                        if (objOrderBookingViewModel.Order_Status == "Submitted")
                        {
                            var getcsvfile = objOrderBookingViewModel.csvfile.Table1;




                            //ntofication and email
                            foreach (var j in getcsvfile)
                            {
                                objMandiUserController.SendFCMNotificationToUsers(number.DeviceToken, "Congratulations! Your deal of " + j.CropName + " with Deal ID " + OrderId + " has been booked successfully. It will be delivered to you soon. For more details, please call our help-center +91 91689 43563.", "Grow Mandi");
                                AddNotification(objOrderBookingViewModel, "Congratulations! Your deal of " + j.CropName + " with Deal ID " + OrderId + " has been booked successfully. It will be delivered to you soon. For more details, please call our help-center +91 91689 43563.");
                            }
                        }
                        objResponse.Message = "Success";
                        objOrderResponse.DAP = "";
                        objOrderResponse.UPI = objOrderBookingViewModel.Order_Id;
                        return Request.CreateResponse(HttpStatusCode.OK, objOrderResponse);
                    }
                    else
                    {
                        objResponse.Message = "Failed";
                        objMandiUserController.SendFCMNotificationToUsers(number.DeviceToken, "Your order for amount ₹" + objOrderBookingViewModel.Totalprice + " has been Failed against Order Id " + objOrderBookingViewModel.Order_Id, "Test");
                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }

                }
                else
                {
                    objResponse.Message = "Mobile number not exists.";

                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);

                }

            }
            catch (Exception ex)

            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);

                OrderErrorLogDetails objOrderErrorLogDetails = new OrderErrorLogDetails();
                objOrderErrorLogDetails.Retailer_Mobile = objOrderBookingViewModel.Buyer_Mobile;
                objOrderErrorLogDetails.PaymentMode = objOrderBookingViewModel.Payment_Mode;
                objOrderErrorLogDetails.Total_Price = objOrderBookingViewModel.Totalprice;
                objOrderErrorLogDetails.Qty = objOrderBookingViewModel.Qty;
                //objOrderErrorLogDetails.Csv_File =;
                objOrderErrorLogDetails.Error_Msg = ex.Message;
                objOrderErrorLogDetails.Tr_Date = DateTime.Now;

                dbContext.OrderErrorLogDetails.Add(objOrderErrorLogDetails);
                var i = dbContext.SaveChanges();

                if (i != 0)
                {
                    objResponse.Message = "Exception Logged Successfully";
                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                }
                else
                {
                    objResponse.Message = "Failed";
                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                }

                //objCommonClasses.InsertExceptionDetails(ex, "UserController", "SubmitFeedback");
                //return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);

            }


        }


        public string AddOrderProductDetail(OrderBookingViewModel objOrderBookingViewModel)
        {
            var i = 0;

            try
            {
                //get mobileNumber from user table
                string mobileNumber = objOrderBookingViewModel.Buyer_Mobile;
                var number = (from user in dbContext.Mandi_UserInfo where user.MobileNumber == mobileNumber select user).FirstOrDefault();
                if (number != null)
                {
                    Mandi_OrderProductDetails objMandi_OrderProductDetails = new Mandi_OrderProductDetails();
                    var getcsvfile = objOrderBookingViewModel.csvfile.Table1;

                    foreach (var j in getcsvfile)
                    {
                        objMandi_OrderProductDetails.Order_Id = objOrderBookingViewModel.Order_Id;
                        objMandi_OrderProductDetails.Product_Id = objOrderBookingViewModel.Product_Id;
                        objMandi_OrderProductDetails.TotalQuantity = objOrderBookingViewModel.SelectedTotalQty;
                        objMandi_OrderProductDetails.Price = Convert.ToInt32(j.Price);
                        objMandi_OrderProductDetails.Tr_Date = DateTime.Now;
                        objMandi_OrderProductDetails.SelectedQuantity = objOrderBookingViewModel.SelectedTotalQty;
                        objMandi_OrderProductDetails.SelectedProductPrice = objOrderBookingViewModel.Totalprice;

                        dbContext.Mandi_OrderProductDetails.Add(objMandi_OrderProductDetails);
                        i = dbContext.SaveChanges();


                    }

                    if (i != 0)
                    {
                        if (objOrderBookingViewModel.Payment_Mode == "Pay on delivery")
                        {
                            objOrderResponse.DAP = objOrderBookingViewModel.TotalAmount;

                            return objOrderResponse.DAP;
                        }
                        else if (objOrderBookingViewModel.Payment_Mode == "RazorPay")
                        {
                            objOrderResponse.OrderId = objOrderBookingViewModel.Order_Id.ToString();
                            objOrderResponse.Message = "Success";
                            return objOrderResponse.ToString();
                        }
                        else
                        {
                            objOrderResponse.UPI = objOrderBookingViewModel.Order_Id;
                            return objOrderResponse.DAP;
                        }
                        //objResponse.Message = objOrderBookingViewModel.Totalprice;
                        //return objResponse.Message;
                    }
                    else
                    {
                        objResponse.Message = "Failed";
                        return objResponse.Message;
                    }
                }
                else
                {
                    objResponse.Message = "Mobile number not exists.";

                    return objResponse.Message;

                }

            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                OrderErrorLogDetails objOrderErrorLogDetails = new OrderErrorLogDetails();
                objOrderErrorLogDetails.Retailer_Mobile = objOrderBookingViewModel.Buyer_Mobile;
                objOrderErrorLogDetails.PaymentMode = objOrderBookingViewModel.Payment_Mode;
                objOrderErrorLogDetails.Total_Price = objOrderBookingViewModel.Totalprice;
                objOrderErrorLogDetails.Qty = objOrderBookingViewModel.Qty;
                objOrderErrorLogDetails.Csv_File = JsonConvert.SerializeObject(objOrderBookingViewModel.csvfile.Table1);
                objOrderErrorLogDetails.Error_Msg = ex.Message;
                objOrderErrorLogDetails.Tr_Date = DateTime.Now;

                dbContext.OrderErrorLogDetails.Add(objOrderErrorLogDetails);
                i = dbContext.SaveChanges();

                if (i != 0)
                {
                    objResponse.Message = "Exception Logged Successfully";
                    return objResponse.Message;
                }
                else
                {
                    objResponse.Message = "Failed to Log Exception";
                    return objResponse.Message;
                }

                //    objCommonClasses.InsertExceptionDetails(ex, "UserController", "AddOrderProductDetail");
                //    return ex.Message;
            }

        }




        /// <summary>
        /// To Update Order Details.
        /// </summary>
        /// <param name="objOrderDetailsViewModel"></param>
        /// <returns></returns>
        [HttpPost]
        //[Authorize]
        [Route("api/Mandi_Order/UpdateOrderDetails")]
        public HttpResponseMessage UpdateOrderDetails(OrderBookingViewModel objOrderBookingViewModel)
        {

            try
            {
                Mandi_OrderDetails objMandi_OrderDetails = new Mandi_OrderDetails();
                int orderId = Convert.ToInt32(objOrderBookingViewModel.Order_Id);
                var deviceToken = (from user in dbContext.Mandi_UserInfo where user.MobileNumber == objOrderBookingViewModel.Buyer_Mobile select user.DeviceToken).FirstOrDefault();

                //get Order from OrderDetails table
                var orderDetails = (from order in dbContext.Mandi_OrderDetails where order.Order_Id == orderId select order).FirstOrDefault();


                if (orderDetails != null)
                {
                    if (orderDetails.Payment_Mode == "UPI" && objOrderBookingViewModel.TransactionStatus == "SUCCESS")
                    {

                        orderDetails.TransactionId = objOrderBookingViewModel.TransactionId;
                        orderDetails.ResponseCode = objOrderBookingViewModel.ResponseCode;
                        orderDetails.TransactionStatus = objOrderBookingViewModel.TransactionStatus;


                        var i = dbContext.SaveChanges();
                        if (i != 0)
                        {

                            MandiUserController objMandiUserController = new MandiUserController();
                        


                            #region new notification functionality


                            objMandiUserController.SendFCMNotificationToUsers(deviceToken, "Congratulations! Your deal of " + objOrderBookingViewModel.CropName + " with Deal ID " + objOrderBookingViewModel.Order_Id + " has been booked successfully. It will be delivered to you soon.For more details, please call our help-center + 91 91689 43563.");
                            AddNotification(objOrderBookingViewModel, "Congratulations! Your deal of " + objOrderBookingViewModel.CropName + " with Deal ID " + objOrderBookingViewModel.Order_Id + " has been booked successfully. It will be delivered to you soon.For more details, please call our help-center + 91 91689 43563.");
                            var getSellerMobileNumber = (from product in dbContext.Mandi_ProductMaster where product.Tr_Id == objOrderBookingViewModel.Product_Id select product.MobileNumber).FirstOrDefault();
                            if (getSellerMobileNumber != null)
                            {
                                var getSellerTokenNumbner = (from mobile in dbContext.Mandi_UserInfo where mobile.MobileNumber == getSellerMobileNumber select mobile.DeviceToken).FirstOrDefault();
                                //send notification to seller now 
                                objMandiUserController.SendFCMNotificationToUsers(getSellerTokenNumbner, "Dear Customer, Your Produce" + objOrderBookingViewModel.CropName + " with" + objOrderBookingViewModel.ProductId + " has been purchased. Order ID is" + objOrderBookingViewModel.Order_Id + " and it will soon be picked up. Please keep the products ready. In case of help, please call on +91 91689 43563.", "Test");
                                AddNotification(objOrderBookingViewModel, "Dear Customer, Your Produce" + objOrderBookingViewModel.CropName + " with" + objOrderBookingViewModel.ProductId + " has been purchased. Order ID is" + objOrderBookingViewModel.Order_Id + " and it will soon be picked up. Please keep the products ready. In case of help, please call on +91 91689 43563>.");
                                objResponse.Message = "Paid Successfully";
                                return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                                #region Email to admin


                                // EmailController objEmailController = new EmailController();
                                //EmailModel objEmailModel = new EmailModel();
                                //objEmailModel.orderId = objOrderBookingViewModel.Order_Id;
                                //objEmailModel.ProductId = objOrderBookingViewModel.ProductId;

                                //  objEmailController.sendEmailViaWebApi();

                                #endregion

                            }

                            #endregion


                            objResponse.Message = "Paid Successfully";
                            return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                        }
                        else
                        {
                            objResponse.Message = "Transaction Failed.";

                            return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                        }

                    }
                    else if (orderDetails.Payment_Mode == "RazorPay" && objOrderBookingViewModel.Order_Status == "success")
                    {

                        orderDetails.TransactionId = objOrderBookingViewModel.TransactionId;
                        orderDetails.ResponseCode = objOrderBookingViewModel.ResponseCode;
                        orderDetails.Order_Status = objOrderBookingViewModel.Order_Status;
                        orderDetails.rzp_order_id = objOrderBookingViewModel.rzp_order_id;
                        orderDetails.rzp_payment_signature = objOrderBookingViewModel.rzp_payment_signature;
                        orderDetails.rzp_payment_status = objOrderBookingViewModel.rzp_payment_status;


                        var i = dbContext.SaveChanges();
                        if (i != 0)
                        {
                            MandiUserController objMandiUserController = new MandiUserController();
                            


                            #region new notification functionality


                            objMandiUserController.SendFCMNotificationToUsers(deviceToken, "Congratulations! Your deal of " + objOrderBookingViewModel.CropName + " with Deal ID " + objOrderBookingViewModel.Order_Id + " has been booked successfully. It will be delivered to you soon.For more details, please call our help-center + 91 91689 43563.");
                            AddNotification(objOrderBookingViewModel, "Congratulations! Your deal of " + objOrderBookingViewModel.CropName + " with Deal ID " + objOrderBookingViewModel.Order_Id + " has been booked successfully. It will be delivered to you soon.For more details, please call our help-center + 91 91689 43563.");
                            var getSellerMobileNumber = (from product in dbContext.Mandi_ProductMaster where product.Tr_Id == objOrderBookingViewModel.Product_Id select product.MobileNumber).FirstOrDefault();
                            if (getSellerMobileNumber != null)
                            {
                                var getSellerTokenNumbner = (from mobile in dbContext.Mandi_UserInfo where mobile.MobileNumber == getSellerMobileNumber select mobile.DeviceToken).FirstOrDefault();
                                //send notification to seller now 
                                objMandiUserController.SendFCMNotificationToUsers(getSellerTokenNumbner, "Dear Customer, Your Produce" + objOrderBookingViewModel.CropName + " with" + objOrderBookingViewModel.ProductId + " has been purchased. Order ID is" + objOrderBookingViewModel.Order_Id + " and it will soon be picked up. Please keep the products ready. In case of help, please call on +91 91689 43563>.", "Test");
                                AddNotification(objOrderBookingViewModel, "Dear Customer, Your Produce" + objOrderBookingViewModel.CropName + " with" + objOrderBookingViewModel.ProductId + " has been purchased. Order ID is" + objOrderBookingViewModel.Order_Id + " and it will soon be picked up. Please keep the products ready. In case of help, please call on +91 91689 43563>.");
                                #region Email to admin


                                // EmailController objEmailController = new EmailController();
                                //EmailModel objEmailModel = new EmailModel();
                                //objEmailModel.orderId = objOrderBookingViewModel.Order_Id;
                                //objEmailModel.ProductId = objOrderBookingViewModel.ProductId;

                                //  objEmailController.sendEmailViaWebApi();

                                #endregion

                            }

                            #endregion


                            objResponse.Message = "Paid Successfully";
                            return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                        }
                        else
                        {
                            objResponse.Message = "Transaction Failed.";

                            return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                        }

                    }
                    else if (objOrderBookingViewModel.TransactionStatus == "FAILURE")
                    {
                        orderDetails.TransactionStatus = objOrderBookingViewModel.TransactionStatus;
                        orderDetails.ResponseCode = objOrderBookingViewModel.ResponseCode;

                        var i = dbContext.SaveChanges();
                        if (i != 0)
                        {
                            objResponse.Message = "Transaction Failed";
                            MandiUserController objMandiUserController = new MandiUserController();
                            objMandiUserController.SendFCMNotificationToUsers(deviceToken, "Transaction Declined for amount ₹" + objOrderBookingViewModel.Totalprice + " against Order Id " + objOrderBookingViewModel.Order_Id, "Grow Mandi");
                            AddNotification(objOrderBookingViewModel, "Transaction Declined for amount ₹" + objOrderBookingViewModel.Totalprice + " against Order Id " + objOrderBookingViewModel.Order_Id);
                            return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                        }
                        else
                        {
                            objResponse.Message = "Error occured While Saving Transaction Status.";

                            return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                        }

                    }
                    else
                    {
                        objResponse.Message = "No Order Details Found.";

                        return Request.CreateResponse(HttpStatusCode.OK, objResponse);
                    }
                }
                else
                {
                    objResponse.Message = "No Order Details Found.";

                    return Request.CreateResponse(HttpStatusCode.OK, objResponse);

                }

            }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "UserController", "UpdateOrderDetails");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);


            }


        }


        /// <summary>
        /// To Get Order Details History.
        /// </summary>
        /// <param name="objOrderHistoryViewModel"></param>
        /// <returns></returns>
        [HttpPost]

        [Route("api/Mandi_Order/GetOrderHistory")]
        public HttpResponseMessage GetOrderHistory(MandiOrderHistoryViewModel objOrderHistoryViewModel)
        {


            try
            {
                MandiOrderList objMandiOrderList = new MandiOrderList();
                List<MandiOrderHistoryViewModel> objListMandiOrderHistory = new List<MandiOrderHistoryViewModel>();
                string mobileNumber = objOrderHistoryViewModel.Buyer_Mobile;

                //get mobileNumber from Mandi_UserInfo table
                //var getUser = (from user in dbContext.Mandi_UserInfo where user.MobileNumber == mobileNumber select user).FirstOrDefault();
                var getOrder = (from order in dbContext.Mandi_OrderDetails where order.Buyer_Mobile == mobileNumber select order).ToList();

                var getOrderList = (from orderList in dbContext.Mandi_OrderDetails
                                    join cart in dbContext.Mandi_CartInfo on orderList.Buyer_Mobile equals cart.BuyerNumber
                                    join product in dbContext.Mandi_OrderProductDetails on orderList.Order_Id equals product.Order_Id
                                    join address in dbContext.UsersAddress on orderList.Shipping_Address_Id equals address.tr_id
                                    join productmaster in dbContext.Mandi_ProductMaster on product.Product_Id equals productmaster.Tr_Id
                                    join crop in dbContext.Crop_Master on productmaster.CropId equals crop.CropId

                                   
                                    join variety in dbContext.Variety_Master on productmaster.VarietyId equals variety.VarietyId


                                    orderby orderList.Order_Id descending

                                    select new
                                    {
                                        orderList.Order_Id,
                                        orderList.Buyer_Mobile,
                                        orderList.TotalPrice,
                                        orderList.Payment_Mode,
                                        orderList.ServiceTax,
                                        orderList.TotalAmount,
                                        orderList.Shipping_Address_Id,
                                        orderList.Order_Status,
                                        orderList.SelectedTotalQty,
                                        orderList.Buyer_Name,
                                        orderList.GeoAddress,
                                        orderList.OrderDate,
                                        address.reciver_name,
                                        address.ship_address,
                                        address.city,
                                        cart.Logistics_Cost,
                                        cart.Other_Charges,
                                        cart.Taxes,
                                        address.pincode,
                                        productmaster.ProductImageUrl,
                                        address.ship_mobile,
                                        product.Product_Id,
                                        product.TotalQuantity,
                                        cart.Price,
                                        productmaster.TermsAndCondition,
                                        productmaster.QuantityUnit,
                                        product.SelectedQuantity,
                                        product.SelectedProductPrice,
                                        crop.CropName,
                                        variety.VarietyName
                                        //productmaster.VarietyId

                                    }).Where(z => z.Buyer_Mobile == mobileNumber).ToList();

                var test = getOrderList;
                var test1 = getOrderList;

                if (getOrderList.Count >= 0)
                {

                    foreach (var i in getOrderList)
                    {
                        var getSellerMobileNumber = (from product in dbContext.Mandi_ProductMaster where product.Tr_Id == i.Product_Id select product).FirstOrDefault();


                        if (getSellerMobileNumber != null)
                        {
                            var getSellerAddress = (from mobile in dbContext.Mandi_UserInfo where mobile.MobileNumber == getSellerMobileNumber.MobileNumber select mobile).FirstOrDefault();
                            //var getCartInfo = (from cart in dbContext.Mandi_CartInfo where cart.Product_Id == i.Product_Id select cart).FirstOrDefault();

                            MandiOrderHistoryViewModel objMandiOrderHistoryList = new MandiOrderHistoryViewModel()
                            {
                                Order_Id = Convert.ToString(i.Order_Id),
                                Buyer_Mobile = i.Buyer_Mobile,
                                TotalPrice = i.TotalPrice,
                                Payment_Mode = i.Payment_Mode,
                                QuantityUnit = i.QuantityUnit,
                                Shipping_Address_Id = Convert.ToString(i.Shipping_Address_Id),
                                Order_Status = i.Order_Status,
                                SelectedTotalQty = i.SelectedTotalQty,
                                Buyer_Name = i.Buyer_Name,
                                GeoCoordinates = i.GeoAddress,
                                ServiceTax = i.ServiceTax == null ? 0 : i.ServiceTax,
                                TotalAmount = i.TotalAmount == null ? "0" : i.TotalAmount,
                                OrderDate = Convert.ToString(i.OrderDate),
                                reciver_name = i.reciver_name,
                                ship_address = i.ship_address,
                                city = i.city,
                                TermsAndCondition = i.TermsAndCondition,
                                Logistics_Cost = i.Logistics_Cost,
                                Other_Charges = i.Other_Charges,
                                Taxes = i.Taxes,
                                SellerName = getSellerAddress.FullName,
                                SellerState = getSellerAddress.State == null ? "N/A" : getSellerAddress.State,
                                SellerDistrict = getSellerAddress.District == null ? "N/A" : getSellerAddress.District,
                                SelletTaluka = getSellerAddress.Taluka == null ? "N/A" : getSellerAddress.Taluka,
                                SellerVillage = getSellerAddress.Village == null ? "N/A" : getSellerAddress.Village,
                                SellerPinCode = getSellerAddress.Pincode == null ? "N/A" : getSellerAddress.Pincode,
                                
                                ProductImageUrl = getSellerMobileNumber.ProductImageUrl,

                                pincode = i.pincode,
                                ship_mobile = i.ship_mobile,
                                Product_Id = Convert.ToInt32(i.Product_Id),
                                TotalQuantity = i.TotalQuantity,
                                Price = Convert.ToInt32(i.Price),
                                SelectedQuantity = i.SelectedQuantity,
                                SelectedProductPrice = i.Price * Convert.ToInt32(i.SelectedQuantity),
                                CropName = i.CropName,
                                VarietyName = i.VarietyName,

                                //Logistics_Cost = getCartInfo == null ? 0 : getCartInfo.Logistics_Cost,
                                //Other_Charges = getCartInfo == null ? 0 : getCartInfo.Other_Charges,
                                //Taxes = getCartInfo == null ? 0 : getCartInfo.Taxes






                                //Qty = i.qty,
                                //ProductName = i.SkuName



                            };

                        objListMandiOrderHistory.Add(objMandiOrderHistoryList);
                    }
                }


                objMandiOrderList.MandiOrders = objListMandiOrderHistory;
                return Request.CreateResponse(HttpStatusCode.OK, objMandiOrderList);
            }
                objResponse.Message = "Order History not found";
            return Request.CreateResponse(HttpStatusCode.OK, objResponse);


        }
            catch (Exception ex)
            {
                Log.Info(Convert.ToString(ex.InnerException));
                Log.Info(ex.Message);
                objCommonClasses.InsertExceptionDetails(ex, "UserController", "GetOrderHistory");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
            }

}



#endregion


#region Notification

public string AddNotification(OrderBookingViewModel objOrderBookingViewModel, string Message)
{


    try
    {
        //get mobileNumber from user table
        string mobileNumber = objOrderBookingViewModel.Buyer_Mobile;
        var number = (from user in dbContext.Mandi_UserInfo where user.MobileNumber == mobileNumber select user).FirstOrDefault();
        if (number != null)
        {
            Mandi_Notification objMandi_Notification = new Mandi_Notification();
            objMandi_Notification.MobileNumber = objOrderBookingViewModel.Buyer_Mobile;
            objMandi_Notification.Message = Message;
            objMandi_Notification.Tr_Date = DateTime.Now;

            dbContext.Mandi_Notification.Add(objMandi_Notification);
            var i = dbContext.SaveChanges();
            if (i != 0)
            {
                objResponse.Message = "Notification Added";
                return objResponse.Message;

            }
            else
            {
                objResponse.Message = "Notification Added";
                return objResponse.Message;

            }


        }
        else
        {
            objResponse.Message = "Mobile number not exists.";
            return objResponse.Message;
        }
    }
    catch (Exception ex)
    {
        Log.Info(Convert.ToString(ex.InnerException));
        Log.Info(ex.Message);
        throw ex;

    }
}

/// <summary>
/// To Get Notification list.
/// </summary>
/// <param name="objNotificationViewModel"></param>
/// <returns></returns>
[HttpPost]
[Authorize]
[Route("api/Mandi_Order/FetchNotification")]
public HttpResponseMessage FetchNotification(NotificationViewModel objNotificationViewModel)
{

    try
    {
        NotificationList objNotificationList = new NotificationList();
        List<NotificationViewModel> objListNotification = new List<NotificationViewModel>();
        string mobileNumber = objNotificationViewModel.MobileNumber;
        var getNotificationList = (from notification in dbContext.Mandi_Notification
                                   orderby notification.Tr_Date descending
                                   select new
                                   {
                                       notification.SerialNumber,
                                       notification.MobileNumber,
                                       notification.Message,
                                       notification.Tr_Date

                                   }).Where(x => x.MobileNumber == mobileNumber).ToList();

        if (getNotificationList != null)
        {
            foreach (var i in getNotificationList)
            {
                NotificationViewModel objNotificationViewModelList = new NotificationViewModel()
                {
                    SerialNumber = i.SerialNumber,
                    MobileNumber = i.MobileNumber,
                    Message = i.Message,
                    Tr_Date = i.Tr_Date

                };
                objListNotification.Add(objNotificationViewModelList);
            }

            objNotificationList.Notifications = objListNotification;
            return Request.CreateResponse(HttpStatusCode.OK, objNotificationList);
        }
        objResponse.Message = "Notification not found";
        return Request.CreateResponse(HttpStatusCode.OK, objResponse);

    }
    catch (Exception ex)
    {
        Log.Info(Convert.ToString(ex.InnerException));
        Log.Info(ex.Message);
        objCommonClasses.InsertExceptionDetails(ex, "UserController", "FetchNotification");
        return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, ex.Message);
    }

}


        #endregion

    }
}
