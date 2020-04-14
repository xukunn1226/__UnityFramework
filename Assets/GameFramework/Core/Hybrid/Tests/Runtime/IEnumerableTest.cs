using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IEnumerableTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    private void Update()
    {
        //IterListBox();

        IterGraphicCards();

        //IterCustomerList();
    }

    void IterListBox()
    {
        ListBoxTest lbt = new ListBoxTest("Hello", "World");

        //添加新的字符串  
        lbt.Add("Who");
        lbt.Add("Is");
        lbt.Add("Douglas");
        lbt.Add("Adams");

        //测试访问  
        string subst = "Universe";
        lbt[1] = subst;

        UnityEngine.Profiling.Profiler.BeginSample("11111111111");
        //访问所有的字符串  
        foreach (string s in lbt)
        {
            //Debug.Log($"{s}");
        }
        UnityEngine.Profiling.Profiler.EndSample();
    }

    void IterGraphicCards()
    {
        MyGraphicCards myGraphicCardList = new MyGraphicCards();
        myGraphicCardList.AddModel("GeForce 8800 Ultra");
        myGraphicCardList.AddModel("Radeon HD 5870");
        myGraphicCardList.AddModel("GeForce GTX 780 Ti");
        myGraphicCardList.AddModel("Radeon R9 Fury X");

        UnityEngine.Profiling.Profiler.BeginSample("11111111111");
        foreach (var myModel in myGraphicCardList)
        {
            //Debug.Log(myModel);
        }
        UnityEngine.Profiling.Profiler.EndSample();

        UnityEngine.Profiling.Profiler.BeginSample("222222222222");
        IEnumerable<string> e = myGraphicCardList.Reverse();
        foreach (var myModel in e)
        {
            //Debug.Log(myModel);
        }
        UnityEngine.Profiling.Profiler.EndSample();

        UnityEngine.Profiling.Profiler.BeginSample("333333333333");
        e = myGraphicCardList.Subset(2, 2);
        foreach (var myModel in e)
        {
            //Debug.Log(myModel);
        }
        UnityEngine.Profiling.Profiler.EndSample();

        UnityEngine.Profiling.Profiler.BeginSample("4444444444444");                // no gc
        for (int i = 0; i < myGraphicCardList.Models.Count; ++i)
        {
            string s = myGraphicCardList.Models[i];
        }
        UnityEngine.Profiling.Profiler.EndSample();
    }

    void IterCustomerList()
    {        
        CustomerList custList = new CustomerList();
        UnityEngine.Profiling.Profiler.BeginSample("11111111111");
        foreach (Customer cust in custList)
        {
            //Debug.Log($"cust.name: {cust.Name}");
        }
        UnityEngine.Profiling.Profiler.EndSample();

        UnityEngine.Profiling.Profiler.BeginSample("222222222222");
        foreach (Customer cust in custList)
        {
            //Debug.Log($"------cust.name: {cust.Name}");
        }
        UnityEngine.Profiling.Profiler.EndSample();
    }
}

public class Customer
{
    public string Name { get; set; }
    public string City { get; set; }
    public long Mobile { get; set; }
    public double Amount { get; set; }
}

public class CustomerList : IEnumerable
{
    Customer[] customers = new Customer[4];
    public CustomerList()
    {
        customers[0] = new Customer { Name = "Bijay Thapa", City = "LA", Mobile = 9841639665, Amount = 89.45 };
        customers[1] = new Customer { Name = "Jack", City = "NYC", Mobile = 9175869002, Amount = 426.00 };
        customers[2] = new Customer { Name = "Anil min", City = "Kathmandu", Mobile = 9173694005, Amount = 5896.20 };
        customers[3] = new Customer { Name = "Jim sin", City = "Delhi", Mobile = 64214556002, Amount = 596.20 };
    }

