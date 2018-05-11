using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace PYcheck
{
    public class CPYCheck
    {
        private const string BLANK = " ";
        private const string NON_CHAR_PATTERN = "[^A-Z|a-z|0-9]";
        private const string CHAR_PATTERN = "[a-z]";
        private const string DEFAULT_DICT_NAME = "pinyin_dataset.txt";
        private Dictionary<char, List<string>> data_set;
        private List<string> word_list; // 存储拼音分词后的结果
        private BKTree<string> bk_tree; 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dict_path">拼音字典文件路径</param>
        public CPYCheck(string nameDict_path = null)
        {            
            data_set = new Dictionary<char, List<string>>();
            bk_tree = new BKTree<string>("yue");
            word_list = new List<string>();

            if (File.Exists(nameDict_path))
            {
                DateTime beforDT = System.DateTime.Now;

                CreateDataSet(nameDict_path);
                //CreatePYDataSet(nameDict_path);

                DateTime afterDT = System.DateTime.Now;
                TimeSpan ts = afterDT.Subtract(beforDT);
                Console.WriteLine("CreateDataSet spend {0} ms", ts.TotalMilliseconds); 
            }
            else
            {
                throw new Exception(string.Format("the file {0} not find!", nameDict_path));
            }
            
        }        

        /// <summary>
        /// 构建姓名的BKTree
        /// </summary>
        /// <param name="nameDict_path">姓名拼音字典</param>
        private void CreateDataSet(string nameDict_path)
        {            
            StreamReader sr = new StreamReader(nameDict_path, Encoding.Default);
            String line = null;            

            while ((line = sr.ReadLine()) != null)
            {
                this.bk_tree.AddNode(line.Trim());
                if (!data_set.ContainsKey(line[0]))
                {
                    data_set[line[0]] = new List<string> { line.Trim() };
                }
                else
                {
                    data_set[line[0]].Add(line.Trim());
                }                   
            }
            sr.Close();            
        }        

        /// <summary>
        /// 未切分拼音只修正一位拼写错误
        /// </summary>
        /// <param name="name_str">待检测拼音</param>
        /// <returns></returns>
        public List<string> NameCheck(string name_str)
        {
            string pname = null;            
            Regex rgx = new Regex(NON_CHAR_PATTERN);

            //用空格替换所有非字母或数字字符
            name_str = rgx.Replace(name_str, BLANK);
            //去除首尾空格
            name_str = name_str.Trim();
           // pname = name_str;
            name_str = name_str.ToLower();
            //认为姓名最少由两个字母组成
            if (name_str.Length < 2)
                return null;

            string[] name_list = name_str.Split(' ');
            List<string> names = new List<string>();

            if (name_list.Length > 1)
            {
                names = GetCorrectName(name_list);
            }
            else
            {
                names = GetCorrectName(name_str);
                if(names.Count == 0)
                {
                    names.Add(name_str);
                }
            }

            return names;
        }

        /// <summary>
        /// 返回纠错后姓名列表
        /// </summary>
        /// <param name="name">姓名拼音，中间不含空格，形如:XxxXxxXxxx</param>
        /// <returns></returns>
        private List<string> GetCorrectName(string name)
        {
            List<string> correct_pname = new List<string>();            
            Regex rgx = new Regex(CHAR_PATTERN);
            for (int index = 0; index < name.Length; index++ )
            {                
                if(!rgx.IsMatch(name[index].ToString()) || !PreSufWordTest(name,index))
                {
                    HashSet<string> pre_correct = new HashSet<string>();
                    HashSet<string> suffix_correct = new HashSet<string>();

                    if(index > 0)
                    {
                        pre_correct = this.bk_tree.Search(name.Substring(index - 1, 2), 1);
                    }

                    if(index < name.Length - 1)
                    {
                        suffix_correct = this.bk_tree.Search(name.Substring(index, 2), 1);
                    }

                    //前缀和后缀修正做并操作
                    pre_correct.UnionWith(suffix_correct);

                    foreach(string correct_word in pre_correct)
                    {
                        if(correct_word.Length > 1)
                        {
                            string new_name = "";
                            if(index > 0 && correct_word[0] == name[index - 1])
                            {
                                //修正前缀匹配到的结果
                                new_name = name.Substring(0, index) + correct_word[1].ToString() + name.Substring(index + 1);
                            }
                            else if(index < name.Length - 1 && correct_word[1] == name[index + 1])
                            {
                                //修正后缀匹配到的结果
                                new_name = name.Substring(0, index) + correct_word[0].ToString() + name.Substring(index + 1);
                            }

                            //拼音规则检测
                            List<string> wordSplit = PYSplit(new_name);
                            bool res = true;
                            if (wordSplit != null)
                            {
                                foreach (string word in wordSplit)
                                    res = res && WordTest(word);
                                this.word_list.Clear();
                                if (res == true)
                                {
                                    correct_pname.Add(new_name);
                                }
                            }                            
                        }
                    }
                }
            }
            return correct_pname;
        }

        /// <summary>
        /// 测试字符串test_str[index -1 : index] 或者test_str[index : index + 1] 是否是合法拼音，有一个合法即返回true
        /// </summary>
        /// <param name="test_str">待测字符串</param>
        /// <param name="index">当前序号</param>
        /// <returns></returns>
        private bool PreSufWordTest(string test_str, int index)
        {
            bool res = true;
            if(index == 0)
            {
                //只测试后缀匹配，即 test_str[index : index + 1] 
                res = WordTest(test_str.Substring(index, 2));
            }
            else if(index == test_str.Length - 1)
            {
                //只测试前缀匹配，即 test_str[index -1 : index]
                res = WordTest(test_str.Substring(index - 1, 2));
            }
            else
            {
                res = WordTest(test_str.Substring(index, 2)) || WordTest(test_str.Substring(index - 1, 2));
            }
            return res;
        }

        /// <summary>
        /// 返回纠错后姓名列表
        /// </summary>
        /// <param name_list="name_list">分割后的拼音数组，形如:xx xxx xx</param>
        /// <returns></returns>
        private List<string> GetCorrectName(string[] name_list)
        {
            Dictionary<int, List<string>> correct_dict = new Dictionary<int, List<string>>();
            for(int i = 0; i < name_list.Length; i++)
            {
                bool is_normal = WordTest(name_list[i]);
                if (!is_normal)
                {
                    //拼写纠错
                    HashSet<string> correct_list = bk_tree.Search(name_list[i], 1);
                    correct_dict[i] = correct_list.ToList();
                }
                else
                {
                    correct_dict[i] = new List<string>{name_list[i]};
                }
            }

            List<string> correct_pname = new List<string>();
            string name = "";
            GetCombination(correct_dict, 0, ref name, ref correct_pname);
            return correct_pname;
        }

        public void GetCombination(Dictionary<int, List<string>> correct_dict, int key, ref string name, ref List<string> name_list)
        {
            string name_copy = name;
            for (int index = 0; index < correct_dict[key].Count; index++)
            {
                name += " " + correct_dict[key][index];
                if (correct_dict.ContainsKey(key + 1) == true)
                {
                    GetCombination(correct_dict, key + 1, ref name, ref name_list);
                    name = name_copy;
                }
                else
                {
                    name_list.Add(name.Trim());
                    name = name_copy;                    
                }
            }
            return;
        }
    
        // 拼音分词,前向最大匹配法算法
        // 返回分词后的拼音数组
        public List<string> PYSplit(string py_str)
        {           
            const int MAX_WORD_LEN = 6; // 单词的最大长度     
            bool flag = false;       
            string s2 = py_str.Length > MAX_WORD_LEN ? py_str.Substring(0, MAX_WORD_LEN) : py_str;
            try
            {
                List<string> word_list = this.data_set[s2[0]];

                for (int index = s2.Length; index > -1; index--)
                {
                    if (word_list.Contains(s2.Substring(0, index)))
                    {
                        this.word_list.Add(s2.Substring(0, index));
                        flag = true;
                        if (index < py_str.Length)
                        {
                            this.PYSplit(py_str.Substring(index, py_str.Length - index));
                        }
                        return this.word_list;
                    }
                }
                if (flag == false)
                    this.word_list.Add(py_str);
                return this.word_list;
            }
            catch(Exception ex)
            {
                this.word_list.Add(py_str);
                return this.word_list;
            }           
        }        

        private bool WordTest(string word)
        {
            bool res = true;
            word = word.ToLower();
            try
            {
                res = data_set[word[0]].Contains(word);
            }
            catch(Exception ex)
            {
                res = false;
            }
            return res;
        }       
    }
}
