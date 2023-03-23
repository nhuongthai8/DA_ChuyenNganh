using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DACN.Models;
using PagedList;

namespace DACN.Controllers
{
    public class AdminController : Controller
    {
        DataClasses1DataContext data = new DataClasses1DataContext();
        // GET: Admin
        public ActionResult IndexAdmin()
        {
            return View();
        }

        //------------------------------------------------------------------------------------------------------------------------------
        //đăng nhập admin
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(FormCollection collection)
        {
            var tendn = collection["username"];
            var matkhau = collection["password"];
            if (String.IsNullOrEmpty(tendn))
            {
                ViewData["Loi1"] = "Vui lòng điền tài khoản";
            }
            else if (String.IsNullOrEmpty(matkhau))
            {
                ViewData["Loi2"] = "Vui lòng nhập mật khẩu";
            }
            else
            {
                Admin admin = data.Admins.SingleOrDefault(a => a.tkAdmin == tendn && a.passAdmin == matkhau);
                if (admin != null)
                {
                    Session["Taikhoanadmin"] = admin;
                    return RedirectToAction("IndexAdmin", "Admin");
                }
                else
                    ViewBag.ThongBao = "Sai tài khoản hoặc mật khẩu";
            }
            return View();
        }
        //------------------------------------------------------------------------------------------------------------------------------

        //------------------------------------------------------------------------------------------------------------------------------
        //trang quản lý sản phẩm
        public ActionResult SanPham(int ? page)
        {
            int pageNum = (page ?? 1);
            int pageSize = 7;

            return View(data.SanPhams.ToList().OrderBy(a=>a.idSP).ToPagedList(pageNum,pageSize));
        }
        //------------------------------------------------------------------------------------------------------------------------------
        //Chức năng thêm, xóa, sửa, xem chi tiết sản phẩm
        [HttpGet]
        public ActionResult CreateSP()
        {
            ViewBag.idLoaiSP = new SelectList(data.LoaiSPs.ToList().OrderBy(a => a.TenLSP), "idLoaiSP", "TenLSP");
            ViewBag.idCate = new SelectList(data.DanhMucs.ToList().OrderBy(a => a.TenCate), "idCate", "TenCate");
            ViewBag.idThuongHieu = new SelectList(data.ThuongHieus.ToList().OrderBy(b => b.TenTH), "idThuongHieu", "TenTH");
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateSP(SanPham sp, HttpPostedFileBase fileupload)
        {
            ViewBag.idLoaiSP = new SelectList(data.LoaiSPs.ToList().OrderBy(a => a.TenLSP), "idLoaiSP", "TenLSP");
            ViewBag.idCate = new SelectList(data.DanhMucs.ToList().OrderBy(a => a.TenCate), "idCate", "TenCate");
            ViewBag.idThuongHieu = new SelectList(data.ThuongHieus.ToList().OrderBy(b => b.TenTH), "idThuongHieu", "TenTH");

            if (fileupload == null)
            {
                ViewBag.ThongBao = "Vui lòng chọn ảnh bìa";
                return View();
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var filename = Path.GetFileName(fileupload.FileName);
                    var path = Path.Combine(Server.MapPath("~/HinhSP"), filename);
                    if (System.IO.File.Exists(path))
                    {
                        ViewBag.ThongBao = "Hình ảnh đã tồn tại";
                    }
                    else
                    {
                        fileupload.SaveAs(path);
                    }
                    sp.HinhSP = filename;
                    data.SanPhams.InsertOnSubmit(sp);
                    data.SubmitChanges();
                }
                return RedirectToAction("SanPham");
            }

        }

