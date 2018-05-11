using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PYcheck
{
    class Program
    {
        static void Main(string[] args)
        {
            //BKTree<string> bk_tree = new BKTree<string>("yue");
            //StreamReader sr = new StreamReader(@"E:\code\MechineLeaning\OCR\PYcheck\pinyin_dataset.txt", Encoding.Default);
            //String line;
            //while ((line = sr.ReadLine()) != null)
            //{
            //    bk_tree.AddNode(line.Trim());
            //}
            //HashSet<string> res = bk_tree.Search("van", 1);
            //foreach (string word in res)
            //    Console.WriteLine(word);

            //int d = LevenshteinDistance("van", "wan");
            
            string name_path = @"D:\Code\OCR\PYcheck\PYcheck\name_data.txt";
            CPYCheck check = new CPYCheck(name_path);
            List<string> res = check.NameCheck("yan a1 neng");

            foreach (string word in res)
                Console.WriteLine(word);

        }

        /// <summary>
        /// 计算两个字符串的编辑距离
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        static int LevenshteinDistance(object obj1, object obj2)
        {
            string first = obj1 as string;
            string second = obj2 as string;

            if (first.Length > second.Length)
            {
                string temp = first;
                first = second;
                second = temp;
            }
            if (first.Length == 0)
                return second.Length;

            if (second.Length == 0)
                return first.Length;

            int first_length = first.Length + 1;
            int second_length = second.Length + 1;

            int[,] distance_matrix = new int[first_length, second_length];

            for (int i = 0; i < second_length; i++)
            {
                distance_matrix[0, i] = i;
            }

            for (int j = 1; j < first_length; j++)
            {
                distance_matrix[j, 0] = j;
            }

            for (int i = 1; i < first_length; i++)
            {
                for (int j = 1; j < second_length; j++)
                {
                    int deletion = distance_matrix[i - 1, j] + 1;
                    int insertion = distance_matrix[i, j - 1] + 1;
                    int substitution = distance_matrix[i - 1, j - 1];
                    if (first[i - 1] != second[j - 1])
                        substitution += 1;
                    int temp = Math.Min(insertion, deletion);
                    distance_matrix[i, j] = Math.Min(temp, substitution);
                }
            }

            return distance_matrix[first_length - 1, second_length - 1];
        }
    }
}

    
        
