using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(StringStringDictionary))]
[CustomPropertyDrawer(typeof(ColorStringDictionary))]
[CustomPropertyDrawer(typeof(StringColorDictionary))]
[CustomPropertyDrawer(typeof(StringMyClassDictionary))]
public class AnySerializableDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer {}