    public int Count()
    {
        return customers.Length;
    }
    public Customer this[int index]
    {
        get
        {
            return customers[index];
        }
    }
    public IEnumerator GetEnumerator()
    {
        return customers.GetEnumerator(); // we can do this but we are going to make our own Enumerator
        //return new CustomerEnumerator(this);
    }
}

public class MyGraphicCards
{
    public List<string> Models = new List<string>();

    public void AddModel(string newModel)
    {
        Models.Add(newModel);
    }

    public int Length()
    {
        return Models.Count;
    }

    public IEnumerator GetEnumerator()
    {
        for (int i = 0; i < Models.Count; i++)
        {
            yield return Models[i];
        }

        //return Models.GetEnumerator();
    }

    //public IEnumerator<string> GetEnumerator()//返回foreach方法需要的IEnumerator<T>对象
    //{
    //    for (int i = 0; i < Models.Count; i++)
    //    {
    //        yield return Models[i];
    //    }
    //}

    public IEnumerable<string> Reverse()//Reverse是实现逆向遍历List的方法，返回的IEnumerable类型会自动实现GetEnumerator方法
    {
        for (int i = Models.Count - 1; i >= 0; i--)
        {
            yield return Models[i];
        }
    }

    public IEnumerable<string> Subset(int index, int length)//Subset方法实现遍历特定List中的子集
    {
        for (int i = index; i < index + length; i++)
        {
            yield return Models[i];
        }
    }
}


public class LinkedListNode
{
    public LinkedListNode(object value)
    {
        Value = value;
    }
    public object Value { get; }//获取当前元素
    public LinkedListNode Next { get; internal set; } //返回下一个元素
    public LinkedListNode Prev { get; internal set; }//返回上一个元素
}

public class LinkedList : IEnumerable
{
    public LinkedListNode First { get; private set; }
    public LinkedListNode Last { get; private set; }
    public LinkedListNode AddLast(object node) //添加元素
    {
        var newNode = new LinkedListNode(node); //定义集合类
        if (First == null)
        {
            First = newNode;
            Last = First;
        }
        else
        {
            Last.Next = newNode;
            Last = newNode;
        }
        return newNode;
    }
    public IEnumerable GetEnumerator() //IEnumerable接口的实现
    {
        LinkedListNode current = First;
        while (current != null)
        {
            yield return current.Value;
            //  yield return 这是迭代替返回集合的一个元素,并移动到下一个元素上.这种方式也能强类型的list<T>
            //相关属性和类名给出<T>就好了，不过很不容易理解，因为在这里就是迭代，内容抽象。
            current = current.Next;
        }
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new System.NotImplementedException();
    }
}



public class ListBoxTest : IEnumerable<string>
{
    private string[] strings;
    private int ctr = 0;

    #region IEnumerable<string> 成员  
    //可枚举的类可以返回枚举  
    public IEnumerator<string> GetEnumerator()
    {
        foreach (string s in strings)
        {
            yield return s;
        }
    }

    #endregion

    #region IEnumerable 成员  
    //显式实现接口  
    IEnumerator IEnumerable.GetEnumerator()
    {
        //return GetEnumerator();
        throw new System.NotImplementedException();
    }

    #endregion

    //用字符串初始化列表框  
    public ListBoxTest(params string[] initialStrings)
    {
        //为字符串分配内存空间  
        strings = new string[8];
        //复制传递给构造方法的字符串  
        foreach (string s in initialStrings)
        {
            strings[ctr++] = s;
        }
    }

    //在列表框最后添加一个字符串  
    public void Add(string theString)
    {
        strings[ctr] = theString;
        ctr++;
    }

    //允许数组式的访问  
    public string this[int index]
    {
        get
        {
            if (index < 0 || index >= strings.Length)
            {
                //处理不良索引  
            }
            return strings[index];
        }
        set
        {
            strings[index] = value;
        }
    }

    //发布拥有的字符串数  
    public int GetNumEntries()
    {
        return ctr;
    }
}