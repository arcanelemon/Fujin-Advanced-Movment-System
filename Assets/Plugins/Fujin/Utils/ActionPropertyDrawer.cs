using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections.Generic;
using Action = Fujin.Utils.Action;

[CustomPropertyDrawer(typeof(Action))]
public class ActionPropertyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => 0;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty targetProperty = property.FindPropertyRelative("target");
        SerializedProperty methodNameProperty = property.FindPropertyRelative("method");
        SerializedProperty candidateNamesProperty = property.FindPropertyRelative("candidates");
        SerializedProperty indexProperty = property.FindPropertyRelative("index");

        GUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(EditorGUIUtility.labelWidth));

        #region Target

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(targetProperty, GUIContent.none);
        GUILayout.EndHorizontal();

        if (targetProperty.objectReferenceValue == null)
            return;

        #endregion Target

        #region Method
        string[] methodCandidateNames;
        GUILayout.BeginHorizontal();
        GUILayout.Label("", GUILayout.Width(EditorGUIUtility.labelWidth));
        if (targetProperty.objectReferenceValue != null)
        {

            // polulate method candidate names
            if (EditorGUI.EndChangeCheck())
            {
                methodCandidateNames = RepopulateCandidateList(targetProperty, candidateNamesProperty, indexProperty);
            }
            else
            {
                methodCandidateNames = new string[candidateNamesProperty.arraySize];

                int i = 0;
                foreach (SerializedProperty element in candidateNamesProperty)
                {
                    methodCandidateNames[i++] = element.stringValue;
                }
            }

            indexProperty.intValue = EditorGUILayout.Popup(indexProperty.intValue, methodCandidateNames);
            methodNameProperty.stringValue = methodCandidateNames[indexProperty.intValue];

        } else
        {
            methodCandidateNames = new string[0];
            indexProperty.intValue = EditorGUILayout.Popup(indexProperty.intValue, methodCandidateNames);
            methodNameProperty.stringValue = "";
        }
        GUILayout.EndHorizontal();

        #endregion Method
    }

    #region Action Utils

    string[] RepopulateCandidateList(SerializedProperty targetProperty, SerializedProperty candidateNamesProperty, SerializedProperty indexProperty)
    {
        Type type = ((MonoScript)targetProperty.objectReferenceValue).GetClass();
        string[] candidateNames;

        List<MethodInfo> methodList = new List<MethodInfo>();
        MethodInfo[] methods = type.GetMethods();
        foreach (MethodInfo m in methods)
                methodList.Add(m);

        // clear/resize/initialize storage containers
        candidateNamesProperty.ClearArray();
        candidateNamesProperty.arraySize = methodList.Count;
        candidateNames = new string[methodList.Count];

        // assign storage containers
        int i = 0;
        foreach (SerializedProperty element in candidateNamesProperty)
        {
            MethodInfo candidate = methodList[i];
            element.stringValue = candidateNames[i] = candidate.Name + FormatParameters(candidate.GetParameters());
            i++;
        }

        // reset popup index
        indexProperty.intValue = 0;

        return candidateNames;
    }

    string FormatParameters(ParameterInfo[] parameters)
    {
        if (parameters.Length == 0)
            return "";
        string format = " (";
        foreach (ParameterInfo parameter in parameters)
        {
            if (format.Length > 2)
                format += ", ";
            string type = parameter.ParameterType.ToString();

            switch (type)
            {
                case "System.Int32":
                    type = "int";
                    break;
                case "System.Boolean":
                    type = "bool";
                    break;
                case "System.Double":
                    type = "double";
                    break;
                case "System.Single":
                    type = "float";
                    break;
                case "System.String":
                    type = "string";
                    break;
            }

            format += type;
        }

        return format + ")";
    }

    #endregion Action Utils
}