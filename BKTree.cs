using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PYcheck
{
    public delegate int DistanceFunction(object o1, object o2);  

    public class Node<T> 
    {
        public T item;
        public Dictionary<int, Node<T>> children;

        public Node(T item) 
        {
            this.item = item;
            this.children = new Dictionary<int, Node<T>>();
        }
    }   

    class BKTree<T>
    {
        private Node<T> rootNode;
        private DistanceFunction distanceFunction;
        private int length;
        private int modCount;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="root">根节点的值</param>
        /// <param name="distanceFunction">默认使用编辑距离</param>
        public BKTree(T root, DistanceFunction distanceFunction = null)
        {
            if (distanceFunction == null)
            {
                this.distanceFunction = this.LevenshteinDistance;
            }
            else
            {
                this.distanceFunction = distanceFunction;
            }

            rootNode = new Node<T>(root);            
            length = 0;
            modCount = 0;
        }

        public bool AddNode(T t)
        {
            if (t == null)
                throw new NullReferenceException();

            if (rootNode == null)
            {
                rootNode = new Node<T>(t);
                length = 1;
                modCount++; // Modified tree by adding root.
                return true;
            }

            Node<T> parentNode = rootNode;
            int distance;
            while ((distance = distanceFunction(parentNode.item, t)) != 0 || !t.Equals(parentNode.item))
            {
                try
                {
                    Node<T> childNode = parentNode.children[distance];
                    parentNode = childNode;
                }
                catch(KeyNotFoundException ex)
                {
                    parentNode.children.Add(distance, new Node<T>(t));
                    length++;
                    modCount++;
                    return true;
                }                
            }

            return false;
        }

        public HashSet<T> Search(T t, int radius)
        {
            HashSet<T> res = new HashSet<T>();
            if(rootNode != null)
            {
                Query(rootNode, t, radius, ref res);
            }
            return res;
        }

        private void Query(Node<T> node,T t, int radius, ref HashSet<T> res)
        {
            int distance = this.distanceFunction(node.item, t);
            if (distance <= radius)
            {
                res.Add(node.item);
            }
            for(int i = Math.Max(distance - radius, 0); i <= distance + radius; i++)
            {
               try
                {
                    Node<T> child = node.children[i];
                    Query(child, t, radius, ref res);
                }                
                catch (KeyNotFoundException ex)
                {
                    continue;
                }
        }
        }

        /// <summary>
        /// 计算两个字符串的编辑距离
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        private int LevenshteinDistance(object obj1, object obj2)
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
