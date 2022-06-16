using System;
using System.Collections.Generic;

namespace AntAntColonyFckOff {
    class Program {
        static void Main(string[] args) {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            OrientGraph inputGraph = new OrientGraph();
            string[] input;
            Console.WriteLine("enter number of cityes");
            int count = Int32.Parse(Console.ReadLine());
            int cityes = count;
            while (count-- > 0) {
                input = Console.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                for (var i = 1; i < input.Length; i++) {
                    string[] temp = input[i].Split(':', StringSplitOptions.RemoveEmptyEntries);
                    inputGraph.AddRoute(input[0], temp[0], Int32.Parse(temp[1]));
                }
            }
            Console.WriteLine(inputGraph);
            //while (true) {
                AntColony myColony = new AntColony(inputGraph.Matrix, cityes, 30, 400, 1, 2, 0.2, 0);

                int wtf;
                int[] way = myColony.TotalyProud(out wtf);
           //}
            for (var i = 0; i < way.Length; i++)
                Console.Write($"{inputGraph.GetNameDic(way[i])} ");
            Console.WriteLine($"\n {wtf}");
        }
    }
    //Пишу этот класс в десятый раз. были разные способы, разные методы. Но сейчас моя жопа просто полыхает огнём от моей же тупости.
    //Кол-во городов, муравьев, итераций, альфа,бета и степень убывания феромонов задаются конструктором, как и матрица переходов
    class AntColony {

        //ОСНОВНЫЕ КОНСТАНТЫ
        int m_numberOfCity;
        int m_numberOfAnts;
        int m_iterations;
        double alpha;
        double beta;
        double feromons;
        int m_start;
        //матрицы феромонов и перехода
        double[,] m_feromonMatrix;
        int[,] m_matrix;
        //пути муравьев на текущей итерации алгоритма
        List<int[]> m_ways;
        //лучший путь на текущей итерации алгоритма. Вы не представляете себе, как моё очко сейчас полухыает
        int[] m_bestWay;
        int lowestWeight;
       

        //инициализация всей хурмы
        public AntColony(int[,] matrix, int numberOfCity, int numberOfAnts=30, int iterations=20, double a=2, double b=2, double fer = 0.1, int startPos=0) {
            m_numberOfCity = numberOfCity;
            m_numberOfAnts = numberOfAnts;
            m_iterations = iterations;
            alpha = a;
            beta = b;
            feromons = fer;
            m_matrix = matrix;
            m_start = startPos;
            m_feromonMatrix = new double[numberOfCity, numberOfCity];
            m_bestWay = new int[numberOfCity];
            lowestWeight = int.MaxValue;
            for (var i = 0; i < numberOfCity; i++) {
                for (var j = 0; j < numberOfCity; j++)
                    m_feromonMatrix[j, i] = 0.5;
                m_bestWay[i] = -1;
            }
            m_ways = new List<int[]>();
        }

        public int[] TotalyProud(out int weightSS) {
            int iterations = m_iterations;
            while (iterations-- > 0){
                //Подсчёт идёт для каждого мураша
                for(var count = 0; count < m_numberOfAnts; count++) {
                    //Добавляем мураша в список путей. Весь путь пока что заявляем как -1, то есть из ниоткуда в никуда
                    m_ways.Add(new int[m_numberOfCity]);
                    for (var i = 0; i < m_numberOfCity; i++)
                        m_ways[count][i] = -1;
                    m_ways[count][0] = m_start;

                    //Создаём матрицу вероятностей для всех переходов на маршруте на текущую итерацию. 
                    //Пока что проверяем вариант, чтобы возможности складывались только между узлами, на которые муравей переходит
                    double[,] perhaps = new double[m_numberOfCity,m_numberOfCity];
                    for(var i =0; i<m_numberOfCity; i++)
                        for(var j = 0; j < m_numberOfCity; j++) {
                            if (m_matrix[j, i] > 0)
                                perhaps[j, i] = Math.Pow(m_feromonMatrix[j, i], alpha) * Math.Pow(1.0 / m_matrix[j, i], beta);
                        }
                    //теперь нужно найти вершины, которые мы могли бы посетить, но с условием что в m_ways их нет
                    //сложить их вероятности, разделить отдельную вероятность на общую, при помощи рандома выбрать вершину
                    //после чего муравьишки должны дойти по возможности до конечной точки своего пути.
                    int checker = 0;
                    for(var i=0; i < m_numberOfCity-1; i++) {
                        List<int> perhapsWays = new List<int>();
                        for(var j = 0; j < m_numberOfCity; j++) {
                            if (m_ways[count][i] == -1) {
                                break;
                            }
                                if (m_matrix[j, m_ways[count][i]] > 0) {
                                    for (var check = 0; check < m_numberOfCity; check++)
                                        if (m_ways[count][check] == j) {
                                            checker = 1;
                                            break;
                                        }
                                    if (checker == 1) {
                                        checker = 0;
                                        continue;
                                    } else {
                                        perhapsWays.Add(j);
                                    }
                                }
                            
                        }
                        if (perhapsWays.Count > 0) {
                           /* if (m_ways[count][1] == 4 && m_ways[count][2] == 3)
                                Console.WriteLine("bingo");*/
                            if (perhapsWays.Count == 1)
                                m_ways[count][i + 1] = perhapsWays[0];
                            else {
                                //Собрав все вероятные пути, мы складываем их вероятности и выбираем при помощи рандома вероятность, то есть путь
                                double sumPerhaps = 0;
                                double totalPerhaps = 0;
                                Random randFrom = new Random();
                                double number = (double)randFrom.Next(100);
                                foreach (var member in perhapsWays)
                                    sumPerhaps += perhaps[member, m_ways[count][i]];
                                foreach (var member in perhapsWays) {
                                    totalPerhaps += perhaps[member, m_ways[count][i]] / sumPerhaps * 100;
                                    if (totalPerhaps > number) {
                                        m_ways[count][i + 1] = member;
                                        break;
                                    }
                                }
                            }
                        } else break;
                        //т.о. мы нашли следующую точку в маршруте конкретного муравья на конкретной итерации.
                        //Теперь просто повторяем, пока путь не забьётся
                        //не стоит забывать, что муравьишкО может заблудиться и попасть в тупик, это тоже надо будет проверять при подсчёте маршрута.
                    }
                }
                //к этому моменту мы должны иметь путь для каждого муравьишки, теперь ищем среди них лучший путь.
                foreach(var ant in m_ways) {
                   /* if (ant[0] == 0 && ant[1] == 4 && ant[2] == 3)
                        Console.Write("bingo");*/
                    int weight = 0;
                    for (var i = 0; i < ant.Length - 1; i++)
                        if (ant[i+1] == -1) {
                            weight = int.MaxValue;
                            break;
                        } else {
                            weight += m_matrix[ant[i + 1], ant[i]];
                        }
                    if (weight < lowestWeight) {
                        lowestWeight = weight;
                        for (var i = 0; i < ant.Length; i++) {
                            m_bestWay[i] = ant[i];
                           // Console.Write($"{m_bestWay[i]} ");
                        }
                    }
                }
                //нашли лучший путь, зафиксировали вес и сам путь. 
                //Теперь нужно подсчитать феромон на следующую стадию
                //Можно короче, но я задолбался ошибаться, сделаю два прогона. Сначала муравьи возвращаются, тем самыми прокладывая феромон
                //Затем феромон будет стиратьсяж. Хотя нет. Сделаем наоборот. Сначала отнимаем феромон, потом добавляем дельту
                for (var i = 0; i < m_numberOfCity; i++)
                    for (var j = 0; j < m_numberOfCity; j++) {
                        m_feromonMatrix[j, i] = m_feromonMatrix[j, i] * (1.0 - feromons);
                    }
                foreach (var ant in m_ways) {
                    for (var i = 0; i < ant.Length - 1; i++) {
                        m_feromonMatrix[i + 1, i] += 1.0 / (m_matrix[i + 1, i]);
                        m_feromonMatrix[i,i+1] += 1.0 / (m_matrix[i + 1, i]);
                    }
                }


                m_ways.Clear();
            }
            weightSS = lowestWeight;
            return m_bestWay;
        }
    }


