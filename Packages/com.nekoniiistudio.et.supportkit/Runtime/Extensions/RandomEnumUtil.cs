public static class RandomEnum<T>
{
    static T[] m_Values;
    static RandomEnum()
    {
        var values = System.Enum.GetValues(typeof(T));
        m_Values = new T[values.Length];
        for (int i = 0; i < m_Values.Length; i++)
            m_Values[i] = (T)values.GetValue(i);
    }
    public static T Get()
    {
        return m_Values[UnityEngine.Random.Range(0, m_Values.Length)];
    }
}
