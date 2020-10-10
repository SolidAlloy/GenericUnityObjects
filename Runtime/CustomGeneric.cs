﻿namespace GenericScriptableObjects
{
    using System;
    using UnityEngine;

    public class CustomGeneric<T> : ScriptableObject<T>
    {
        public T VariableTypeField;

        public static ScriptableObject<T> Create()
        {
            return Create(typeof(CustomGeneric<>));
        }
    }
}