    //Класс ориентированного графа. Добавление вершин и путей при помощи метода AddRoute(string name1, string name2, int weight)
    class OrientGraph {
        int[,] m_matrix;
        
        Dictionary<string, int> m_names;
        public int[,] Matrix { set { } get { return m_matrix; } }
        public Dictionary<string, int> Names { set { } get { return m_names; } }
        public OrientGraph() {
            m_matrix = new int[0, 0];
            m_names = new Dictionary<string, int>();
        }


        //Метод для добавления новой вершины. 
        void AddFreePeak(string newPeak) {
            if (this.m_names.ContainsKey(newPeak))
                throw new Exception("something wrong, u use one name twice");
            this.m_names.Add(newPeak, m_names.Count);
            int[,] newMatrix = new int[m_names.Count, m_names.Count];
            for (var i = 0; i < m_names.Count - 1; i++)
                for (var j = 0; j < m_names.Count - 1; j++)
                    newMatrix[i, j] = this.m_matrix[i, j];
            this.m_matrix = newMatrix;
        }

        //Метод добавления маршрута
        public void AddRoute(string name1, string name2, int weight) {
            if (name1 == name2)
                return;
            if (!this.m_names.ContainsKey(name1))
                AddFreePeak(name1);
            if (!this.m_names.ContainsKey(name2))
                AddFreePeak(name2);
            if (this.m_matrix[m_names[name2], m_names[name1]] > 0)
                return;
            this.m_matrix[m_names[name2], m_names[name1]] = weight;
        }
        //выделение нулевых маршрутов
        public void ReverseZero() {
            for (var i = 0; i < m_names.Count; i++)
                for (var j = 0; j < m_names.Count; j++)
                    if (m_matrix[i, j] == 0)
                        m_matrix[i, j] = int.MinValue;

        }
        public string GetNameDic(int number) {
            foreach (var pair in m_names) {
                if (pair.Value == number)
                    return pair.Key;
            }
            return string.Empty;
        }
        //вывод графа
        public override string ToString() {
            string output = string.Empty;
            for (var i = 0; i < m_names.Count; i++)
                output += $"{i,4}";
            output += "\n";
            for (var i = 0; i < m_names.Count; i++) {
                output += $"{i}  ";
                for (var j = 0; j < m_names.Count; j++) {
                    if (m_matrix[j, i] == int.MinValue)
                        output += ($"{0,2}  ");
                    else output += $"{m_matrix[j, i],2}  ";
                }
                output += "\n";
            }
            foreach (var pair in m_names) {
                output += $"{pair.Key} is {pair.Value}\n";
            }
            return output;
        }
        static public int[,] CopyMatrix(int[,] inputmatrix) {
            int[,] returnMatrix = new int[inputmatrix.GetLength(0), inputmatrix.GetLength(1)];
            for (var i = 0; i < inputmatrix.GetLength(0); i++)
                for (var j = 0; j < inputmatrix.GetLength(1); j++)
                    returnMatrix[i, j] = inputmatrix[i, j];
            return returnMatrix;
        }
    }

}
