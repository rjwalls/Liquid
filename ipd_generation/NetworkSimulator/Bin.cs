
namespace NetworkSimulator.Bll
{
    public class Bin
    {
        private readonly int _binNum;
        private int _count;
        private decimal _max = decimal.MinValue;
        private decimal _min = decimal.MaxValue;

        public override string ToString()
        {
            return string.Format("Bin #: {0}; Range: {1} - {2}; Count: {3}", new object[] {_binNum, _min, _max, _count});
        }

        public Bin(int binNum)
        {
            _binNum = binNum;
        }

        public void AddValue(decimal value)
        {
            _count++;

            if (value > _max)
                _max = value;

            if (value < _min)
                _min = value;
        }

        public int BinNum
        {
            get { return _binNum; }
        }

        public int Count
        {
            get { return _count; }
        }

        public decimal MaxValue
        {
            get { return _max; }
        }

        public decimal MinValue
        {
            get { return _min; }
        }
    }
}