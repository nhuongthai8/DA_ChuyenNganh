using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DACN.Models;
using Newtonsoft.Json;
using PagedList;

namespace DACN.Controllers
{
    public class StoreController : Controller
    {
        DataClasses1DataContext data = new DataClasses1DataContext();
        // GET: Store

        

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult NewsLetter()
        {
            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult NewsLetter(FormCollection collection)
        {
            NewsLetter nl = new NewsLetter();
            var mail = collection["Mail"];
            if (String.IsNullOrEmpty(mail))
            {
                ViewData["Loi1"] = "Vui lòng nhập eMail!";
            }
            else
            {
                nl.Mail = mail;
                if (ModelState.IsValid)
                {
                    data.NewsLetters.InsertOnSubmit(nl);
                    data.SubmitChanges();
                }
                ViewBag.Thongbao = "Gửi thành công!";
            }
            return this.NewsLetter();
        }

        [HttpPost]
        public ActionResult ValidateCaptcha()
        {
            var respone = Request["g-recaptcha-response"];
            const string secret = "6LdrUR8jAAAAAC7Re69sMWRXyIjWyTM6NkuFQQ9z";
            var client = new WebClient();
            var reply = client.DownloadString(
                    string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}%response={1}",secret,respone)
                );
            var catpchaResponse = JsonConvert.DeserializeObject<reCaptcha>(reply);

            //check lỗi
            if (!catpchaResponse.Success)
            {
                if (catpchaResponse.ErrorCodes.Count <= 0) return View("Gioithieu");
                var error = catpchaResponse.ErrorCodes[0].ToLower();
                switch (error)
                {
                    case "missing-input-secret":
                        ViewBag.Message = "The secret parameter is missing.";
                        break;
                    case "invalid-input-secret":
                        ViewBag.Message = "The secret parameter is invalid or malformed.";
                        break;
                    case "missing-input-response":
                        ViewBag.Message = "The response parameter is missing.";
                        break;
                    case "invalid-input-response":
                        ViewBag.Message = "The response parameter is invalid or malformed.";
                        break;
                    case "bad-request":
                        ViewBag.Message = "The request is invalid or malformed.";
                        break;
                    case "timeout-or-duplicate":
                        ViewBag.Message = "The response is no longer valid: either is too old or has been used previously.";
                        break;
                    default:
                        ViewBag.Message = "Có lỗi, vui lòng thử lại sau!";
                        break;
                }
            }
            else
            {
                ViewBag.Message = "Xác thực thành công!";
            }
            return View("Gioithieu");
        }


        //
        //trang Liên hệ 
        public ActionResult Gioithieu()
        {
            if (Session["TaiKhoan"] == null || Session["TaiKhoan"].ToString() == "")
            {
                return RedirectToAction("Dangnhap", "KhachHang");
            }
            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Gioithieu(FormCollection collection)
        {
            LienHe lh = new LienHe();
            KhachHang kh = (KhachHang)Session["TaiKhoan"];
            var hoten = collection["HoTen"];
            var email = collection["Mail"];
            var sdt = collection["SDT"];
            var noidunglh = collection["NoiDung"];

            if (String.IsNullOrEmpty(hoten))
            {
                ViewData["Loi1"] = "Vui lòng nhập họ và tên!";
            }
            else if (String.IsNullOrEmpty(sdt))
            {
                ViewData["Loi2"] = "Vui lòng nhập số điện thoại!";
            }
            else if (String.IsNullOrEmpty(email))
            {
                ViewData["Loi3"] = "Vui lòng nhập Email!";
            }
            else if (String.IsNullOrEmpty(noidunglh))
            {
                ViewData["Loi4"] = "Vui lòng nhập nội dung!";
            }
            else
            {
                lh.id = kh.MaKH;
                lh.NoiDung = noidunglh;
                lh.HoTen = hoten;
                lh.Mail = email;
                lh.SDT = sdt;
                if (ModelState.IsValid)
                {
                    data.LienHes.InsertOnSubmit(lh);
                    data.SubmitChanges();
                }
                ViewBag.Thongbao = "Gửi thành công!";
                return RedirectToAction("Gioithieu");
            }
            return this.Gioithieu();
        }

        //chưa có view
        //trang giới thiệu
        public ActionResult GioithieuNew()
        {
            return View();
        }

        //-------------------------------------------------------------------------------------------
        //truyền tất cả tin tức

        public ActionResult IndexTinTuc(int? page)
        {
            int pageSize = 6;
            int pageNum = (page ?? 1);

            var tt = (from t in data.TinTucs select t).ToList();
            return View(tt.ToPagedList(pageNum,pageSize));
        }
        //truyền thể loại tin lên trang
        public ActionResult Theloaitin()
        {
            var tlt = from tt in data.TheLoaiTins select tt;
            return PartialView(tlt);
        }
        //lấy tin theo thể loại
        public ActionResult Tintheoloai(int id)
        {
            var t = from tt in data.TinTucs where tt.idLoai == id select tt;
            return View(t);
        }
        //chi tiết tin tức
        public ActionResult Chitiettintuc(int id)
        {
            var t = from tt in data.TinTucs where tt.idTT == id select tt;
            return View(t.Single());
        }

        //-------------------------------------------------------------------------------------------

    }
}