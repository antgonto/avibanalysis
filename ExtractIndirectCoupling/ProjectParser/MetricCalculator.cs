using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectParser
{
    public class SparseMatrix<T>
    {
        public int Width { get; private set; }
        public int Height { get; private set; } 
        public long Size { get; private set; }

        private Dictionary<long, T> _cells = new Dictionary<long, T>();

        public SparseMatrix(int w, int h)
        {
            this.Width = w;
            this.Height = h;
            this.Size = w * h;
        }

        public bool IsCellPresent(int row, int col)
        {
            long index = row * Width + col;
            return _cells.ContainsKey(index);
        }

        public T this[int row, int col]
        {
            get
            {
                long index = row * Width + col;
                T result;
                _cells.TryGetValue(index, out result);
                return result;
            }
            set
            {
                long index = row * Width + col;
                _cells[index] = value;
            }
        }
    }

    class MetricCalculator
    {
        List<Metodo> listaMetodos;
        List<Chains> listaCadenas;
        SparseMatrix<Pair> matrizPares;
        //Pair[,] matrizPares;
        List<Pair> listaPares;

        Normalizer pairNormal = new Normalizer();
        Normalizer exportNormal = new Normalizer();
        Normalizer importNormal = new Normalizer();

        public MetricCalculator(List<Metodo> list, List<Chains> cadenas)
        {
            listaMetodos = list;
            listaCadenas = cadenas;
            //matrizPares = new Pair[listaMetodos.Count, listaMetodos.Count];
            matrizPares = new SparseMatrix<Pair>(listaMetodos.Count, listaMetodos.Count);
        }
        private void setMinLoc(Metodo metodo, int value)
        {
            if (value < metodo.ExportLOCMin)
            {
                metodo.ExportLOCMin = value;
            }
        }

        private void setMinCyclo(Metodo metodo, int value)
        {
            if (value < metodo.ExportCLYCLOMin)
            {
                metodo.ExportCLYCLOMin = value;
            }
        }

        private void setMinConstant(Metodo metodo, int value)
        {
            if (value < metodo.ExportConstantMin)
            {
                metodo.ExportConstantMin = value;
            }
        }

        private void setMaxLoc(Metodo metodo, int value)
        {
            if (value > metodo.ExportLOCMax)
            {
                metodo.ExportLOCMax = value;
            }
        }

        private void setMaxCyclo(Metodo metodo, int value)
        {
            if (value > metodo.ExportCLYCLOMax)
            {
                metodo.ExportCLYCLOMax = value;
            }
        }

        private void setMaxConstant(Metodo metodo, int value)
        {
            if (value > metodo.ExportConstantMax)
            {
                metodo.ExportConstantMax = value;
            }
        }
        public void calculateAVG()
        {

            foreach (Metodo method in listaMetodos)// Cuando tenga los imports hacer un solo recorrido allá y sacar ambos
            {

                //------------------------------AVG------------------------------------
                method.ImportLOCAvg = (float)method.ImportLOCSum / (float)method.ListaEslavones.Count;
                method.ImportCLYCLOAvg = (float)method.ImportCLYCLOSum / (float)method.ListaEslavones.Count;
                method.ImportConstantAvg = (float)method.ImportConstantSum / (float)method.ListaEslavones.Count;
                //--------------------------------------------------------

                //------------------------------AVG------------------------------------
                method.ExportLOCAvg = (float)method.ExportLOCSum / (float)method.ListaEslavones.Count;
                method.ExportCLYCLOAvg = (float)method.ExportCLYCLOSum / (float)method.ListaEslavones.Count;
                method.ExportConstantAvg = (float)method.ExportConstantSum / (float)method.ListaEslavones.Count;
                //--------------------------------------------------------

                // save Min and Max values for later normalization steps

                // Import - Sum - Min
                ImportNormal.LocSumMin = Math.Min(ImportNormal.LocSumMin, method.ImportLOCSum);
                ImportNormal.CycloSumMin = Math.Min(ImportNormal.CycloSumMin, method.ImportCLYCLOSum);
                ImportNormal.ConstantSumMin = Math.Min(ImportNormal.ConstantSumMin, method.ImportConstantSum);

                // Import - Sum - Max
                ImportNormal.LocSumMax = Math.Max(ImportNormal.LocSumMax, method.ImportLOCSum);
                ImportNormal.CycloSumMax = Math.Max(ImportNormal.CycloSumMax, method.ImportCLYCLOSum);
                ImportNormal.ConstantSumMax = Math.Max(ImportNormal.ConstantSumMax, method.ImportConstantSum);

                // Import - Avg - Min
                ImportNormal.LocAvgMin = Math.Min(ImportNormal.LocAvgMin, method.ImportLOCAvg);
                ImportNormal.CycloAvgMin = Math.Min(ImportNormal.CycloAvgMin, method.ImportCLYCLOAvg);
                ImportNormal.ConstantAvgMin = Math.Min(ImportNormal.ConstantAvgMin, method.ImportConstantAvg);

                // Import - Avg - Max
                ImportNormal.LocAvgMax = Math.Max(ImportNormal.LocAvgMax, method.ImportLOCAvg);
                ImportNormal.CycloAvgMax = Math.Max(ImportNormal.CycloAvgMax, method.ImportCLYCLOAvg);
                ImportNormal.ConstantAvgMax = Math.Max(ImportNormal.ConstantAvgMax, method.ImportConstantAvg);

                // Import - Min - Min
                ImportNormal.LocMinMin = Math.Min(ImportNormal.LocMinMin, method.ImportLOCMin);
                ImportNormal.CycloMinMin = Math.Min(ImportNormal.CycloMinMin, method.ImportCLYCLOMin);
                ImportNormal.ConstantMinMin = Math.Min(ImportNormal.ConstantMinMin, method.ImportConstantMin);

                // Import - Min - Max
                ImportNormal.LocMinMax = Math.Max(ImportNormal.LocMinMax, method.ImportLOCMin);
                ImportNormal.CycloMinMax = Math.Max(ImportNormal.CycloMinMax, method.ImportCLYCLOMin);
                ImportNormal.ConstantMinMax = Math.Max(ImportNormal.ConstantMinMax, method.ImportConstantMin);

                // Import - Max - Min
                ImportNormal.LocMaxMin = Math.Min(ImportNormal.LocMaxMin, method.ImportLOCMax);
                ImportNormal.CycloMaxMin = Math.Min(ImportNormal.CycloMaxMin, method.ImportCLYCLOMax);
                ImportNormal.ConstantMaxMin = Math.Min(ImportNormal.ConstantMaxMin, method.ImportConstantMax);

                // Import - Max - Max
                ImportNormal.LocMaxMax = Math.Max(ImportNormal.LocMaxMax, method.ImportLOCMax);
                ImportNormal.CycloMaxMax = Math.Max(ImportNormal.CycloMaxMax, method.ImportCLYCLOMax);
                ImportNormal.ConstantMaxMax = Math.Max(ImportNormal.ConstantMaxMax, method.ImportConstantMax);

                // Export - Sum - Min
                ExportNormal.LocSumMin = Math.Min(ExportNormal.LocSumMin, method.ExportLOCSum);
                ExportNormal.CycloSumMin = Math.Min(ExportNormal.CycloSumMin, method.ExportCLYCLOSum);
                ExportNormal.ConstantSumMin = Math.Min(ExportNormal.ConstantSumMin, method.ExportConstantSum);

                // Export - Sum - Max
                ExportNormal.LocSumMax = Math.Max(ExportNormal.LocSumMax, method.ExportLOCSum);
                ExportNormal.CycloSumMax = Math.Max(ExportNormal.CycloSumMax, method.ExportCLYCLOSum);
                ExportNormal.ConstantSumMax = Math.Max(ExportNormal.ConstantSumMax, method.ExportConstantSum);

                // Export - Avg - Min
                ExportNormal.LocAvgMin = Math.Min(ExportNormal.LocAvgMin, method.ExportLOCAvg);
                ExportNormal.CycloAvgMin = Math.Min(ExportNormal.CycloAvgMin, method.ExportCLYCLOAvg);
                ExportNormal.ConstantAvgMin = Math.Min(ExportNormal.ConstantAvgMin, method.ExportConstantAvg);

                // Export - Avg - Max
                ExportNormal.LocAvgMax = Math.Max(ExportNormal.LocAvgMax, method.ExportLOCAvg);
                ExportNormal.CycloAvgMax = Math.Max(ExportNormal.CycloAvgMax, method.ExportCLYCLOAvg);
                ExportNormal.ConstantAvgMax = Math.Max(ExportNormal.ConstantAvgMax, method.ExportConstantAvg);

                // Export - Min - Min
                ExportNormal.LocMinMin = Math.Min(ExportNormal.LocMinMin, method.ExportLOCMin);
                ExportNormal.CycloMinMin = Math.Min(ExportNormal.CycloMinMin, method.ExportCLYCLOMin);
                ExportNormal.ConstantMinMin = Math.Min(ExportNormal.ConstantMinMin, method.ExportConstantMin);

                // Export - Min - Max
                ExportNormal.LocMinMax = Math.Max(ExportNormal.LocMinMax, method.ExportLOCMin);
                ExportNormal.CycloMinMax = Math.Max(ExportNormal.CycloMinMax, method.ExportCLYCLOMin);
                ExportNormal.ConstantMinMax = Math.Max(ExportNormal.ConstantMinMax, method.ExportConstantMin);

                // Export - Max - Min
                ExportNormal.LocMaxMin = Math.Min(ExportNormal.LocMaxMin, method.ExportLOCMax);
                ExportNormal.CycloMaxMin = Math.Min(ExportNormal.CycloMaxMin, method.ExportCLYCLOMax);
                ExportNormal.ConstantMaxMin = Math.Min(ExportNormal.ConstantMaxMin, method.ExportConstantMax);

                // Export - Max - Max
                ExportNormal.LocMaxMax = Math.Max(ExportNormal.LocMaxMax, method.ExportLOCMax);
                ExportNormal.CycloMaxMax = Math.Max(ExportNormal.CycloMaxMax, method.ExportCLYCLOMax);
                ExportNormal.ConstantMaxMax = Math.Max(ExportNormal.ConstantMaxMax, method.ExportConstantMax);
            }
        }

        public void importCalculator()
        {
            foreach (Chains chain in listaCadenas)
            {
                int acumImportLOC = 0;
                int acumImportCYCLO = 0;
                int acumImportConstant = 0;
                for (int i = (chain.CadenaDeMetodos.Count - 1); i >= 0; i--)//Comenzar en el método de más a la derecha
                {

                    Metodo method = chain.CadenaDeMetodos[i];


                    acumImportLOC += method.CantidadLineasMetodo;
                    acumImportCYCLO += method.ComplejidadCiclomatica;
                    acumImportConstant += method.ConstanteK;


                    Eslavon eslavon = method.ListaEslavones.Find(x => x.Cadena == chain);
                    eslavon.LocImport = acumImportLOC;
                    eslavon.CycloImport = acumImportCYCLO;
                    eslavon.ConstantImport = acumImportConstant;

                    //------------------------SUM-----------------------------------
                    method.ImportLOCSum += acumImportLOC;
                    method.ImportCLYCLOSum += acumImportCYCLO;
                    method.ImportConstantSum += acumImportConstant;
                    //-----------------------MIN y MAX-------------------------------------
                    if (!method.BanderaMinimoMaximoImport)
                    {
                        method.ImportLOCMin = acumImportLOC;
                        method.ImportCLYCLOMin = acumImportCYCLO;
                        method.ImportConstantMin = acumImportConstant;

                        method.ImportLOCMax = acumImportLOC;
                        method.ImportCLYCLOMax = acumImportCYCLO;
                        method.ImportConstantMax = acumImportConstant;

                        method.BanderaMinimoMaximoImport = true;
                    }
                    else
                    {
                        setMinLocImport(method, acumImportLOC);
                        setMinCycloImport(method, acumImportCYCLO);
                        setMinConstantImport(method, acumImportConstant);

                        setMaxLocImport(method, acumImportLOC);
                        setMaxCycloImport(method, acumImportCYCLO);
                        setMaxConstantImport(method, acumImportConstant);
                    }
                }

            }

        }

        public void exportCalculator()
        {
            foreach (Chains chain in listaCadenas)
            {
                int acumExportLOC = 0;
                int acumExportCYCLO = 0;
                int acumExportConstant = 0;
                for (int i = 0; i < chain.CadenaDeMetodos.Count; i++)//Comenzar en el método de más a la izquierda
                {

                    Metodo method = chain.CadenaDeMetodos[i];


                    acumExportLOC += method.CantidadLineasMetodo;
                    acumExportCYCLO += method.ComplejidadCiclomatica;
                    acumExportConstant += method.ConstanteK;
                    //Eslabon de cadena 
                    Eslavon eslavon = method.ListaEslavones.Find(x => x.Cadena == chain);
                    eslavon.LocExport = acumExportLOC;
                    eslavon.CycloExport = acumExportCYCLO;
                    eslavon.ConstantExport = acumExportConstant;

                    //------------------------SUM-----------------------------------
                    method.ExportLOCSum += acumExportLOC;
                    method.ExportCLYCLOSum += acumExportCYCLO;
                    method.ExportConstantSum += acumExportConstant;
                    //-----------------------MIN y MAX-------------------------------------
                    if (!method.BanderaMinimoMaximoExport)
                    {
                        method.ExportLOCMin = acumExportLOC;
                        method.ExportCLYCLOMin = acumExportCYCLO;
                        method.ExportConstantMin = acumExportConstant;

                        method.ExportLOCMax = acumExportLOC;
                        method.ExportCLYCLOMax = acumExportCYCLO;
                        method.ExportConstantMax = acumExportConstant;

                        method.BanderaMinimoMaximoExport = true;
                    }
                    else
                    {
                        setMinLoc(method, acumExportLOC);
                        setMinCyclo(method, acumExportCYCLO);
                        setMinConstant(method, acumExportConstant);

                        setMaxLoc(method, acumExportLOC);
                        setMaxCyclo(method, acumExportCYCLO);
                        setMaxConstant(method, acumExportConstant);
                    }
                }
                chain.PesoLOC = acumExportLOC;
                chain.PesoCYCLO = acumExportCYCLO;
                chain.PesoConstant = acumExportConstant;


            }
        }
        private void setMinLocImport(Metodo metodo, int value)
        {
            if (value < metodo.ImportLOCMin)
            {
                metodo.ImportLOCMin = value;
            }
        }

        private void setMinCycloImport(Metodo metodo, int value)
        {
            if (value < metodo.ImportCLYCLOMin)
            {
                metodo.ImportCLYCLOMin = value;
            }
        }

        private void setMinConstantImport(Metodo metodo, int value)
        {
            if (value < metodo.ImportConstantMin)
            {
                metodo.ImportConstantMin = value;
            }
        }

        private void setMaxLocImport(Metodo metodo, int value)
        {
            if (value > metodo.ImportLOCMax)
            {
                metodo.ImportLOCMax = value;
            }
        }

        private void setMaxCycloImport(Metodo metodo, int value)
        {
            if (value > metodo.ImportCLYCLOMax)
            {
                metodo.ImportCLYCLOMax = value;
            }
        }

        private void setMaxConstantImport(Metodo metodo, int value)
        {
            if (value > metodo.ImportConstantMax)
            {
                metodo.ImportConstantMax = value;
            }
        }
        private List<Eslavon> getMenorEslavon(List<Eslavon> num, List<Eslavon> num2)
        {
            if (num.Count < num2.Count)
            {
                return num;
            }

            return num2;
        }
        private List<Eslavon> getMayorEslavon(List<Eslavon> num, List<Eslavon> num2)
        {
            if (num.Count > num2.Count)
            {
                return num;
            }

            return num2;
        }
        public void calculatePairs()
        {
            for (int i = 0; i < listaMetodos.Count; i++)
            {
                Metodo methodI = ListaMetodos[i];
                for (int j = 0; j < listaMetodos.Count; j++)
                {
                    Metodo methodJ = ListaMetodos[j];
                    //Console.WriteLine("Metodo I: " + methodI.Nombre + " Metodo J: "+ methodJ.Nombre);
                    if (j != i)
                    {

                        int contadorPares = 0;
                        Pair pares = new Pair();
                        pares.NombreI = methodI.Clase + "." + methodI.Nombre;
                        pares.NombreD = methodJ.Clase + "." + methodJ.Nombre;
                        for (int k = 0; k < methodI.ListaEslavones.Count; k++)
                        {
                            if (methodI.Nombre.Equals("metodoA1") && methodJ.Nombre.Equals("metodoE1"))
                            {
                                // stop here
                                int dummy = 0;
                            }
                            int id = methodI.ListaEslavones[k].ID1;
                            Eslavon eslavonIzquierda = methodI.ListaEslavones[k];
                            Eslavon eslavonDerecha = methodJ.ListaEslavones.Find(x => x.ID1 == id);// Realiza busquedas dentro de la estructura para encontrar una coincidencia

                            if (eslavonDerecha != null && eslavonIzquierda.Indice < eslavonDerecha.Indice)
                            {
                                int resultConstant = eslavonIzquierda.Cadena.PesoConstant - eslavonIzquierda.ConstantExport - eslavonDerecha.ConstantImport + methodI.ConstanteK + methodJ.ConstanteK;
                                int resultCyclo = eslavonIzquierda.Cadena.PesoCYCLO - eslavonIzquierda.CycloExport - eslavonDerecha.CycloImport + methodI.ComplejidadCiclomatica + methodJ.ComplejidadCiclomatica;
                                int resultLoc = eslavonIzquierda.Cadena.PesoLOC - eslavonIzquierda.LocExport - eslavonDerecha.LocImport + methodI.CantidadLineasMetodo + methodJ.CantidadLineasMetodo;

                                //-------------------------------------------- SUM ------------------------------------------
                                pares.ConstantSum += resultConstant;
                                pares.CycloSum += resultCyclo;
                                pares.LocSum += resultLoc;

                                //---------------------------------------------MIN Y MAX--------------------------------------
                                if (!pares.BanderaMinimoMaximo)
                                {
                                    pares.ConstantMin = resultConstant;
                                    pares.CycloMin = resultCyclo;
                                    pares.LocMin = resultLoc;
                                    pares.ConstantMax = resultConstant;
                                    pares.CycloMax = resultCyclo;
                                    pares.LocMax = resultLoc;
                                    pares.BanderaMinimoMaximo = true;
                                }
                                else
                                {
                                    //-----------Min---------------------
                                    if (resultConstant < pares.ConstantMin) pares.ConstantMin = resultConstant;
                                    if (resultCyclo < pares.CycloMin) pares.CycloMin = resultCyclo;
                                    if (resultLoc < pares.LocMin) pares.LocMin = resultLoc;
                                    //-----------Max---------------------
                                    if (resultConstant > pares.ConstantMax) pares.ConstantMax = resultConstant;
                                    if (resultCyclo > pares.CycloMax) pares.CycloMax = resultCyclo;
                                    if (resultLoc > pares.LocMax) pares.LocMax = resultLoc;
                                }

                                contadorPares++;


                            }

                        }
                        //Calcular promedio
                        if (contadorPares != 0)
                        {
                            pares.CycloAvg = (float)pares.CycloSum / (float)contadorPares;
                            pares.LocAvg = (float)pares.LocSum / (float)contadorPares;
                            pares.ConstantAvg = (float)pares.ConstantSum / (float)contadorPares;
                            matrizPares[i, j] = pares;
                        }
                    }
                }
            }
        }


        public List<Pair> obtainMaxPairs()
        {
            ListaPares = new List<Pair>();

            // collect every method pairs
            for (int i = 0; i < listaMetodos.Count; i++)
            {
                for (int j = 0; j < listaMetodos.Count; j++)
                {
                    //if (i != j && matrizPares[i, j] != null)
                    if (i != j && matrizPares.IsCellPresent(i, j))
                    {
                        Pair p = matrizPares[i, j];
                        ListaPares.Add(p);

                        // save Min and Max for each combination for later normalization steps
                        PairNormal.LocSumMin = Math.Min(PairNormal.LocSumMin, p.LocSum);
                        PairNormal.LocAvgMin = Math.Min(PairNormal.LocAvgMin, p.LocAvg);
                        PairNormal.LocMinMin = Math.Min(PairNormal.LocMinMin, p.LocMin);
                        PairNormal.LocMaxMin = Math.Min(PairNormal.LocMaxMin, p.LocMax);

                        PairNormal.CycloSumMin = Math.Min(PairNormal.CycloSumMin, p.CycloSum);
                        PairNormal.CycloAvgMin = Math.Min(PairNormal.CycloAvgMin, p.CycloAvg);
                        PairNormal.CycloMinMin = Math.Min(PairNormal.CycloMinMin, p.CycloMin);
                        PairNormal.CycloMaxMin = Math.Min(PairNormal.CycloMaxMin, p.CycloMax);

                        PairNormal.ConstantSumMin = Math.Min(PairNormal.ConstantSumMin, p.ConstantSum);
                        PairNormal.ConstantAvgMin = Math.Min(PairNormal.ConstantAvgMin, p.ConstantAvg);
                        PairNormal.ConstantMinMin = Math.Min(PairNormal.ConstantMinMin, p.ConstantMin);
                        PairNormal.ConstantMaxMin = Math.Min(PairNormal.ConstantMaxMin, p.ConstantMax);

                        PairNormal.LocSumMax = Math.Max(PairNormal.LocSumMax, p.LocSum);
                        PairNormal.LocAvgMax = Math.Max(PairNormal.LocAvgMax, p.LocAvg);
                        PairNormal.LocMinMax = Math.Max(PairNormal.LocMinMax, p.LocMin);
                        PairNormal.LocMaxMax = Math.Max(PairNormal.LocMaxMax, p.LocMax);

                        PairNormal.CycloSumMax = Math.Max(PairNormal.CycloSumMax, p.CycloSum);
                        PairNormal.CycloAvgMax = Math.Max(PairNormal.CycloAvgMax, p.CycloAvg);
                        PairNormal.CycloMinMax = Math.Max(PairNormal.CycloMinMax, p.CycloMin);
                        PairNormal.CycloMaxMax = Math.Max(PairNormal.CycloMaxMax, p.CycloMax);

                        PairNormal.ConstantSumMax = Math.Max(PairNormal.ConstantSumMax, p.ConstantSum);
                        PairNormal.ConstantAvgMax = Math.Max(PairNormal.ConstantAvgMax, p.ConstantAvg);
                        PairNormal.ConstantMinMax = Math.Max(PairNormal.ConstantMinMax, p.ConstantMin);
                        PairNormal.ConstantMaxMax = Math.Max(PairNormal.ConstantMaxMax, p.ConstantMax);

                    }
                }
            }

            // sort method pairs in descending order by their Total Cyclomatic Complexity Indirect Coupling Strength
            ListaPares.Sort(delegate (Pair m_i, Pair m_j)
            {
                return m_j.CycloMax.CompareTo(m_i.CycloMax);
            });

            return ListaPares;
        }


        public int calculateImportNLs(Metodo metodo)
        {
            int sum = 0;
            bool[] palomar = new bool[listaMetodos.Count];
            foreach (Eslavon eslavon in metodo.ListaEslavones)
            {
                Chains cadenaAux = eslavon.Cadena;
                int indice = listaMetodos.FindIndex(x => (x.Clase == cadenaAux.CadenaDeMetodos[0].Clase && x.Nombre == cadenaAux.CadenaDeMetodos[0].Nombre));
                if (!palomar[indice] && metodo != cadenaAux.CadenaDeMetodos[0])
                {
                    palomar[indice] = true;
                    sum++;
                }

            }
            return sum;
        }
        public int calculateImportNCns(Metodo metodo)
        {
            return metodo.ListaEslavones.Count;
        }
        internal List<Metodo> ListaMetodos
        {
            get
            {
                return listaMetodos;
            }

            set
            {
                listaMetodos = value;
            }
        }

        internal Normalizer PairNormal
        {
            get
            {
                return pairNormal;
            }

            set
            {
                pairNormal = value;
            }
        }

        internal Normalizer ExportNormal
        {
            get
            {
                return exportNormal;
            }

            set
            {
                exportNormal = value;
            }
        }

        internal Normalizer ImportNormal
        {
            get
            {
                return importNormal;
            }

            set
            {
                importNormal = value;
            }
        }

        internal List<Pair> ListaPares { get => listaPares; set => listaPares = value; }
    }
}