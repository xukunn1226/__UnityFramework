using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IEnumeratorTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Person[] peopleArray = new Person[8]
        {
            new Person("John", "Smith"),
            new Person("Jim", "Johnson"),
            new Person("Sue", "Rabon"),
            new Person("Sue2", "Rabon"),
            new Person("Sue3", "Rabon"),
            new Person("Sue4", "Rabon"),
            new Person("Sue5", "Rabon"),
            new Person("Sue6", "Rabon")
        };

        People peopleList = new People(peopleArray);
        UnityEngine.Profiling.Profiler.BeginSample("11111111111");

        foreach (Person p in peopleList)
        {
            //int ii = 0;
        }

        IEnumerator e = peopleList.GetEnumerator();
        while(e.MoveNext())
        {
            //Debug.Log(((Person)e.Current).lastName);
        }
        UnityEngine.Profiling.Profiler.EndSample();

        UnityEngine.Profiling.Profiler.BeginSample("222222222");
        e.Reset();
        while (e.MoveNext())
        {
            //Debug.Log(((Person)e.Current).lastName);
        }
        UnityEngine.Profiling.Profiler.EndSample();
    }
}

// Simple business object.
public class Person
{
    public Person(string fName, string lName)
    {
        this.firstName = fName;
        this.lastName = lName;
    }

    public string firstName;
    public string lastName;
}

// Collection of Person objects. This class
// implements IEnumerable so that it can be used
// with ForEach syntax.
public class People : IEnumerable
{
    private Person[] _people;
    public People(Person[] pArray)
    {
        _people = new Person[pArray.Length];

        for (int i = 0; i < pArray.Length; i++)
        {
            _people[i] = pArray[i];
        }
    }

    // Implementation for the GetEnumerator method.
    IEnumerator IEnumerable.GetEnumerator()
    {
        return (IEnumerator)GetEnumerator();
    }

    public PeopleEnum GetEnumerator()
    {
        return new PeopleEnum(_people);
    }
}

// When you implement IEnumerable, you must also implement IEnumerator.
public class PeopleEnum : IEnumerator
{
    public Person[] _people;

    // Enumerators are positioned before the first element
    // until the first MoveNext() call.
    int position = -1;

    public PeopleEnum(Person[] list)
    {
        _people = list;
    }

    public bool MoveNext()
    {
        position++;
        return (position < _people.Length);
    }

    public void Reset()
    {
        position = -1;
    }

    object IEnumerator.Current
    {
        get
        {
            return Current;
        }
    }

    public Person Current
    {
        get
        {
            try
            {
                return _people[position];
            }
            catch (IndexOutOfRangeException)
            {
                throw new InvalidOperationException();
            }
        }
    }
}
