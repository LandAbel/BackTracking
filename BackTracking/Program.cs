using System;
using System.ComponentModel;
using System.IO;

namespace BackTracking
{
    class Program
    {
        private static void LevelDisplay(int level, object[] E)
        {
            Console.WriteLine($"Aktuális szint:{level}");
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Adjon meg egy fájlnevet: ");
            string input = Console.ReadLine();
            SudokuMegoldo solver = new SudokuMegoldo(input);
            solver.Megjelenites();
            solver.Probalkozas += LevelDisplay;
            System.Threading.Thread.Sleep(600);
            solver.Megoldas();
            Console.WriteLine("\r\n Megoldás:");
            solver.Megjelenites();
        }
    }
    class NincsMegoldasKivetel:Exception
    {
        public NincsMegoldasKivetel(string message) : base(message)
        {
        }
    }
    public delegate void AllapotFigyelo(int dn, object[] yet);
    abstract class Backtrack
    {
        protected int N;
        protected int[] M;
        protected object[,] R;
        public event AllapotFigyelo Probalkozas;
        abstract public bool ft(int m,object r);
        abstract public bool fk(int m, object Elso, int n, object Masodik);
        public Object[] Kereses()
        {
            bool van = false;
            object[] E = new object[N];
            Probal(0, ref van, E);
            if (van)
            {
                return E;
            }
            else
            {
                throw new NincsMegoldasKivetel("Nincs megoldása a feladatnak");
            }

        }
        public void Probal(int level,ref bool van, object[] E)
        {
            int idx = 0;
            while (van==false && idx<M[level])
            {
                if (ft(level, R[level,idx]))
                {
                    bool exist = true;
                    int k = 0;
                    while (k<level && exist==true)
                    {
                        if (fk(level, R[level, idx],k, E[k])==false )
                        {
                            exist = false;
                        }
                        k++;
                    }
                    if (exist)
                    {
                        E[level] = R[level, idx];
                        Probalkozas?.Invoke(level,E);
                        if (level==N-1)
                        {
                            van = true;
                        }
                        else
                        {
                            Probal(level + 1, ref van, E);
                        }

                    }
                }
                idx++;
            }
        }

    }
    class Pozicio
    {
        public int Sor { get;}
        public int Oszlop { get;}
        public bool Fix { get;}
        public object Ertek { get; set; }

        public Pozicio(int sor, int oszlop)
        {
            Sor = sor;
            Oszlop = oszlop;
            Fix = false;
        }

        public Pozicio(int sor, int oszlop, object ertek)
        {
            this.Sor = sor;
            this.Oszlop = oszlop;
            this.Ertek = ertek;
            Fix = true;
        }
        public static bool Kizaroak(Pozicio A, Pozicio B)
        {
            if (A.Sor==B.Sor || A.Oszlop==B.Oszlop || (A.Sor/3== B.Sor/3 && A.Oszlop/3 == B.Oszlop/3))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    class SudokuMegoldo:Backtrack
    {
        Pozicio[,] tabla = new Pozicio[9, 9];
        Pozicio[] fixMezok;
        Pozicio[] uresMezok;
        int emptysq = 0;
        int fixsq = 0;

        public SudokuMegoldo(string Fname)
        {
            TablaBetoltes(Fname);
        }
        public override bool ft(int m, object r)
        {
            uresMezok[m].Ertek = (int)r;
            int idx = 0;
            while (idx < fixMezok.Length)
            {
                if (Pozicio.Kizaroak(fixMezok[idx], uresMezok[m]) && (int)uresMezok[m].Ertek == (int)fixMezok[idx].Ertek)
                {
                    return false;
                }
                idx++;
                
            }
            return false;
        }
        public override bool fk(int m, object Elso, int n, object Masodik)
        {
            uresMezok[m].Ertek = (int)Elso;
            fixMezok[n].Ertek = (int)Masodik;
            if (Pozicio.Kizaroak(uresMezok[m], uresMezok[n]) && (int)uresMezok[m].Ertek == (int)uresMezok[n].Ertek)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void TablaBetoltes(string fajlnev)
        {
            StreamReader input = new StreamReader(fajlnev);
            string ideiglenes = "";
            int line = 0;
            while (input.EndOfStream==false)
            {
                ideiglenes = input.ReadLine();
                for (int i = 0; i < ideiglenes.Length; i++)
                {
                    if (ideiglenes[i]=='.')
                    {
                        Pozicio position = new Pozicio(line, i);
                        tabla[line, i] = position;
                        emptysq++;
                    }
                    else
                    {
                        Pozicio position = new Pozicio(line, i, int.Parse(ideiglenes[i].ToString()));
                        tabla[line, i] = position;
                        fixsq++;
                    }
                }
                line++;
            }
            input.Close();
            NewMezo();
            N = emptysq;
            M = new int[N];
            for (int j = 0; j < M.Length; j++)
            {
                M[j] = 9;
            }

            R = new object[N,9];
            for (int k = 0; k < R.GetLength(0); k++)
            {
                for (int l = 0; l < R.GetLength(1); l++)
                {
                    R[k, l] = l + 1;
                }
            }

        }
        public void Megoldas()
        {
            try
            {
                object[] data = Kereses();
                for (int i = 0; i < uresMezok.Length; i++)
                {
                    tabla[uresMezok[i].Sor, uresMezok[i].Oszlop] = uresMezok[i];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public void Megjelenites()
        {
            for (int i = 0; i < tabla.GetLength(0); i++)
            {
                for (int k = 0; k < tabla.GetLength(1); k++)
                {
                    if (tabla[i,k].Ertek != null)
                    {
                        if (tabla[i,k].Fix)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.Write(tabla[i,k].Ertek);
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            Console.Write(tabla[i,k].Ertek);
                        }

                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkBlue;
                        Console.Write(".");
                    }
                }
                Console.Write("\r\n");
            }
        }
        private void NewMezo()
        {
            fixMezok = new Pozicio[fixsq];
            uresMezok = new Pozicio[emptysq];
            int EmptyIdx = 0;
            int FixIdx = 0;
            for (int i = 0; i < tabla.GetLength(0); i++)
            {
                for (int j = 0; j < tabla.GetLength(1); j++)
                {
                    if (tabla[i,j]!=null)
                    {
                        if (tabla[i,j].Fix)
                        {
                            fixMezok[FixIdx] = tabla[i, j];
                            FixIdx++;
                        }
                        else
                        {
                            uresMezok[EmptyIdx] = tabla[i, j];
                            EmptyIdx++;
                        }
                    }
                }
            }
        }


    }
}

