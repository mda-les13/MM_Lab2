// Класс для генерации биномиального распределения
public class BinomialGenerator
{
    private int m;
    private double p;
    private MultiplicativeCongruentialGenerator rng;

    public BinomialGenerator(int m, double p, MultiplicativeCongruentialGenerator rng)
    {
        this.m = m;
        this.p = p;
        this.rng = rng;
    }

    public int Next()
    {
        int successes = 0;
        for (int i = 0; i < m; i++)
        {
            if (rng.Next() < p)
                successes++;
        }
        return successes;
    }
}

// Класс для генерации отрицательного биномиального распределения
public class NegativeBinomialGenerator
{
    private int r;
    private double p;
    private MultiplicativeCongruentialGenerator rng;

    public NegativeBinomialGenerator(int r, double p, MultiplicativeCongruentialGenerator rng)
    {
        this.r = r;
        this.p = p;
        this.rng = rng;
    }

    public int Next()
    {
        int trials = 0;
        int successes = 0;
        while (successes < r)
        {
            if (rng.Next() < p)
                successes++;
            trials++;
        }
        return trials;
    }
}

class Lab2Program
{
    // Вычисление биномиального коэффициента
    private static double BinomialCoefficient(int n, int k)
    {
        if (k > n) return 0;
        if (k == 0 || k == n) return 1;
        double result = 1;
        for (int i = 1; i <= k; i++)
        {
            result *= n - (k - i);
            result /= i;
        }
        return result;
    }

    // Вероятность для биномиального распределения
    private static double BinomialProbability(int k, int m, double p)
    {
        if (k < 0 || k > m) return 0;
        double coeff = BinomialCoefficient(m, k);
        return coeff * Math.Pow(p, k) * Math.Pow(1 - p, m - k);
    }

    // Вероятность для отрицательного биномиального распределения
    private static double NegativeBinomialProbability(int k, int r, double p)
    {
        if (k < r) return 0;
        double coeff = BinomialCoefficient(k - 1, r - 1);
        return coeff * Math.Pow(p, r) * Math.Pow(1 - p, k - r);
    }

    // Вычисление статистики χ² для дискретного распределения
    private static (double chiSquared, double chiSquaredCritical, int degreesOfFreedom)
        ChiSquaredTest(List<int> data, int n, double alpha,
        Func<int, double> probabilityFunc, int minExpectedCount = 5)
    {
        // Подсчет наблюдаемых частот
        var observed = new Dictionary<int, int>();
        foreach (var value in data)
        {
            if (observed.ContainsKey(value))
                observed[value]++;
            else
                observed[value] = 1;
        }

        // Получаем все уникальные значения
        var values = observed.Keys.OrderBy(x => x).ToList();

        // Группируем значения с малыми ожидаемыми частотами
        var groupedValues = new List<List<int>>();
        var currentGroup = new List<int>();
        double currentExpected = 0;

        foreach (var value in values)
        {
            double expected = n * probabilityFunc(value);
            currentExpected += expected;

            currentGroup.Add(value);

            if (currentExpected >= minExpectedCount || value == values.Last())
            {
                groupedValues.Add(new List<int>(currentGroup));
                currentGroup.Clear();
                currentExpected = 0;
            }
        }

        // Если последняя группа слишком мала, присоединяем к предыдущей
        if (currentGroup.Count > 0 && groupedValues.Count > 0)
        {
            groupedValues[groupedValues.Count - 1].AddRange(currentGroup);
        }

        // Вычисляем наблюдаемые и ожидаемые частоты для групп
        var observedFrequencies = new List<int>();
        var expectedFrequencies = new List<double>();

        foreach (var group in groupedValues)
        {
            int obsCount = group.Sum(value => observed.GetValueOrDefault(value, 0));
            double expCount = group.Sum(value => n * probabilityFunc(value));

            observedFrequencies.Add(obsCount);
            expectedFrequencies.Add(expCount);
        }

        // Удаляем группы с нулевыми наблюдаемыми частотами
        for (int i = observedFrequencies.Count - 1; i >= 0; i--)
        {
            if (observedFrequencies[i] == 0 && expectedFrequencies[i] == 0)
            {
                observedFrequencies.RemoveAt(i);
                expectedFrequencies.RemoveAt(i);
            }
        }

        // Вычисление статистики χ²
        double chiSquared = 0.0;
        for (int i = 0; i < observedFrequencies.Count; i++)
        {
            double diff = observedFrequencies[i] - expectedFrequencies[i];
            chiSquared += (diff * diff) / expectedFrequencies[i];
        }

        // Степени свободы = количество групп - 1 - количество оцененных параметров
        // Здесь параметры известны, поэтому -1 (только проверка распределения)
        int degreesOfFreedom = observedFrequencies.Count - 1;

        // Критическое значение
        double chiSquaredCritical = GetChiSquaredCriticalValue(degreesOfFreedom, alpha);

        return (chiSquared, chiSquaredCritical, degreesOfFreedom);
    }

    // Упрощенное получение критического значения χ2
    private static double GetChiSquaredCriticalValue(int df, double alpha)
    {
        var chiSquaredTable = new Dictionary<int, double>
        {
            { 1, 3.841 },
            { 2, 5.991 },
            { 3, 7.815 },
            { 4, 9.488 },
            { 5, 11.070 },
            { 6, 12.592 },
            { 7, 14.067 },
            { 8, 15.507 },
            { 9, 16.919 },
            { 10, 18.307 }
        };

        if (df <= 10 && chiSquaredTable.ContainsKey(df))
        {
            return chiSquaredTable[df];
        }
        return 0;
    }

