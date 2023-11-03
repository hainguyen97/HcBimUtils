using System.Collections.ObjectModel;

namespace HcBimUtils.JsonData.Beam
{
    public class BeamRebarSettingJson
    {
        public ObservableCollection<NumberOfRebarByWidth> NumberOfRebarByWidths { get; set; } = new ObservableCollection<NumberOfRebarByWidth>();

        public bool NeoThepTheoQuyDinh { get; set; } = true;
        public AnchorRebarSetting AnchorRebarSettingForWall { get; set; } = new AnchorRebarSetting();
        public AnchorRebarSetting AnchorRebarSettingForColumn { get; set; } = new AnchorRebarSetting();
        public AnchorRebarSetting AnchorRebarSettingForBeam { get; set; } = new AnchorRebarSetting();
        public AnchorRebarSetting AnchorRebarSettingForFoundation { get; set; } = new AnchorRebarSetting();

        #region Con Ke Thep

        public ConKeThepModel ConKeThep { get; set; }

        #endregion Con Ke Thep

        #region ThepCauTaoLopTren

        public int DuongKinhThepCauTaoLopTren { get; set; }
        public double ChieuDaiDoanNoiThepCauTaoLopTren { get; set; }

        #endregion ThepCauTaoLopTren

        #region Chieu Dai Toi Thieu Don Moc

        public double ChieuDaiToiThieuDoanMoc { get; set; }

        #endregion Chieu Dai Toi Thieu Don Moc

        #region Khoang Cach Dai Gia Cuong

        public double KhoangCachDaiGiaCuong { get; set; }
        public int SoLuongDaiGiaCuong { get; set; } = 5;
        public int DuongKinhDaiGiaCuong { get; set; } = 8;

        #endregion Khoang Cach Dai Gia Cuong

        #region Chieu Dai Thep Bo Xung

        public double ThepNhip { get; set; }
        public double ThepNhip2 { get; set; }
        public double ThepNhip3 { get; set; }
        public double ThepGoiLop1 { get; set; }
        public double ThepGoiLop2 { get; set; }
        public double ThepGoiLop3 { get; set; }

        #endregion Chieu Dai Thep Bo Xung

        #region Thep Cau Tao Giua Dam

        public ThepCauTaoGiuaDamModel ThepCauTaoGiuaDamModel { get; set; }

        #endregion Thep Cau Tao Giua Dam

        #region Cot Thep Dai Bo Xung

        public int DuongKinhCotThepDaiBoXung { get; set; }
        public double KhoangCachCotThepDaiBoXung { get; set; }

        #endregion Cot Thep Dai Bo Xung

        #region Thep Vai Bo

        public bool TaoThepVaiBoDamPhu { get; set; } = true;
        public int DuongKhiThepVaiBo { get; set; } = 16;
        public int SoLuongThepVaiBo { get; set; } = 2;
        public double DoanKeoDai2Ben { get; set; } = 300.MmToFoot();

        #endregion Thep Vai Bo

        public double KhoangGiatCapDuocNhanThep { get; set; } = 50.MmToFoot();

        public double RebarCover { get; set; } = 25.MmToFoot();
        public double RebarDistance2Layers { get; set; } = 25.MmToFoot();

        public double Position1 { get; set; } = 0.1;
        public double Position2 { get; set; } = 0.5;
        public double Position3 { get; set; } = 0.9;

        public BeamShopSettingJson BeamShopSetting { get; set; } = new BeamShopSettingJson();

        public BeamRebarSettingJson()
        {
        }
    }

    public class BeamShopSettingJson
    {
        public double TopShopSpanFactor { get; set; } = 1 / 3.0;
        public double BotShopSpanFactor { get; set; } = 1 / 4.0;
        public double Distance2LapFactor { get; set; } = 10.0;
        public double FactorTopLapping { get; set; } = 40;
        public double FactorBotLapping { get; set; } = 30;
        public double MaxLengthOfOneRebar { get; set; } = 11.7.MeterToFoot();
        public string TagTypeStandardBar { get; set; }
        public string TagTypeStirrupBar { get; set; }
        public string HatchTypeNameCutZone { get; set; }
    }
}