using System;
using System.Collections.Generic;
using UnityEngine;
using RandomSys = System.Random;
using RandomUnity = UnityEngine.Random;

namespace FrostfallSaga.Utils
{
    public static class Randomizer
    {
        private static readonly RandomSys _randomizer = new();

        public static int GetRandomIntBetween(int min, int max)
        {
            return _randomizer.Next(min, max);
        }

        public static bool GetBooleanOnChance(float chance)
        {
            return _randomizer.NextDouble() < chance;
        }

        public static T GetRandomElementFromArray<T>(T[] array)
        {
            return array[_randomizer.Next(0, array.Length)];
        }

        public static T GetRandomElementFromList<T>(List<T> list)
        {
            if (list == null || list.Count == 0)
                throw new ArgumentException("List is null or empty");

            return list[_randomizer.Next(0, list.Count)];
        }

        public static T[] GetRandomUniqueElementsFromArray<T>(T[] array, int count)
        {
            T[] result = new T[count];
            T[] copy = new T[array.Length];
            Array.Copy(array, copy, array.Length);

            for (int i = 0; i < count; i++)
            {
                int randomIndex = _randomizer.Next(0, copy.Length);
                result[i] = copy[randomIndex];
                copy[randomIndex] = copy[^1];
                Array.Resize(ref copy, copy.Length - 1);
            }

            return result;
        }

        public static float GetRandomFloatBetween(float min, float max)
        {
            return RandomUnity.Range(min, max);
        }

        public static void InitState(int state)
        {
            RandomUnity.InitState(state);
        }

        public static T GetRandomElementFromEnum<T>(T[] toExclude = null) where T : Enum
        {
            Array values = Enum.GetValues(typeof(T));
            int randomIndex = _randomizer.Next(0, values.Length);

            if (toExclude != null)
                while (Array.Exists(toExclude, element => element.Equals(values.GetValue(randomIndex))))
                    randomIndex = _randomizer.Next(0, values.Length);

            return (T)values.GetValue(randomIndex);
        }

        public static Quaternion GetRandomRotationY(Quaternion rotation)
        {
            return Quaternion.Euler(rotation.x, RandomUnity.Range(0f, 360.0f), rotation.z);
        }
    }
}