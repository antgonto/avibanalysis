using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectParser
{
    class Pair
    {
        bool banderaMinimoMaximo = false;

        string nombreI;
        string nombreD;

        int locSum = 0;
        float locAvg = 0; 
        int locMin = 0;
        int locMax = 0;

        int cycloSum = 0;
        float cycloAvg = 0;
        int cycloMin = 0;
        int cycloMax = 0;

        int constantSum = 0;
        float constantAvg = 0;
        int constantMin = 0;
        int constantMax = 0;

        public override string ToString()
        {
            return nombreI + "->" + nombreD + " - " + locSum + " - " + locAvg + " - " + locMin + " - " + locMax + " - " + cycloSum + " - " + cycloAvg + " - " +
                cycloMin + " - " + cycloMax + " - " + constantSum + " - " + constantAvg + " - " + constantMin + " - " + constantMax;
        }



        public bool BanderaMinimoMaximo
        {
            get
            {
                return banderaMinimoMaximo;
            }

            set
            {
                banderaMinimoMaximo = value;
            }
        }

       

        public int LocSum
        {
            get
            {
                return locSum;
            }

            set
            {
                locSum = value;
            }
        }

        public float LocAvg
        {
            get
            {
                return locAvg;
            }

            set
            {
                locAvg = value;
            }
        }

        public int LocMin
        {
            get
            {
                return locMin;
            }

            set
            {
                locMin = value;
            }
        }

        public int LocMax
        {
            get
            {
                return locMax;
            }

            set
            {
                locMax = value;
            }
        }

        public int CycloSum
        {
            get
            {
                return cycloSum;
            }

            set
            {
                cycloSum = value;
            }
        }

        public float CycloAvg
        {
            get
            {
                return cycloAvg;
            }

            set
            {
                cycloAvg = value;
            }
        }

        public int CycloMin
        {
            get
            {
                return cycloMin;
            }

            set
            {
                cycloMin = value;
            }
        }

        public int CycloMax
        {
            get
            {
                return cycloMax;
            }

            set
            {
                cycloMax = value;
            }
        }

        public int ConstantSum
        {
            get
            {
                return constantSum;
            }

            set
            {
                constantSum = value;
            }
        }

        public float ConstantAvg
        {
            get
            {
                return constantAvg;
            }

            set
            {
                constantAvg = value;
            }
        }

        public int ConstantMin
        {
            get
            {
                return constantMin;
            }

            set
            {
                constantMin = value;
            }
        }

        public int ConstantMax
        {
            get
            {
                return constantMax;
            }

            set
            {
                constantMax = value;
            }
        }

        public string NombreI
        {
            get
            {
                return nombreI;
            }

            set
            {
                nombreI = value;
            }
        }

        public string NombreD
        {
            get
            {
                return nombreD;
            }

            set
            {
                nombreD = value;
            }
        }

    }
}
