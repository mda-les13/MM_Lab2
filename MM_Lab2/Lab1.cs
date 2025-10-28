// Класс для генерации случайных чисел методом мультипликативного конгруэнтного метода (МКМ)
public class MultiplicativeCongruentialGenerator
{
    private long a; // Множитель
    private long m; // Модуль
    private long seed; // Текущее значение (посев)
    private long initialSeed; // Начальное значение (посев)

    public MultiplicativeCongruentialGenerator(long a, long m, long seed)
    {
        this.a = a;
        this.m = m;
        this.seed = seed;
        this.initialSeed = seed;
    }

    // Генерирует следующее случайное число в диапазоне [0, 1)
    public double Next()
    {
        seed = (a * seed) % m;
        return (double)seed / m;
    }

    // Сброс генератора к начальному значению
    public void Reset()
    {
        seed = initialSeed;
    }
}
