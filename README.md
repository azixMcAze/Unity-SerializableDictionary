# SerializableDictionary
A serializable dictionary class for Unity.

Unity cannot serialize standard dictionaries. This means that they won't show or be edited in the inspector
and they won't be instantiated at startup. A classic workaround is to store the keys and values in separate arrays
and construct the dictionary at startup.

This project provides a generic dictionary class and its custom property drawer that solves this problem.

![screenshot 1](http://azixmcaze.github.io/files/SerializableDictionary_screenshot1.png)

## Features

- It inherits from `Dictionary<TKey, TValue>`
- It implements a `CopyFrom(IDictionary<TKey, TValue>)` method to help assign values from regular dictionaries
- You can use any serializable type by unity as key or value. 
- It can be edited in the inspector without having to implement custom editors or property drawers.
- The inspector will handle invalid dictionary keys such as duplicated or `null` keys and warn the user that data loss can occur if the keys are not fixed.
    
    ![screenshot 2 ](http://azixmcaze.github.io/files/SerializableDictionary_screenshot2.png)

    ![screenshot 3](http://azixmcaze.github.io/files/SerializableDictionary_screenshot3.png)

## Usage

As Unity is unable to serialize generic types, create a derived class for each `SerializedDictionary` specialization you want.
```csharp
[Serializable]
public class StringStringDictionary : SerializableDictionary<string, string> {}

[Serializable]
public class MyScriptColorDictionary : SerializableDictionary<MyScript, Color> {}
```

Declare the custom property drawer for these new types by adding the `CustomPropertyDrawer` attribute to the `SerializableDictionaryPropertyDrawer` class or of one of its derived class.

```csharp
[CustomPropertyDrawer(typeof(StringStringDictionary))]
[CustomPropertyDrawer(typeof(MyScriptColorDictionary))]
public class AnySerializableDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer {}
```

It is recommended to create one derivate class in a separate file and add the attributes to this class instead of modifying the original `SerializableDictionaryPropertyDrawer` class.
You can use the same class for all your `SerializableDictionary` specializations, there is no need to create a new one for each specialization.

Add the dictionaries to your scripts and access them directly of through a property.

```csharp
public StringStringDictionary m_testDictionary1;

[SerializeField]
MyScriptColorDictionary m_testDictionary2;
public IDictionary<MyScript, Color> TestDictionary2
{
    get { return m_testDictionary2; }
    set { m_testDictionary2.CopyFrom (value); }
}
```