namespace HcBimUtils.JsonData.Beam
{
    public class BeamQuickSettingJson
    {
        public bool IsCreateSheet { get; set; }
        public int TopMainBarDiameter { get; set; }
        public int BotMainBarDiameter { get; set; }
        public int TopAdditionalBarDiameter1 { get; set; }
        public int BotAdditionBarDiameter1 { get; set; }
        public int TopAdditionalBarDiameter2 { get; set; }
        public int BotAdditionBarDiameter2 { get; set; }
        public double LengthNeedAdditionalBotBar { get; set; } = 2000.MmToFoot();
        public bool HasMainTopBar { get; set; }
        public bool HasMainBotBar { get; set; }

        public bool HasTop1 { get; set; }

        public bool HasBot1 { get; set; }

        public bool HasTop2 { get; set; }

        public bool HasBot2 { get; set; }
        public int MainTop1 { get; set; } = 2;
        public int AddTop1 { get; set; } = 2;
        public int AddTop2 { get; set; } = 2;

        public int MainBot1 { get; set; } = 2;
        public int AddBot1 { get; set; } = 2;
        public int AddBot2 { get; set; } = 2;

        public bool HasStirrup { get; set; }

        public string KieuBoTriThepDai { get; set; }
        public int StirrupBarDiameter { get; set; }
        public int A1 { get; set; }
        public int A2 { get; set; }
        public double L { get; set; }
    }
}