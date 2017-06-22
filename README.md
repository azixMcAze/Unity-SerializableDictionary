# SerializableDictionary
A serializable dictionary class for Unity.

Unity cannot serialize standard dictionaries. This means that they won't show or be edited in the inspector
and they won't be instantiated at startup. A classic workaround is to store the keys and values in separate arrays
and construct the dictionary at startup.

This project provides a generic dictionary class and its custom property drawer that solves this problem.

![General screenshot](http://azixmcaze.github.io/files/SerializableDictionary_screenshot1.png)

## Features

- It inherits from `Dictionary<TKey, TValue>`
- It implements a `CopyFrom(IDictionary<TKey, TValue>)` method to help assign values from regular dictionaries
- You can use any serializable type by unity as key or value. 
- It can be edited in the inspector without having to implement custom editors or property drawers.
- The inspector will handle invalid dictionary keys such as duplicated or `null` keys and warn the user that data loss can occur if the keys are not fixed.
    
    ![Conflicting keys screenshot](http://azixmcaze.github.io/files/SerializableDictionary_screenshot2.png)

    ![Null key screenshot](http://azixmcaze.github.io/files/SerializableDictionary_screenshot3.png)

## Limitations
- A derived class has to be created for each specialization of `SerializableDictionary`
- Using complex types as keys or values (like a `Quaternion` or any serializable class) result in non-optimal display in the inspector

    ![Complex type screenshot](http://azixmcaze.github.io/files/SerializableDictionary_screenshot4.png)
- Multiple editing of scripts using `SerializableDictionaries` in the inspector is not supported. The inspector will show the dictionnaries but data loss is likely to occur

## Usage

Copy these files in your project:
- `Assets/`
    - `Scripts/`
        - `SerializableDictionary.cs`
        - `UserSerializableDictionaryPropertyDrawer.cs` (optional)
    - `Editor/`
        - `DebugUtilsEditor.cs`
        - `SerializableDictionaryPropertyDrawer.cs`
        - `UserSerializableDictionaries.cs` (optional)

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
The dictionaries can be accessed through a property of type `IDictionary<TKey, TValue>` for better encapsulation.

```csharp
public StringStringDictionary m_myDictionary1;

[SerializeField]
MyScriptColorDictionary m_myDictionary2;
public IDictionary<MyScript, Color> MyDictionary2
{
    get { return m_myDictionary2; }
    set { m_myDictionary2.CopyFrom (value); }
}
```

The `CopyFrom(value)` method clears the `m_myDictionary2` dictionary and adds to it each of content of the `value` dictionary,  effectively copying `value` into `m_myDictionary2`.
