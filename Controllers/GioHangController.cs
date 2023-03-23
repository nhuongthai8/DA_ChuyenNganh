using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DACN.Models;
using PayPal.Api;

namespace DACN.Controllers
{
    public class GioHangController : Controller
    {
        DataClasses1DataContext data = new DataClasses1DataContext();
        //Lấy giỏ hàng
        public List<ShoppingCart> Laygiohang()
        {
            List<ShoppingCart> listGiohang = Session["ShoppingCart"] as List<ShoppingCart>;
            if (listGiohang == null)
            {
                listGiohang = new List<ShoppingCart>();
                Session["ShoppingCart"] = listGiohang;
            }
            return listGiohang;
        }
        //thêm sản phẩm vào giỏ hàng
        public ActionResult Themgiohang(int iMaSP, string strURL)
        {
            List<ShoppingCart> listGiohang = Laygiohang();
            ShoppingCart sp = listGiohang.Find(n => n.iMaSP == iMaSP);
            if (sp == null)
            {
                sp = new ShoppingCart(iMaSP);
                listGiohang.Add(sp);
                return Redirect(strURL);
            }
            else
            {
                sp.iSoluong++;
                return Redirect(strURL);
            }
        }
        //tổng số lượng
        private int TongSoLuong()
        {
            int iTSL = 0;
            List<ShoppingCart> listGiohang = Session["ShoppingCart"] as List<ShoppingCart>;
            if (listGiohang != null)
            {
                iTSL = listGiohang.Sum(n => n.iSoluong);
            }
            return iTSL;
        }
        //tính tổng tiền
        private double TongTien()
        {
            double iTT = 0;
            List<ShoppingCart> listGiohang = Session["ShoppingCart"] as List<ShoppingCart>;
            if (listGiohang != null)
            {
                iTT = listGiohang.Sum(n => n.dThanhtien);
            }
            return iTT;
        }

        //trang xác nhận thông tin đặt hàng thành công
        public ActionResult ConfirmDH()
        {
            return View();
        }

        //chức năng giỏ hàng
        public ActionResult GioHang()
        {
            List<ShoppingCart> listGiohang = Laygiohang();
            if (listGiohang.Count == 0)
            {
                return RedirectToAction("Sanpham", "MainStore");
            }
            ViewBag.Tongsoluong = TongSoLuong();
            ViewBag.Tongtien = TongTien();
            return View(listGiohang);
        }

        //truyền giỏ hàng lên header
        public ActionResult GioHangPartial()
        {
            ViewBag.Tongsoluong = TongSoLuong();
            ViewBag.Tongtien = TongTien();
            return PartialView();
        }

        //xóa sản phẩm trong giỏ hàng
        public ActionResult XoaGioHang(int iMaSP)
        {
            List<ShoppingCart> listGiohang = Laygiohang();
            ShoppingCart sp = listGiohang.SingleOrDefault(n => n.iMaSP == iMaSP);
            if (sp != null)
            {
                listGiohang.RemoveAll(n => n.iMaSP == iMaSP);
                return RedirectToAction("GioHang");
            }
            if (listGiohang.Count == 0)
            {
                return RedirectToAction("Sanpham", "MainStore");
            }
            return RedirectToAction("GioHang");
        }

        //xóa tất cả sản phẩm trong giỏ hàng
        public ActionResult XoaAll()
        {
            List<ShoppingCart> listGiohang = Laygiohang();
            listGiohang.Clear();
            return RedirectToAction("Sanpham", "MainStore");
        }

        //cập nhật thêm xóa sửa giỏ hàng
        public ActionResult UpdateGioHang(int iMaSP, FormCollection f)
        {
            List<ShoppingCart> listGiohang = Laygiohang();
            ShoppingCart sp = listGiohang.SingleOrDefault(n => n.iMaSP == iMaSP);
            if (sp != null)
            {
                sp.iSoluong = int.Parse(f["txtSoluong"].ToString());
            }
            return RedirectToAction("GioHang");
        }

