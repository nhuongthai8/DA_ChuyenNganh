using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DACN.Models
{
    public class ShoppingCart
    {
        DataClasses1DataContext data = new DataClasses1DataContext();
        public int iMaSP { get; set; }
        public string sTenSP { get; set; }
        public string sAnhbia { get; set; }
        public Double dGiatien { get; set; }
        public int iSoluong { get; set; }
        public Double dThanhtien
        {
            get { return iSoluong * dGiatien; }
        }
        public ShoppingCart(int idSP)
        {
            iMaSP = idSP;
            SanPham sp = data.SanPhams.Single(n => n.idSP == iMaSP);
            sTenSP = sp.TenSP;
            sAnhbia = sp.HinhSP;
            dGiatien = double.Parse(sp.GiaTien.ToString());
            iSoluong = 1;
        }
    }
}