        public ActionResult DetailsSP(int id)
        {
            SanPham sp = data.SanPhams.SingleOrDefault(a => a.idSP == id);
            ViewBag.idSP = sp.idSP;
            if (sp == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(sp);
        }

        [HttpGet]
        public ActionResult DeleteSP(int id)
        {
            SanPham sp = data.SanPhams.SingleOrDefault(a => a.idSP == id);
            ViewBag.idSP = sp.idSP;
            if (sp == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(sp);
        }
        [HttpPost, ActionName("DeleteSP")]
        public ActionResult ConfirmDelete(int id)
        {
            SanPham sp = data.SanPhams.SingleOrDefault(a => a.idSP == id);
            ViewBag.idSP = sp.idSP;
            if (sp == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            data.SanPhams.DeleteOnSubmit(sp);
            data.SubmitChanges();
            return RedirectToAction("SanPham");
        }

        [HttpGet]
        public ActionResult EditSP(int id)
        {
            SanPham sp = data.SanPhams.SingleOrDefault(a => a.idSP == id);
            if (sp == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            ViewBag.idLoaiSP = new SelectList(data.LoaiSPs.ToList().OrderBy(a => a.TenLSP), "idLoaiSP", "TenLSP");
            ViewBag.idCate = new SelectList(data.DanhMucs.ToList().OrderBy(a => a.TenCate), "idCate", "TenCate");
            ViewBag.idThuongHieu = new SelectList(data.ThuongHieus.ToList().OrderBy(b => b.TenTH), "idThuongHieu", "TenTH");
            return View(sp);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditSP(int id, HttpPostedFileBase fileUpload)
        {
            var sp = data.SanPhams.FirstOrDefault(a => a.idSP == id);
            sp.idSP = id;
            ViewBag.idLoaiSP = new SelectList(data.LoaiSPs.ToList().OrderBy(a => a.TenLSP), "idLoaiSP", "TenLSP");
            ViewBag.idCate = new SelectList(data.DanhMucs.ToList().OrderBy(a => a.TenCate), "idCate", "TenCate");
            ViewBag.idThuongHieu = new SelectList(data.ThuongHieus.ToList().OrderBy(b => b.TenTH), "idThuongHieu", "TenTH");
            if (fileUpload == null)
            {
                ViewBag.ThongBao = "Vui lòng chọn ảnh bìa";
                return View(sp);
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var fileName = Path.GetFileName(fileUpload.FileName);
                    var path = Path.Combine(Server.MapPath("~/HinhSP"), fileName);
                    if (System.IO.File.Exists(path))
                        ViewBag.ThongBao = "Ảnh đã tồn tại";
                    else
                    {
                        fileUpload.SaveAs(path);
                    }
                    sp.HinhSP = fileName;
                    sp.idSP = id;
                    UpdateModel(sp);
                    data.SubmitChanges();
                    return RedirectToAction("SanPham");
                }
                return this.EditSP(id);
            }

        }
        //------------------------------------------------------------------------------------------------------------------------------

        //------------------------------------------------------------------------------------------------------------------------------
        //trang quản lý loại sản phẩm
        public ActionResult LoaiSanPham(int? page)
        {
            int pageNum = (page ?? 1);
            int pageSize = 7;

            return View(data.LoaiSPs.ToList().OrderBy(a => a.idLoaiSP).ToPagedList(pageNum, pageSize));
        }
        //------------------------------------------------------------------------------------------------------------------------------
        //Chức năng thêm, xóa, sửa, xem loại sản phẩm
        [HttpGet]
        public ActionResult CreateLSP()
        {
            ViewBag.idCate = new SelectList(data.DanhMucs.ToList().OrderBy(a => a.TenCate), "idCate", "TenCate");
            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateLSP(LoaiSP lsp)
        {
            data.LoaiSPs.InsertOnSubmit(lsp);
            data.SubmitChanges();
            return RedirectToAction("LoaiSanPham");
        }
        //sửa
        [HttpGet]
        public ActionResult EditLSP(int id)
        {
            ViewBag.idCate = new SelectList(data.DanhMucs.ToList().OrderBy(a => a.TenCate), "idCate", "TenCate");
            LoaiSP lsp = data.LoaiSPs.SingleOrDefault(a => a.idLoaiSP == id);
            return View(lsp);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditLSp(int id)
        {
            var lsp = data.LoaiSPs.FirstOrDefault(a => a.idLoaiSP == id);
            lsp.idLoaiSP = id;
            if (ModelState.IsValid)
            {
                UpdateModel(lsp);
                data.SubmitChanges();
                return RedirectToAction("LoaiSanPham");
            }
            return this.EditLSP(id);
        }
        //xóa
        [HttpGet]
        public ActionResult DeleteLSP(int id)
        {
            LoaiSP lsp = data.LoaiSPs.SingleOrDefault(a => a.idLoaiSP == id);

            return View(lsp);
        }
        [HttpPost, ActionName("DeleteLSP")]
        public ActionResult ConfirmDeleteLSP(int id)
        {
            LoaiSP lsp = data.LoaiSPs.SingleOrDefault(a => a.idLoaiSP == id);
            ViewBag.idLoaiSP = lsp.idLoaiSP; //viewbag dùng để chuyển kiểu dữ liệu như viewdata
            if (lsp == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            data.LoaiSPs.DeleteOnSubmit(lsp);
            data.SubmitChanges();
            return RedirectToAction("LoaiSanPham");
        }
        //xem
        public ActionResult DetailsLSP(int id)
        {
            LoaiSP lsp = data.LoaiSPs.SingleOrDefault(a => a.idLoaiSP == id);
            ViewBag.idLoaiSP = lsp.idLoaiSP;
            if (lsp == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(lsp);
        }
        //------------------------------------------------------------------------------------------------------------------------------
        //trang quản lý danh mục
        public ActionResult DanhMuc(int? page)
        {
            int pageNum = (page ?? 1);
            int pageSize = 7;

            return View(data.DanhMucs.ToList().OrderBy(a => a.idCate).ToPagedList(pageNum, pageSize));
        }
        //------------------------------------------------------------------------------------------------------------------------------
        //Chức năng thêm, xóa, sửa, xem danh mục
        //thêm
        [HttpGet]
        public ActionResult CreateDM()
        {
            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateDM(DanhMuc dm)
        {
            data.DanhMucs.InsertOnSubmit(dm);
            data.SubmitChanges();
            return RedirectToAction("DanhMuc");
        }
        //sửa
        [HttpGet]
        public ActionResult EditDM(int id)
        {
            DanhMuc dm = data.DanhMucs.SingleOrDefault(a => a.idCate == id);
            return View(dm);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditDm(int id)
        {
            var dm = data.DanhMucs.FirstOrDefault(a => a.idCate == id);
            dm.idCate = id;
            if (ModelState.IsValid)
            {
                UpdateModel(dm);
                data.SubmitChanges();
                return RedirectToAction("DanhMuc");
            }
            return this.EditDM(id);
        }
        //xóa
        [HttpGet]
        public ActionResult DeleteDM(int id)
        {
            DanhMuc dm = data.DanhMucs.SingleOrDefault(a => a.idCate == id);

            return View(dm);
        }
        [HttpPost, ActionName("DeleteDM")]
        public ActionResult ConfirmDeleteDM(int id)
        {
            DanhMuc dm = data.DanhMucs.SingleOrDefault(a => a.idCate == id);
            ViewBag.idCate = dm.idCate; //viewbag dùng để chuyển kiểu dữ liệu như viewdata
            if (dm == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            data.DanhMucs.DeleteOnSubmit(dm);
            data.SubmitChanges();
            return RedirectToAction("DanhMuc");
        }
        //xem
        public ActionResult DetailsDM(int id)
        {
            DanhMuc dm = data.DanhMucs.SingleOrDefault(a => a.idCate == id);
            ViewBag.idCate = dm.idCate;
            if (dm == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(dm);
        }
        //------------------------------------------------------------------------------------------------------------------------------
        //Trang quản lý thông tin khách hàng
        public ActionResult KhachHang()
        {
            return View(data.KhachHangs.ToList().OrderBy(a => a.MaKH));
        }
        //sửa
        [HttpGet]
        public ActionResult EditKH(int id)
        {
            KhachHang kh = data.KhachHangs.SingleOrDefault(a => a.MaKH == id);
            return View(kh);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditKh(int id)
        {
            var kh = data.KhachHangs.FirstOrDefault(a => a.MaKH == id);
            kh.MaKH = id;
            if (ModelState.IsValid)
            {
                UpdateModel(kh);
                data.SubmitChanges();
                return RedirectToAction("KhachHang");
            }
            return this.EditKH(id);
        }
        //chi tiết
        public ActionResult DetailsKH(int id)
        {
            KhachHang kh = data.KhachHangs.SingleOrDefault(a => a.MaKH == id);
            ViewBag.MaKH = kh.MaKH;
            if (kh == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(kh);
        }
        //xóa
        [HttpGet]
        public ActionResult DeleteKH(int id)
        {
            KhachHang kh = data.KhachHangs.SingleOrDefault(a => a.MaKH == id);
            return View(kh);
        }
        [HttpPost, ActionName("DeleteKH")]
        public ActionResult DeleteKh(int id)
        {
            KhachHang kh = data.KhachHangs.SingleOrDefault(a => a.MaKH == id);
            ViewBag.MaKH = kh.MaKH;
            if (kh == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            data.KhachHangs.DeleteOnSubmit(kh);
            data.SubmitChanges();
            return RedirectToAction("KhachHang");
        }
        //------------------------------------------------------------------------------------------------------------------------------
        //Trang quản lý đơn hàng
        public ActionResult DonHang(int? page)
        {
            int pageNum = (page ?? 1);
            int pageSzie = 7;
            return View(data.ChiTietOrders.ToList().OrderBy(a => a.MaOrder).ToPagedList(pageNum, pageSzie));
        }
        //chi tiết
        public ActionResult DetailsDH(int id)
        {
            ChiTietOrder ct = data.ChiTietOrders.FirstOrDefault(a => a.MaOrder == id);
            ViewBag.MaOrder = ct.MaOrder;
            return View(ct);
        }
        //sửa
        [HttpGet]
        public ActionResult EditDH(int id)
        {
            ChiTietOrder ct = data.ChiTietOrders.SingleOrDefault(a => a.MaOrder == id);
            return View(ct);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditDh(int id)
        {
            var ct = data.ChiTietOrders.FirstOrDefault(a => a.MaOrder == id);
            ct.MaOrder = id;
            if (ModelState.IsValid)
            {
                UpdateModel(ct);
                data.SubmitChanges();
                return RedirectToAction("DonHang");
            }
            return this.EditDH(id);
        }
        //------------------------------------------------------------------------------------------------------------------------------
        //Trang quản lý thể loại tin tức
        public ActionResult TheLoaiTin()
        {
            return View(data.TheLoaiTins.ToList().OrderBy(a => a.idLoai));
        }
        //thêm
        [HttpGet]
        public ActionResult CreateTLT()
        {
            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateTLT(TheLoaiTin tlt)
        {
            data.TheLoaiTins.InsertOnSubmit(tlt);
            data.SubmitChanges();
            return RedirectToAction("TheLoaiTin");
        }
        //xóa
        [HttpGet]
        public ActionResult DeleteTLT(int id)
        {
            TheLoaiTin tlt = data.TheLoaiTins.SingleOrDefault(a => a.idLoai == id);
            return View(tlt);
        }
        [HttpPost, ActionName("DeleteTLT")]
        public ActionResult DeleteTlt(int id)
        {
            TheLoaiTin tlt = data.TheLoaiTins.SingleOrDefault(a => a.idLoai == id);
            ViewBag.idLoai = tlt.idLoai;
            if (tlt == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            data.TheLoaiTins.DeleteOnSubmit(tlt);
            data.SubmitChanges();
            return RedirectToAction("TheLoaiTin");
        }
        //sửa
        [HttpGet]
        public ActionResult EditTLT(int id)
        {
            TheLoaiTin tlt = data.TheLoaiTins.SingleOrDefault(a => a.idLoai == id);
            return View(tlt);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditTlt(int id)
        {
            var tlt = data.TheLoaiTins.FirstOrDefault(a => a.idLoai == id);
            tlt.idLoai = id;
            if (ModelState.IsValid)
            {
                UpdateModel(tlt);
                data.SubmitChanges();
                return RedirectToAction("TheLoaiTin");
            }
            return this.EditTLT(id);
        }
        //------------------------------------------------------------------------------------------------------------------------------
        //Trang quản lý tin tức
        public ActionResult TinTuc(int? page)
        {
            int pageNum = (page ?? 1);
            int pageSize = 5;
            return View(data.TinTucs.ToList().OrderBy(a => a.idTT).ToPagedList(pageNum,pageSize));
        }
        //chi tiết
        public ActionResult DetailsTT(int id)
        {
            TinTuc tt = data.TinTucs.FirstOrDefault(a => a.idTT == id);
            return View(tt);
        }
        //thêm
        [HttpGet]
        public ActionResult CreateTT()
        {
            ViewBag.idLoai = new SelectList(data.TheLoaiTins.ToList().OrderBy(b => b.TenLoai), "idLoai", "TenLoai");
            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateTT(TinTuc tt, HttpPostedFileBase fileupload)
        {
            ViewBag.idLoai = new SelectList(data.TheLoaiTins.ToList().OrderBy(b => b.TenLoai), "idLoai", "TenLoai");

            if (fileupload == null)
            {
                ViewBag.ThongBao = "Vui lòng chọn ảnh bìa";
                return View();
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var filename = Path.GetFileName(fileupload.FileName);
                    var path = Path.Combine(Server.MapPath("~/HinhTT"), filename);
                    if (System.IO.File.Exists(path))
                    {
                        ViewBag.ThongBao = "Hình ảnh đã tồn tại";
                    }
                    else
                    {
                        fileupload.SaveAs(path);
                    }
                    tt.Anhbia = filename;
                    data.TinTucs.InsertOnSubmit(tt);
                    data.SubmitChanges();
                }
                return RedirectToAction("TinTuc");
            }
        }
        //xóa
        [HttpGet]
        public ActionResult DeleteTT(int id)
        {
            TinTuc nl = data.TinTucs.SingleOrDefault(a => a.idTT == id);
            return View(nl);
        }
        [HttpPost, ActionName("DeleteTT")]
        public ActionResult DeleteTt(int id)
        {
            TinTuc nl = data.TinTucs.SingleOrDefault(a => a.idTT == id);
            ViewBag.id = nl.idTT;
            if (nl == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            data.TinTucs.DeleteOnSubmit(nl);
            data.SubmitChanges();
            return RedirectToAction("TinTuc");
        }
        //sửa
        [HttpGet]
        public ActionResult EditTT(int id)
        {
            TinTuc tt = data.TinTucs.SingleOrDefault(a => a.idTT == id);
            if (tt == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            ViewBag.idLoai = new SelectList(data.TheLoaiTins.ToList().OrderBy(b => b.TenLoai), "idLoai", "TenLoai");
            return View(tt);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditTT(int id, HttpPostedFileBase fileUpload)
        {
            var tt = data.TinTucs.FirstOrDefault(a => a.idTT == id);
            tt.idTT = id;
            ViewBag.idLoai = new SelectList(data.TheLoaiTins.ToList().OrderBy(b => b.TenLoai), "idLoai", "TenLoai");
            if (fileUpload == null)
            {
                ViewBag.ThongBao = "Vui lòng chọn ảnh bìa";
                return View(tt);
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var fileName = Path.GetFileName(fileUpload.FileName);
                    var path = Path.Combine(Server.MapPath("~/HinhTT"), fileName);
                    if (System.IO.File.Exists(path))
                        ViewBag.ThongBao = "Ảnh đã tồn tại";
                    else
                    {
                        fileUpload.SaveAs(path);
                    }
                    tt.Anhbia = fileName;
                    tt.idTT = id;
                    UpdateModel(tt);
                    data.SubmitChanges();
                    return RedirectToAction("TinTuc");
                }
                return this.EditTT(id);
            }
        }

        //------------------------------------------------------------------------------------------------------------------------------
        //thông tin liên hệ
        public ActionResult LienHe()
        {
            return View(data.LienHes.ToList().OrderBy(a=>a.id));
        }
        //chi tiết
        public ActionResult DetailsLH(int id)
        {
            LienHe lh = data.LienHes.FirstOrDefault(a => a.id == id);
            return View(lh);
        }
        //xóa
        [HttpGet]
        public ActionResult DeleteLH(int id)
        {
            LienHe nl = data.LienHes.SingleOrDefault(a => a.id == id);
            return View(nl);
        }
        [HttpPost, ActionName("DeleteLH")]
        public ActionResult DeleteLh(int id)
        {
            LienHe nl = data.LienHes.SingleOrDefault(a => a.id == id);
            ViewBag.id = nl.id;
            if (nl == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            data.LienHes.DeleteOnSubmit(nl);
            data.SubmitChanges();
            return RedirectToAction("LienHe");
        }
        //------------------------------------------------------------------------------------------------------------------------------
        //trang mai nhận tin
        public ActionResult NewsLetter()
        {
            return View(data.NewsLetters.ToList().OrderBy(a=>a.id));
        }
        //xóa
        [HttpGet]
        public ActionResult DeleteNL(int id)
        {
            NewsLetter nl = data.NewsLetters.SingleOrDefault(a => a.id == id);
            return View(nl);
        }
        [HttpPost, ActionName("DeleteNL")]
        public ActionResult DeleteNl(int id)
        {
            NewsLetter nl = data.NewsLetters.SingleOrDefault(a => a.id == id);
            ViewBag.id = nl.id;
            if (nl == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            data.NewsLetters.DeleteOnSubmit(nl);
            data.SubmitChanges();
            return RedirectToAction("NewsLetter");
        }
        //------------------------------------------------------------------------------------------------------------------------------
        //trang quản lý thương hiệu
        public ActionResult ThuongHieu()
        {
            return View(data.ThuongHieus.ToList().OrderBy(a=>a.idThuongHieu));
        }
        //thêm
        [HttpGet]
        public ActionResult CreateTH()
        {
            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateTH(ThuongHieu tlt)
        {
            data.ThuongHieus.InsertOnSubmit(tlt);
            data.SubmitChanges();
            return RedirectToAction("ThuongHieu");
        }
        //chi tiết
        public ActionResult DetailsTH(int id)
        {
            ThuongHieu lh = data.ThuongHieus.FirstOrDefault(a => a.idThuongHieu == id);
            return View(lh);
        }
        //xóa
        [HttpGet]
        public ActionResult DeleteTH(int id)
        {
            ThuongHieu nl = data.ThuongHieus.SingleOrDefault(a => a.idThuongHieu == id);
            return View(nl);
        }
        [HttpPost, ActionName("DeleteTH")]
        public ActionResult DeleteTh(int id)
        {
            ThuongHieu nl = data.ThuongHieus.SingleOrDefault(a => a.idThuongHieu == id);
            ViewBag.id = nl.idThuongHieu;
            if (nl == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            data.ThuongHieus.DeleteOnSubmit(nl);
            data.SubmitChanges();
            return RedirectToAction("ThuongHieu");
        }
        //sửa
        [HttpGet]
        public ActionResult EditTH(int id)
        {
            ThuongHieu tlt = data.ThuongHieus.SingleOrDefault(a => a.idThuongHieu == id);
            return View(tlt);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditTh(int id)
        {
            var tlt = data.ThuongHieus.FirstOrDefault(a => a.idThuongHieu == id);
            tlt.idThuongHieu = id;
            if (ModelState.IsValid)
            {
                UpdateModel(tlt);
                data.SubmitChanges();
                return RedirectToAction("ThuongHieu");
            }
            return this.EditTH(id);
        }
        //------------------------------------------------------------------------------------------------------------------------------
    }
}