    static void Main(string[] args)
    {
        Console.WriteLine("Лабораторная работа №2. Моделирование дискретных СВ. Вариант 5");
        Console.WriteLine("Биномиальное распределение: Bi(m=6, p=0.3333333);");
        Console.WriteLine("Отрицательное биномиальное распределение: Bi(r=4, p=0.2)");
        Console.WriteLine(new string('-', 60));

        // Параметры
        int n = 1000; // Количество реализаций
        double alpha = 0.05; // Уровень значимости

        // Инициализация генератора случайных чисел (из лабораторной работы №1)
        long M = (long)Math.Pow(2, 31); // 2^31
        long a = 16807; // Стандартный множитель
        long seed = 12345; // Начальное значение
        var rng = new MultiplicativeCongruentialGenerator(a, M, seed);

        // 1. Биномиальное распределение Bi(m=6, p=0.3333333)
        Console.WriteLine("1) Биномиальное распределение Bi(m=6, p=0.3333333):");

        var binomialGenerator = new BinomialGenerator(6, 0.3333333, rng);
        var binomialData = new List<int>();

        for (int i = 0; i < n; i++)
        {
            binomialData.Add(binomialGenerator.Next());
        }

        // Теоретические характеристики
        double binomialMean = 6 * 0.3333333;
        double binomialVariance = 6 * 0.3333333 * (1 - 0.3333333);

        // Выборочные характеристики
        double sampleBinomialMean = binomialData.Average();
        double sampleBinomialVariance = binomialData.Select(x => Math.Pow(x - sampleBinomialMean, 2)).Sum() / (n - 1);

        Console.WriteLine($"   Теоретическое среднее: {binomialMean:F4}");
        Console.WriteLine($"   Выборочное среднее: {sampleBinomialMean:F4}");
        Console.WriteLine($"   Отклонение: {Math.Abs(sampleBinomialMean - binomialMean):F4}");

        Console.WriteLine($"   Теоретическая дисперсия: {binomialVariance:F4}");
        Console.WriteLine($"   Выборочная дисперсия: {sampleBinomialVariance:F4}");
        Console.WriteLine($"   Отклонение: {Math.Abs(sampleBinomialVariance - binomialVariance):F4}");

        // Проверка по χ²-критерию
        var (chiSquaredBinomial, chiSquaredCriticalBinomial, dfBinomial) =
            ChiSquaredTest(binomialData, n, alpha, k => BinomialProbability(k, 6, 0.3333333));

        Console.WriteLine($"\n   χ²-тест:");
        Console.WriteLine($"   χ² = {chiSquaredBinomial:F4}, df = {dfBinomial}, χ²_критическое = {chiSquaredCriticalBinomial:F4}");
        if (chiSquaredBinomial < chiSquaredCriticalBinomial)
            Console.WriteLine("   Гипотеза о соответствии распределения ПРИНЯТА.");
        else
            Console.WriteLine("   Гипотеза о соответствии распределения ОТВЕРГНУТА.");
        Console.WriteLine();

        // 2. Отрицательное биномиальное распределение Bi(r=4, p=0.2)
        Console.WriteLine("2) Отрицательное биномиальное распределение Bi(r=4, p=0.2):");

        var negativeBinomialGenerator = new NegativeBinomialGenerator(4, 0.2, rng);
        var negativeBinomialData = new List<int>();

        for (int i = 0; i < n; i++)
        {
            negativeBinomialData.Add(negativeBinomialGenerator.Next());
        }

        // Теоретические характеристики
        double negativeBinomialMean = 4 / 0.2;
        double negativeBinomialVariance = 4 * (1 - 0.2) / Math.Pow(0.2, 2);

        // Выборочные характеристики
        double sampleNegativeBinomialMean = negativeBinomialData.Average();
        double sampleNegativeBinomialVariance = negativeBinomialData.Select(x => Math.Pow(x - sampleNegativeBinomialMean, 2)).Sum() / (n - 1);

        Console.WriteLine($"   Теоретическое среднее: {negativeBinomialMean:F4}");
        Console.WriteLine($"   Выборочное среднее: {sampleNegativeBinomialMean:F4}");
        Console.WriteLine($"   Отклонение: {Math.Abs(sampleNegativeBinomialMean - negativeBinomialMean):F4}");

        Console.WriteLine($"   Теоретическая дисперсия: {negativeBinomialVariance:F4}");
        Console.WriteLine($"   Выборочная дисперсия: {sampleNegativeBinomialVariance:F4}");
        Console.WriteLine($"   Отклонение: {Math.Abs(sampleNegativeBinomialVariance - negativeBinomialVariance):F4}");

        // Проверка по χ²-критерию
        var (chiSquaredNegativeBinomial, chiSquaredCriticalNegativeBinomial, dfNegativeBinomial) =
            ChiSquaredTest(negativeBinomialData, n, alpha, k => NegativeBinomialProbability(k, 4, 0.2));

        Console.WriteLine($"\n   χ²-тест:");
        Console.WriteLine($"   χ² = {chiSquaredNegativeBinomial:F4}, df = {dfNegativeBinomial}, χ²_критическое = {chiSquaredCriticalNegativeBinomial:F4}");
        if (chiSquaredNegativeBinomial < chiSquaredCriticalNegativeBinomial)
            Console.WriteLine("   Гипотеза о соответствии распределения ПРИНЯТА.");
        else
            Console.WriteLine("   Гипотеза о соответствии распределения ОТВЕРГНУТА.");

        Console.WriteLine("\nНажмите любую клавишу для выхода...");
        Console.ReadKey();
    }
}
