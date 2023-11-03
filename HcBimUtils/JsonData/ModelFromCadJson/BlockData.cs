namespace HcBimUtils.JsonData.ModelFromCadJson
{
    public class BlockData
    {
        public double Rotate { get; set; }
        public XyzData Origin { get; set; }

        public BlockData()
        {
        }

        public override bool Equals(object obj)
        {
            if (obj is BlockData data)
            {
                if (data.Origin.X.IsEqual(Origin.X, 1)
                    && data.Origin.Y.IsEqual(Origin.Y, 1)
                    && data.Origin.Z.IsEqual(Origin.Z, 1))
                {
                    return true;
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}