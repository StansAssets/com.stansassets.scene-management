using System;
using System.Collections.Generic;
using System.Linq;

namespace StansAssets.SceneManagement.StackVisualizer.Utility
{
    internal static class StackVisualizerUtility
    {
        static VisualStackTemplate[] s_EmptyTemplatesCollection = new VisualStackTemplate[0];
        public static IEnumerable<VisualStackTemplate> CreateTemplatesFor<T>(IEnumerable<T> stack) where T : Enum
        {
            if (!stack.Any())
            {
                return s_EmptyTemplatesCollection;
            }

            var newStack = stack.Select(st => new VisualStackTemplate() {Title = GetTitleFromEnum(st)}).ToList();
            // Set last state as Active
            newStack.Last().Status = VisualStackItemStatus.Active;
            // Reverse to display the stack from top to bottom
            newStack.Reverse();

            return newStack;
        }

        static Dictionary<int, string> s_StackTitles;
        /// <summary>
        /// Method that gives cached string title for a given Enum value.
        /// </summary>
        /// <param name="enumValue">Enum value.</param>
        /// <returns>Returns first character of enum value name.</returns>
        static string GetTitleFromEnum<T>(T enumValue) where T : Enum
        {
            if (s_StackTitles == null)
            {
                s_StackTitles = new Dictionary<int, string>();
                foreach (var enumItem in (T[]) Enum.GetValues(typeof(T)))
                {
                    s_StackTitles.Add(enumItem.GetHashCode(), enumItem.ToString().Substring(0, 1).ToUpper());
                }
            }

            return s_StackTitles[enumValue.GetHashCode()];
        }
    }
}