        [HttpGet]
        public ActionResult DatHang()
        {
            if (Session["TaiKhoan"] == null || Session["TaiKhoan"].ToString() == "")
            {
                return RedirectToAction("Dangnhap", "KhachHang");
            }

            List<ShoppingCart> listGiohang = Laygiohang();
            ViewBag.Tongsoluong = TongSoLuong();
            ViewBag.Tongtien = TongTien();
            return View(listGiohang);
        }
        [HttpPost]
        public ActionResult DatHang(FormCollection collection)
        {
            //Order ddh = new Order();
            var ddh = new DACN.Models.Order();
            KhachHang kh = (KhachHang)Session["TaiKhoan"];
            List<ShoppingCart> listGiohang = Laygiohang();
            ddh.MaKH = kh.MaKH;
            ddh.NgayTao = DateTime.Now;
            var ngaygiao = String.Format("{0:MM/dd/yyyy}", collection["NgayGiao"]);
            ddh.NgayGiao = DateTime.Parse(ngaygiao);
            ddh.TinhTrangTT = false;
            ddh.TinhTrangGH = false;
            data.Orders.InsertOnSubmit(ddh);
            data.SubmitChanges();
            foreach (var item in listGiohang)
            {
                ChiTietOrder ctorder = new ChiTietOrder();
                ctorder.MaOrder = ddh.MaOrder;
                ctorder.idSP = item.iMaSP;
                ctorder.SoLuong = item.iSoluong;
                ctorder.DonGia = (decimal)item.dGiatien;
                data.ChiTietOrders.InsertOnSubmit(ctorder);
            }
            data.SubmitChanges();
            //Session["GioHang"] = null;
            //xóa sản phẩm trong giỏ hàng sau khi đặt hàng xong
            Session.Remove("ShoppingCart");
            return RedirectToAction("ConfirmDH", "GioHang");
        }


        //Paypal
        //work with paypal payment
        private Payment payment;
        //create payment with apicontext
        private Payment CreatePayment(APIContext apiContext, string redirectUrl)
        {
            var listItems = new ItemList() { items = new List<Item>()};
            List<ShoppingCart> listGiohang = (List<ShoppingCart>)Session["ShoppingCart"];
            foreach(var cart in listGiohang)
            {
                listItems.items.Add(new Item()
                {
                    name = cart.sTenSP,
                    currency = "USD",
                    price = cart.dGiatien.ToString(),
                    quantity = cart.iSoluong.ToString(),
                    sku = "sku"
                });
            }
            var payer = new Payer() { payment_method = "paypal" };
            //
            var rdrUrls = new RedirectUrls() 
            {
                cancel_url = redirectUrl,
                return_url = redirectUrl
            };
            //
            var details = new Details()
            {
                tax = "1",
                shipping = "2",
                subtotal = listGiohang.Sum(n => n.dThanhtien).ToString()
            };
            //create amount object
            var amount = new Amount()
            {
                currency = "USD",
                total = (Convert.ToDouble(details.tax) + Convert.ToDouble(details.shipping) + Convert.ToDouble(details.subtotal)).ToString(),
                details = details
            };
            //tao giao dich
            var transactionList = new List<Transaction>();
            transactionList.Add(new Transaction()
            {
                description = "Transaction testing",
                invoice_number = Convert.ToString((new Random()).Next(100000)),
                amount = amount,
                item_list = listItems
            });
            //
            payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = rdrUrls
            };
            return payment.Create(apiContext);
        }
        
