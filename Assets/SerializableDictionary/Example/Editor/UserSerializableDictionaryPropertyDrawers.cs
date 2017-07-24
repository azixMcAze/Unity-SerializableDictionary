using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(StringStringDictionary))]
[CustomPropertyDrawer(typeof(ObjectColorDictionary))]
public class AnySingleLineSerializableDictionaryPropertyDrawer : SingleLineSerializableDictionaryPropertyDrawer {}

[CustomPropertyDrawer(typeof(QuaternionMyClassDictionary))]
public class AnyDoubleLineSerializableDictionaryPropertyDrawer : DoubleLineSerializableDictionaryPropertyDrawer {}
