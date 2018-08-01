using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectParser
{
    public enum WeightType { CONST, CYCLO, LOC };
    public enum ValueType { SUM, AVG, MIN, MAX };

    class Normalizer
    {
        // Max and Min values to use in Normalization
        float locSumMin = float.MaxValue;
        float locAvgMin = float.MaxValue;
        float locMinMin = float.MaxValue;
        float locMaxMin = float.MaxValue;

        float cycloSumMin = float.MaxValue;
        float cycloAvgMin = float.MaxValue;
        float cycloMinMin = float.MaxValue;
        float cycloMaxMin = float.MaxValue;

        float constantSumMin = float.MaxValue;
        float constantAvgMin = float.MaxValue;
        float constantMinMin = float.MaxValue;
        float constantMaxMin = float.MaxValue;

        float locSumMax = float.MinValue;
        float locAvgMax = float.MinValue;
        float locMinMax = float.MinValue;
        float locMaxMax = float.MinValue;

        float cycloSumMax = float.MinValue;
        float cycloAvgMax = float.MinValue;
        float cycloMinMax = float.MinValue;
        float cycloMaxMax = float.MinValue;

        float constantSumMax = float.MinValue;
        float constantAvgMax = float.MinValue;
        float constantMinMax = float.MinValue;
        float constantMaxMax = float.MinValue;

        public float LocSumMin
        {
            get
            {
                return locSumMin;
            }

            set
            {
                locSumMin = value;
            }
        }

        public float LocAvgMin
        {
            get
            {
                return locAvgMin;
            }

            set
            {
                locAvgMin = value;
            }
        }

        public float LocMinMin
        {
            get
            {
                return locMinMin;
            }

            set
            {
                locMinMin = value;
            }
        }

        public float LocMaxMin
        {
            get
            {
                return locMaxMin;
            }

            set
            {
                locMaxMin = value;
            }
        }

        public float CycloSumMin
        {
            get
            {
                return cycloSumMin;
            }

            set
            {
                cycloSumMin = value;
            }
        }

        public float CycloAvgMin
        {
            get
            {
                return cycloAvgMin;
            }

            set
            {
                cycloAvgMin = value;
            }
        }

        public float CycloMinMin
        {
            get
            {
                return cycloMinMin;
            }

            set
            {
                cycloMinMin = value;
            }
        }

        public float CycloMaxMin
        {
            get
            {
                return cycloMaxMin;
            }

            set
            {
                cycloMaxMin = value;
            }
        }

        public float ConstantSumMin
        {
            get
            {
                return constantSumMin;
            }

            set
            {
                constantSumMin = value;
            }
        }

        public float ConstantAvgMin
        {
            get
            {
                return constantAvgMin;
            }

            set
            {
                constantAvgMin = value;
            }
        }

        public float ConstantMinMin
        {
            get
            {
                return constantMinMin;
            }

            set
            {
                constantMinMin = value;
            }
        }

        public float ConstantMaxMin
        {
            get
            {
                return constantMaxMin;
            }

            set
            {
                constantMaxMin = value;
            }
        }

        public float LocSumMax
        {
            get
            {
                return locSumMax;
            }

            set
            {
                locSumMax = value;
            }
        }

        public float LocAvgMax
        {
            get
            {
                return locAvgMax;
            }

            set
            {
                locAvgMax = value;
            }
        }

        public float LocMinMax
        {
            get
            {
                return locMinMax;
            }

            set
            {
                locMinMax = value;
            }
        }

        public float LocMaxMax
        {
            get
            {
                return locMaxMax;
            }

            set
            {
                locMaxMax = value;
            }
        }

        public float CycloSumMax
        {
            get
            {
                return cycloSumMax;
            }

            set
            {
                cycloSumMax = value;
            }
        }

        public float CycloAvgMax
        {
            get
            {
                return cycloAvgMax;
            }

            set
            {
                cycloAvgMax = value;
            }
        }

        public float CycloMinMax
        {
            get
            {
                return cycloMinMax;
            }

            set
            {
                cycloMinMax = value;
            }
        }

        public float CycloMaxMax
        {
            get
            {
                return cycloMaxMax;
            }

            set
            {
                cycloMaxMax = value;
            }
        }

        public float ConstantSumMax
        {
            get
            {
                return constantSumMax;
            }

            set
            {
                constantSumMax = value;
            }
        }

        public float ConstantAvgMax
        {
            get
            {
                return constantAvgMax;
            }

            set
            {
                constantAvgMax = value;
            }
        }

        public float ConstantMinMax
        {
            get
            {
                return constantMinMax;
            }

            set
            {
                constantMinMax = value;
            }
        }

        public float ConstantMaxMax
        {
            get
            {
                return constantMaxMax;
            }

            set
            {
                constantMaxMax = value;
            }
        }

        public float Normalize(float f, WeightType w, ValueType v)
        {
            float min = 0, max = 1;

            switch (w)
            {
                case WeightType.CONST:
                    switch (v)
                    {
                        case ValueType.SUM:
                            min = ConstantSumMin;
                            max = ConstantSumMax;
                            break;
                        case ValueType.AVG:
                            min = ConstantAvgMin;
                            max = ConstantAvgMax;
                            break;
                        case ValueType.MIN:
                            min = ConstantMinMin;
                            max = ConstantMaxMax;
                            break;
                        case ValueType.MAX:
                            min = ConstantMaxMin;
                            max = ConstantMaxMax;
                            break;
                    }
                    break;
                case WeightType.LOC:
                    switch (v)
                    {
                        case ValueType.SUM:
                            min = LocSumMin;
                            max = LocSumMax;
                            break;
                        case ValueType.AVG:
                            min = LocAvgMin;
                            max = LocAvgMax;
                            break;
                        case ValueType.MIN:
                            min = LocMinMin;
                            max = LocMaxMax;
                            break;
                        case ValueType.MAX:
                            min = LocMaxMin;
                            max = LocMaxMax;
                            break;
                    }
                    break;
                case WeightType.CYCLO:
                    switch (v)
                    {
                        case ValueType.SUM:
                            min = CycloSumMin;
                            max = CycloSumMax;
                            break;
                        case ValueType.AVG:
                            min = CycloAvgMin;
                            max = CycloAvgMax;
                            break;
                        case ValueType.MIN:
                            min = CycloMinMin;
                            max = CycloMaxMax;
                            break;
                        case ValueType.MAX:
                            min = CycloMaxMin; 
                            max = CycloMaxMax;
                            break;
                    }
                    break;
                default:
                    break;
            }

            return (f - min) / (max - min);
        }

        public float NormalizeCycloSum(float f)
        {
            return Normalize(f, WeightType.CYCLO, ValueType.SUM);
        }

        public float NormalizeCycloMax(float f)
        {
            return Normalize(f, WeightType.CYCLO, ValueType.MAX);
        }

        public float NormalizeCycloAvg(float f)
        {
            return Normalize(f, WeightType.CYCLO, ValueType.AVG);
        }
    }
}