using System;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;
using MonoScript = UnityEditor.MonoScript;

namespace Fujin.Utils
{
    [System.Serializable]
    public class Action
    {
        public MonoScript target;
        public string method;

        // inspector cache
        public string[] candidates = { };
        public Type[] candidateTypes = { };
        public int index;
        MethodInfo Method;

        public Action() { }

        public Action(MonoScript target, string method) 
        {
            SetListener(target, method);
        }

        public void SetListener(MonoScript target, MethodInfo method)
        {
            this.target = target;
            Method = method;
        }

        public void SetListener(MonoScript target, string method)
        {
            string[] parameters = method.Split(new char[] { '(', ')', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            Type[] args = new Type[parameters.Length - 1];

            int i = 0;
            foreach (string param in parameters)
            {
                if (i == 0)
                {
                    i++;
                    continue;
                }

                Type arg;
                switch (param)
                {
                    case "int":
                        arg = typeof(int);
                        break;
                    case "bool":
                        arg = typeof(bool);
                        break;
                    case "double":
                        arg = typeof(double);
                        break;
                    case "float":
                        arg = typeof(float);
                        break;
                    case "string":
                        arg = typeof(string);
                        break;
                    default:
                        arg = Type.GetType(param);
                        break;
                }

                args[i - 1] = arg;
            }

            SetListener(target, target.GetClass().GetMethod(parameters[0], args));
        }

        public void RemoveListener()
        {
            target = null;
            method = null;
            Method = null;
        }

        public void Invoke(object[] args)
        {
            Method.Invoke(target, args);
        }

        public object Invoke(GameObject gameObject, params object[] args)
        {

            Object obj;

            if (!target || method == null || !(obj = gameObject.GetComponent(target.GetClass().ToString())))
                return null;

            if (Method == null) // init
                SetListener(target, method);

            return Method.Invoke(obj, args);
        }
    }
}
