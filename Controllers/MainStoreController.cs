using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DACN.Models;
using PagedList;

namespace DACN.Controllers
{
    public class MainStoreController : Controller
    {
        //kết nối csdl
        DataClasses1DataContext data = new DataClasses1DataContext();
        // GET: MainStore

        //list sản phẩm mới cập nhật
        private List<SanPham> Sanphammoi(int count)
        {
            return data.SanPhams.OrderByDescending(a => a.NgayNhap).Take(count).ToList();
        }

        public ActionResult Index()
        {
            var spm = Sanphammoi(6);
            return PartialView(spm);
        }

        //Đưa tất cả sản phẩm lên trang
        public ActionResult Sanpham(int? page)
        {
            int pageSize = 12;
            int pageNum = (page ?? 1);

            var sp = (from s in data.SanPhams select s).ToList();
            return View(sp.ToPagedList(pageNum,pageSize));
        }

        //chưa có view
        //public ActionResult SearchSP(string search)
        //{
        //    var sp = from s in data.SanPhams select s;
        //    if (!String.IsNullOrEmpty(search))
        //    {
        //        sp = sp.Where(s => s.TenSP.Contains(search));
        //    }
        //    return View(sp.ToList());
        //}

        //sản phẩm cùng loại
        public ActionResult SpLienQuan(int id, int idLSP)
        {
            var spl = from s in data.SanPhams where s.idLoaiSP == idLSP where id != s.idSP select s;
            return View(spl);
        }



        //-------------------------------------------------------------------------------------------
        //chi tiết sản phẩm'
        public ActionResult Chitietsanpham(int id)
        {
            var ct = from tt in data.SanPhams where tt.idSP == id select tt;
            return View(ct.Single());
        }

        //-------------------------------------------------------------------------------------------
        //lấy sản phẩm theo loại
        public ActionResult Sptheoloai(int id)
        {
            var sptl = from tt in data.SanPhams where tt.idLoaiSP == id select tt;
            return View(sptl);
        }
        //truyền loại sản phẩm
        public ActionResult Loaisanpham()
        {
            var lsp = from tt in data.LoaiSPs select tt;
            return PartialView(lsp);
        }
        //-------------------------------------------------------------------------------------------

        //-------------------------------------------------------------------------------------------
        //lấy sản phẩm theo thương thiệu
        public ActionResult Sptheothuonghieu(int id)
        {
            var sptdm = from tt in data.SanPhams where tt.idThuongHieu == id select tt;
            return View(sptdm);
        }
        //truyền thương hiệu
        public ActionResult Thuonghieu()
        {
            var th = from tt in data.ThuongHieus select tt;
            return PartialView(th);
        }
        //-------------------------------------------------------------------------------------------

        //-------------------------------------------------------------------------------------------
        //truyền danh muc sản phẩm
        public ActionResult Danhmucsanpham()
        {
            var dmsp = from tt in data.DanhMucs select tt;
            return PartialView(dmsp);
        }
        //lấy sản phẩm theo danh mục
        public ActionResult Sptheodanhmuc(int id)
        {
            var dm = from tt in data.SanPhams where tt.idCate == id select tt;
            return View(dm);
        }
        //-------------------------------------------------------------------------------------------



    }
}