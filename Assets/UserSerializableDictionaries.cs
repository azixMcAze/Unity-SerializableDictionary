using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StringStringDictionary : SerializableDictionary<string, string> {}
[System.Serializable]
public class StringStringDictionary2 : StringStringDictionary {}
[System.Serializable]
public class StringColorDictionary : SerializableDictionary<string, Color> {}
[System.Serializable]
public class ColorStringDictionary : SerializableDictionary<Color, string> {}
[System.Serializable]
public class VectorStringDictionary : SerializableDictionary<Vector3, string> {}
[System.Serializable]
public class StringGradientDictionary : SerializableDictionary<string, Gradient> {}
[System.Serializable]
public class StringMyClassDictionary : SerializableDictionary<string, DictionaryTest.MyClass> {}
