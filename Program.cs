using System;
using System.Linq;
using System.Collections.Generic;

class Ant
{
    public int[] Trail { get; private set; }
    private bool[] visited;
    private Random random;

    public Ant(int numberOfCities)
    {
        Trail = new int[numberOfCities];
        visited = new bool[numberOfCities];
        random = new Random();
    }

    public void Clear()
    {
        Array.Fill(Trail, -1);
        Array.Fill(visited, false);
    }

    public void VisitCity(int currentIndex, int city)
    {
        Trail[currentIndex + 1] = city;
        visited[city] = true;
    }

    public bool Visited(int i)
    {
        return visited[i];
    }

    public double TrailLength(double[][] graph)
    {
        double length = graph[Trail[Trail.Length - 1]][Trail[0]];
        for (int i = 0; i < Trail.Length - 1; i++)
        {
            length += graph[Trail[i]][Trail[i + 1]];
        }
        return length;
    }
}

class AntColony
{
    public int[] bestTour;
    private double[][] graph;
    private double[][] trails;
    private double[] probabilities;
    private Ant[] ants;
    private int numberOfCities;
    private int numberOfAnts;
    private double evaporation;
    private double alpha;
    private double beta;
    private double Q;
    private Random random;
    private int currentIndex;
    private int[] bestTourOrder;
    private double bestTourLength;

    public AntColony(double[][] graph, int numberOfAnts, double evaporation, double alpha, double beta, double Q, int[] bestTour = null)
    {
        this.graph = graph;
        this.numberOfCities = graph.Length;
        this.numberOfAnts = numberOfAnts;
        this.evaporation = evaporation;
        this.alpha = alpha;
        this.beta = beta;
        this.Q = Q;
        this.random = new Random();
        this.currentIndex = 0;
        this.bestTourOrder = null;
        this.bestTourLength = double.MaxValue;

        trails = new double[numberOfCities][];
        for (int i = 0; i < numberOfCities; i++)
        {
            trails[i] = new double[numberOfCities];
            for (int j = 0; j < numberOfCities; j++)
            {
                trails[i][j] = 1.0;
            }
        }

        probabilities = new double[numberOfCities];
        ants = new Ant[numberOfAnts];
        for (int i = 0; i < numberOfAnts; i++)
        {
            ants[i] = new Ant(numberOfCities);
        }

        this.bestTour = bestTour;
    }

    public int[] Solve(int maxIterations)
    {
        int[] globalBestTour = null;
        double globalBestLength = double.MaxValue;

        for (int iteration = 0; iteration < maxIterations; iteration++)
        {
            SetupAnts();
            MoveAnts();
            UpdateTrails();
            UpdateBest();

            if (bestTourLength < globalBestLength)
            {
                globalBestLength = bestTourLength;
                globalBestTour = bestTourOrder.Clone() as int[];
            }

            Console.WriteLine($"Iтерацiя {iteration + 1}: Найкраща довжина шляху: {bestTourLength}");
            Console.Write("Найкращий шлях: " + string.Join(" -> ", bestTourOrder.Select(cityIndex => cityIndex + 1)));
            Console.WriteLine($" -> {++bestTourOrder[0]}\n");
            Thread.Sleep(1000);
        }

        return globalBestTour;
    }

    private void SetupAnts()
    {
        foreach (var ant in ants)
        {
            ant.Clear();
            ant.VisitCity(-1, random.Next(numberOfCities));
        }
        currentIndex = 0;
    }

    private void MoveAnts()
    {
        for (int i = currentIndex; i < numberOfCities - 1; i++)
        {
            foreach (var ant in ants)
            {
                ant.VisitCity(currentIndex, SelectNextCity(ant));
            }
            currentIndex++;
        }
    }
    
    private int SelectNextCity(Ant ant)
    {
        int i = ant.Trail[currentIndex];
        double pheromone = 0.0;
        for (int l = 0; l < numberOfCities; l++)
        {
            if (!ant.Visited(l))
            {
                pheromone += Math.Pow(trails[i][l], alpha) * Math.Pow(1.0 / graph[i][l], beta);
            }
        }
        for (int j = 0; j < numberOfCities; j++)
        {
            if (ant.Visited(j))
            {
                probabilities[j] = 0.0;
            }
            else
            {
                double numerator = Math.Pow(trails[i][j], alpha) * Math.Pow(1.0 / graph[i][j], beta);
                probabilities[j] = numerator / pheromone;
            }
        }

        double r = random.NextDouble();
        double total = 0;
        for (int q = 0; q < numberOfCities; q++)
        {
            total += probabilities[q];
            if (total >= r)
            {
                return q;
            }
        }

        // This code should never be reached, but to avoid compiler errors
        return -1;
    }

    private void UpdateTrails()
    {
        for (int i = 0; i < numberOfCities; i++)
        {
            for (int j = 0; j < numberOfCities; j++)
            {
                trails[i][j] *= evaporation;
            }
        }
        foreach (var ant in ants)
        {
            double contribution = Q / ant.TrailLength(graph);
            for (int i = 0; i < numberOfCities - 1; i++)
            {
                trails[ant.Trail[i]][ant.Trail[i + 1]] += contribution;
            }
            trails[ant.Trail[numberOfCities - 1]][ant.Trail[0]] += contribution;
        }
    }

    private void UpdateBest()
    {
        if (bestTourOrder == null)
        {
            bestTourOrder = ants[0].Trail;
            bestTourLength = ants[0].TrailLength(graph);
        }
        foreach (var ant in ants)
        {
            if (ant.TrailLength(graph) < bestTourLength)
            {
                bestTourLength = ant.TrailLength(graph);
                bestTourOrder = ant.Trail.ToArray();
            }
        }
    }
}

class Program
{
    static void Main()
    {
        double[][] distances = {
            new double[] {0, 6, 5, 12},
            new double[] {6, 0, 15, 3},
            new double[] {5, 15, 0, 21},
            new double[] {12, 3, 21, 0}
        };

        int numAnts = 5;
        double evaporation = 0.1;
        double alpha = 1.0;
        double beta = 1.0;
        double Q = 20;
        AntColony antColony = new AntColony(distances, numAnts, evaporation, alpha, beta, Q);

        int[] bestTour = antColony.Solve(10);
        double bestTourLength = CalculateTourLength(bestTour, distances);

    }

    static double CalculateTourLength(int[] tour, double[][] distances)
    {
        double length = distances[tour[tour.Length - 1]][tour[0]];
        for (int i = 0; i < tour.Length - 1; i++)
        {
            length += distances[tour[i]][tour[i + 1]];
        }
        return length;
    }
}