        //create execute payment method
        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution()
            {
                payer_id = payerId,
            };
            payment = new Payment() { id = paymentId };
            return payment.Execute(apiContext, paymentExecution);
        }

        //create paymentwithpaypal method
        public ActionResult PaymentWithPaypal()
        {
            //gettings context from the paypal base on clientid and ckientsecret for payment
            APIContext apiContext = PaypalConfigurations.GetAPIContext();
            try
            {
                string payerId = Request.Params["PayerID"];
                if (string.IsNullOrEmpty(payerId))
                {
                    //creating payment
                    string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/GioHang/PaymentWithPaypal?";
                    var guid = Convert.ToString((new Random()).Next(100000));
                    var createdPayment = CreatePayment(apiContext, baseURI + "guid=" + guid);
                    //get links returned from paypal respone to create call function
                    var links = createdPayment.links.GetEnumerator();
                    string paypalRedirectUrl = string.Empty;
                    while (links.MoveNext())
                    {
                        Links link = links.Current;
                        if (link.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            paypalRedirectUrl = link.href;
                        }
                    }
                    Session.Add(guid,createdPayment.id);
                    return Redirect(paypalRedirectUrl);
                }
                else
                {
                    //executed when we hve recieved all the payment params from previous call
                    var guid = Request.Params["guid"];
                    var executePayment = ExecutePayment(apiContext, payerId, Session[guid] as string);
                    if(executePayment.state.ToLower() != "approved")
                    {
                        //xóa sản phẩm trong giỏ hàng sau khi đặt hàng xong
                        Session.Remove("ShoppingCart");
                        return View("FailureDH");
                    }
                }
            }
            catch (Exception ex)
            {
                PaypalLogger.Log("Error: " + ex.Message);
                //xóa sản phẩm trong giỏ hàng sau khi đặt hàng xong
                Session.Remove("ShoppingCart");
                return View("FailureDH");
            }
            //xóa sản phẩm trong giỏ hàng sau khi đặt hàng xong
            Session.Remove("ShoppingCart");
            return View("ConfirmDH");
        }

        public ActionResult FailureDH()
        {
            return View();
        }


        //VNPAY
        public ActionResult PaymentWithVNPAY()
        {
            string url = ConfigurationManager.AppSettings["Url"];
            string returnUrl = ConfigurationManager.AppSettings["ReturnUrl"];
            string tmnCode = ConfigurationManager.AppSettings["TmnCode"];
            string hashSecret = ConfigurationManager.AppSettings["HashSecret"];

            PayLib pay = new PayLib();

            pay.AddRequestData("vnp_Version", "2.1.0"); //Phiên bản api mà merchant kết nối. Phiên bản hiện tại là 2.1.0
            pay.AddRequestData("vnp_Command", "pay"); //Mã API sử dụng, mã cho giao dịch thanh toán là 'pay'
            pay.AddRequestData("vnp_TmnCode", tmnCode); //Mã website của merchant trên hệ thống của VNPAY (khi đăng ký tài khoản sẽ có trong mail VNPAY gửi về)
            pay.AddRequestData("vnp_Amount", "1000000"); //số tiền cần thanh toán, công thức: số tiền * 100 - ví dụ 10.000 (mười nghìn đồng) --> 1000000
            pay.AddRequestData("vnp_BankCode", ""); //Mã Ngân hàng thanh toán (tham khảo: https://sandbox.vnpayment.vn/apis/danh-sach-ngan-hang/), có thể để trống, người dùng có thể chọn trên cổng thanh toán VNPAY
            pay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss")); //ngày thanh toán theo định dạng yyyyMMddHHmmss
            pay.AddRequestData("vnp_CurrCode", "VND"); //Đơn vị tiền tệ sử dụng thanh toán. Hiện tại chỉ hỗ trợ VND
            pay.AddRequestData("vnp_IpAddr", Util.GetIpAddress()); //Địa chỉ IP của khách hàng thực hiện giao dịch
            pay.AddRequestData("vnp_Locale", "vn"); //Ngôn ngữ giao diện hiển thị - Tiếng Việt (vn), Tiếng Anh (en)
            pay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang"); //Thông tin mô tả nội dung thanh toán
            pay.AddRequestData("vnp_OrderType", "other"); //topup: Nạp tiền điện thoại - billpayment: Thanh toán hóa đơn - fashion: Thời trang - other: Thanh toán trực tuyến
            pay.AddRequestData("vnp_ReturnUrl", returnUrl); //URL thông báo kết quả giao dịch khi Khách hàng kết thúc thanh toán
            pay.AddRequestData("vnp_TxnRef", DateTime.Now.Ticks.ToString()); //mã hóa đơn

            string paymentUrl = pay.CreateRequestUrl(url, hashSecret);

            return Redirect(paymentUrl);
        }

        public ActionResult ConfirmDHVnPay()
        {
            if (Request.QueryString.Count > 0)
            {
                string hashSecret = ConfigurationManager.AppSettings["HashSecret"]; //Chuỗi bí mật
                var vnpayData = Request.QueryString;
                PayLib pay = new PayLib();

                //lấy toàn bộ dữ liệu được trả về
                foreach (string s in vnpayData)
                {
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                    {
                        pay.AddResponseData(s, vnpayData[s]);
                    }
                }

                long orderId = Convert.ToInt64(pay.GetResponseData("vnp_TxnRef")); //mã hóa đơn
                long vnpayTranId = Convert.ToInt64(pay.GetResponseData("vnp_TransactionNo")); //mã giao dịch tại hệ thống VNPAY
                string vnp_ResponseCode = pay.GetResponseData("vnp_ResponseCode"); //response code: 00 - thành công, khác 00 - xem thêm https://sandbox.vnpayment.vn/apis/docs/bang-ma-loi/
                string vnp_SecureHash = Request.QueryString["vnp_SecureHash"]; //hash của dữ liệu trả về

                bool checkSignature = pay.ValidateSignature(vnp_SecureHash, hashSecret); //check chữ ký đúng hay không?

                if (checkSignature)
                {
                    if (vnp_ResponseCode == "00")
                    {
                        //Thanh toán thành công
                        ViewBag.Message = "Thanh toán thành công hóa đơn " + orderId + " | Mã giao dịch: " + vnpayTranId;
                    }
                    else
                    {
                        //Thanh toán không thành công. Mã lỗi: vnp_ResponseCode
                        ViewBag.Message = "Có lỗi xảy ra trong quá trình xử lý hóa đơn " + orderId + " | Mã giao dịch: " + vnpayTranId + " | Mã lỗi: " + vnp_ResponseCode;
                    }
                }
                else
                {
                    ViewBag.Message = "Có lỗi xảy ra trong quá trình xử lý";
                }
            }
            Session.Remove("ShoppingCart");
            return View();
        }
    }
}